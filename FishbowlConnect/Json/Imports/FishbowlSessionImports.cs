using CsvHelper;
using CsvHelper.Configuration;
using FishbowlConnect.Exceptions;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Json;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Json.Imports;
using FishbowlConnect.QueryClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Returns the header rows in csv format for the specified import
        /// </summary>
        /// <param name="ImportType">CSV import name, in format 'Import' + CamelCase of import name</param>
        /// <returns>List of header rows, most will be 1, Sales order and Purchase order will be 2</returns>
        public async Task<List<string>> getImportHeaderRowAsync(string ImportType)
        {
            ImportHeaderRq importHeaderRq = new ImportHeaderRq();
            importHeaderRq.Type = ImportType;

            ImportHeaderRs importHeaderRs = await IssueJsonRequestAsync<ImportHeaderRs>(importHeaderRq);

            if (importHeaderRs.StatusCode != "1000")
            {
                throw new Exception(importHeaderRs.StatusCode + " - " + Utilities.StatusCodeMessage(importHeaderRs.StatusCode));
            }

            return importHeaderRs.Header.Row;
        }

        /// <summary>
        /// Adds inventory to Fishbowl through the import request
        /// </summary>
        /// <param name="PartNumber">Part number to add inventory for</param>
        /// <param name="Qty">How many pieces</param>
        /// <param name="LocationName">Location Name to add to</param>
        /// <param name="LocationGroup">Location group to add to</param>
        /// <param name="Note">Any note to record with the transaction</param>
        /// <param name="partTrackingInfo">List of part tracking items</param>
        /// <returns>Void</returns>
        public async Task AddInventoryImportAsync(string PartNumber, int Qty,
            string LocationName, string LocationGroup, string Note = "", IEnumerable<IPartTrackingFields> partTrackingInfo = null)
        {

            //validate
            if (String.IsNullOrEmpty(PartNumber))
            {
                throw new ArgumentNullException("Part Number is required");
            }
            if (String.IsNullOrEmpty(LocationName))
            {
                throw new ArgumentNullException("Location is required");
            }
            if (String.IsNullOrEmpty(LocationGroup))
            {
                throw new ArgumentNullException("Location Group is required");
            }
            if (Qty <= 0)
            {
                throw new ArgumentNullException("Qty to add must be higher than 0");
            }


            ////get headers
            string header = (await getImportHeaderRowAsync(ImportNameConsts.INVENTORY_ADD)).FirstOrDefault();
            //serialize to csv format


            if (String.IsNullOrEmpty(header))
            {
                throw new FishbowlException("Header row is not available");
            }

            Configuration configuration = new Configuration();
            DefaultClassMap<ImportAddInventory> custMap;
            int writeableFieldCount = 0;
            int headerFieldCount = 0;


            Dictionary<int, string> trackingIndexes = null;


            if (partTrackingInfo?.Count() > 0)
            {

                //we need to deserialize the header row to find out which index column our tracking field ends up in

                using (TextReader textReader = new StringReader(header))
                using (var csvHeader = new CsvReader(textReader))
                {
                    csvHeader.Read();
                    csvHeader.ReadHeader();

                    headerFieldCount = csvHeader.Context.HeaderRecord.Length;

                    trackingIndexes = new Dictionary<int, string>();


                    foreach (IPartTrackingFields item in partTrackingInfo)
                    {
                        if (!string.IsNullOrEmpty(item.TrackingLabel))
                        {

                            //get field index for each returned item and add to dictionary
                            try
                            {
                                trackingIndexes.Add(csvHeader.GetFieldIndex("Tracking-" + item.TrackingLabel), item.TrackingInfo);
                            }
                            catch (CsvHelper.MissingFieldException ex)
                            {
                                throw new ArgumentException("Tracking Name not found.", ex);
                            }
                        }
                    }


                    custMap = new DefaultClassMap<ImportAddInventory>();
                    custMap.AutoMap();


                    writeableFieldCount = custMap.MemberMaps.Where(m => m.Data.Ignore == false).Count(); //for add import, its all fields in the class Map

                    configuration.RegisterClassMap(custMap);
                }

            }

            StringBuilder sb = new StringBuilder();
            configuration.QuoteAllFields = true;
            List<Row> importRows = new List<Row>();

            using (TextWriter textWriter = new StringWriter(sb))
            using (var csvWriter = new CsvWriter(textWriter, configuration))
            {
                csvWriter.Context.Writer.NewLine = "";
                //List<string> importRows = new List<string>();



                //Write the new headers with only the primary tracking
                //2018.7 doesnt parse header row on import correctly so we are leaving it off, but we still need the full qty of fields
                //csvWriter.WriteHeader<ImportInventoryMove>();
                //csvWriter.NextRecord();
                //importRows.Add(new Row { RowField = sb.ToString() });
                //sb.Clear();

                string cost = null;
                try
                {
                    cost = await GetPartLastCost(PartNumber);
                }
                catch (Exception)
                {
                    cost = "0.00";
                }

                ImportAddInventory importAddInventory = new ImportAddInventory
                {
                    PartNumber = PartNumber,
                    Qty = Qty,
                    Cost = cost,
                    Date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                    Location = LocationGroup + "-" + LocationName,
                    Note = Note
                };

                csvWriter.WriteRecord(importAddInventory);

                //write the variable custom fields using the field indicies in our dictionary to define column placement
                //starts at the end of the regular import property fields and if the index matches then it writes the value
                for (int i = writeableFieldCount; i < headerFieldCount; i++)
                {
                    if (trackingIndexes.TryGetValue(i, out string value))
                    {
                        csvWriter.WriteField(value);
                    }
                    else
                    {
                        csvWriter.WriteField("");
                    }
                }

                csvWriter.Flush();
            }
            importRows.Add(new Row { RowField = new List<string> { sb.ToString() } });


            ImportRq importRq = new ImportRq();
            importRq.Type = ImportNameConsts.INVENTORY_ADD;
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);

        }


        /// <summary>
        /// Takes a class object and converts it to the CSV import version that FB requires and submits it to FB.
        /// Adds in missing API fields like phone, email, carrier name, and carrier service
        /// </summary>
        /// <param name="salesOrder">Sales Order Object</param>
        /// <returns></returns>
        public async Task ImportSalesOrderAsync(SalesOrder salesOrder)
        {
            if (salesOrder == null)
            {
                throw new ArgumentNullException("Sales order cannot be null");
            }

            //get headers
            List<string> headers = await getImportHeaderRowAsync(ImportNameConsts.SALES_ORDER_IMPORT);
            string soHeader = headers[0];
            string soItemHeader = headers[1];

            if (string.IsNullOrEmpty(soHeader))
            {
                throw new FishbowlException("Sales Order Header row is not available");
            }
            if (string.IsNullOrEmpty(soItemHeader))
            {
                throw new FishbowlException("Item Header row is not available");
            }

            Configuration configuration = new Configuration();
            DefaultClassMap<ImportSalesOrder> soMap;
            int writeableFieldCount = 0;
            int headerFieldCount = 0;

            Dictionary<int, string> salesOrderCustomFieldIndexes = null;


            //if (salesOrder.CustomField?.Count() > 0)
            //{

            //we need to deserialize the header row to find out which index column our custom field ends up in

            using (TextReader textReader = new StringReader(soHeader))
            using (var csvHeader = new CsvReader(textReader))
            {
                csvHeader.Read();
                csvHeader.ReadHeader();

                headerFieldCount = csvHeader.Context.HeaderRecord.Length;

                salesOrderCustomFieldIndexes = new Dictionary<int, string>();


                if (salesOrder?.CustomField != null)
                {

                    foreach (CustomField customField in salesOrder.CustomField)
                    {
                        if (!string.IsNullOrEmpty(customField.Name))
                        {

                            //get field index for each returned item and add to dictionary
                            try
                            {
                                salesOrderCustomFieldIndexes.Add(csvHeader.GetFieldIndex("CF-" + customField.Name), customField.Info);
                            }
                            catch (CsvHelper.MissingFieldException ex)
                            {
                                //throw new ArgumentException("Custom Field not found.", ex);
                            }
                        }
                    }
                }

                soMap = new DefaultClassMap<ImportSalesOrder>();
                soMap.AutoMap();


                writeableFieldCount = soMap.MemberMaps.Where(m => m.Data.Ignore == false).Count(); //for so header line import, its all fields in the class Map

                configuration.RegisterClassMap(soMap);
            }


            StringBuilder sb = new StringBuilder();
            configuration.QuoteAllFields = true;


            List<Row> importRows = new List<Row>();
            List<string> rowField = new List<string>();


            using (TextWriter textWriter = new StringWriter(sb))
            using (var csvWriter = new CsvWriter(textWriter, configuration))
            {
                csvWriter.Context.Writer.NewLine = "";

                ImportSalesOrder importSalesOrder = new ImportSalesOrder
                {
                    BillToAddress = salesOrder.BillTo?.AddressField,
                    BillToCity = salesOrder.BillTo?.City,
                    BillToCountry = salesOrder.BillTo?.Country,
                    BillToName = salesOrder.BillTo?.Name,
                    BillToState = salesOrder.BillTo?.State,
                    BillToZip = salesOrder.BillTo?.Zip,

                    ShipToAddress = salesOrder.Ship?.AddressField,
                    ShipToCity = salesOrder.Ship?.City,
                    ShipToCountry = salesOrder.Ship?.Country,
                    ShipToName = salesOrder.Ship?.Name,
                    ShipToState = salesOrder.Ship?.State,
                    ShipToZip = salesOrder.Ship?.Zip,

                    CarrierName = salesOrder.CarrierName == null ? salesOrder?.Carrier : salesOrder.CarrierName,
                    CarrierService = salesOrder.CarrierService,
                    CustomerContact = salesOrder.CustomerContact,
                    CustomerName = salesOrder.CustomerName,
                    Date = salesOrder.CreatedDate,
                    Email = salesOrder.Email,
                    Flag = "SO",
                    FOB = salesOrder.FOB,
                    LocationGroupName = salesOrder.LocationGroup,
                    Note = salesOrder.Note,
                    OrderDateScheduled = DateTime.Now.ToShortDateString(),
                    PaymentTerms = salesOrder.PaymentTerms,
                    Phone = salesOrder.Phone,
                    PONum = salesOrder.CustomerPO,
                    PriorityId = int.Parse(salesOrder.PriorityID),
                    QuickbooksClassName = salesOrder.QuickBooksClassName,
                    Salesman = salesOrder.Salesman,
                    ShippingTerms = salesOrder.ShippingTerms,
                    ShipToResidential = salesOrder.ResidentialFlag,
                    SONum = salesOrder.Number,
                    Status = int.Parse(salesOrder.Status),
                    TaxRateName = salesOrder.TaxRateName,
                    URL = salesOrder.URL,
                    VendorPONum = salesOrder.VendorPO
                };

                csvWriter.WriteRecord(importSalesOrder);

                //set the custom fields for the so
                //write the variable custom fields using the field indicies in our dictionary to define column placement
                //starts at the end of the regular import property fields and if the index matches then it writes the value
                for (int i = writeableFieldCount; i < headerFieldCount; i++)
                {
                    if (salesOrderCustomFieldIndexes.TryGetValue(i, out string value))
                    {
                        csvWriter.WriteField(value);
                    }
                    else
                    {
                        csvWriter.WriteField("");
                    }
                }

                csvWriter.Flush();
            }
            //move to end
            //importRows.Add(new Row { RowField = sb.ToString() });
            rowField.Add(sb.ToString());



            //loop through the sales order items and write them to the csv

            if (salesOrder.Items?.SalesOrderItem?.Count == 0)
            {
                throw new ArgumentNullException("Sales order must have items");
            }

            Configuration configurationItems = new Configuration();
            DefaultClassMap<ImportSalesOrderItem> soItemsMap;
            int writeableItemFieldCount = 0;
            int headerItemFieldCount = 0;

            Dictionary<int, string> salesOrderItemCustomFieldIndexes = null;

            //we need to deserialize the header row to find out which index column our custom field ends up in

            using (TextReader textReader = new StringReader(soItemHeader))
            using (var csvHeader = new CsvReader(textReader))
            {
                csvHeader.Read();
                csvHeader.ReadHeader();

                headerItemFieldCount = csvHeader.Context.HeaderRecord.Length;

                salesOrderItemCustomFieldIndexes = new Dictionary<int, string>();


                if (salesOrder.Items.SalesOrderItem[0].CustomField != null)
                {
                    foreach (CustomField customField in salesOrder.Items.SalesOrderItem[0].CustomField)
                    {
                        if (!string.IsNullOrEmpty(customField.Name))
                        {

                            //get field index for each returned item and add to dictionary
                            try
                            {
                                salesOrderItemCustomFieldIndexes.Add(csvHeader.GetFieldIndex("CFI-" + customField.Name), customField.Info);
                            }
                            catch (CsvHelper.MissingFieldException ex)
                            {
                                //throw new ArgumentException("Custom Field not found.", ex);
                            }
                        }
                    }

                }
                soItemsMap = new DefaultClassMap<ImportSalesOrderItem>();
                soItemsMap.AutoMap();


                writeableItemFieldCount = soItemsMap.MemberMaps.Where(m => m.Data.Ignore == false).Count(); //for so header line import, its all fields in the class Map

                configurationItems.RegisterClassMap(soItemsMap);

            }



            StringBuilder sbItem = new StringBuilder();
            configurationItems.QuoteAllFields = true;

            using (TextWriter textWriterItems = new StringWriter(sbItem))
            using (var csvWriterItems = new CsvWriter(textWriterItems, configurationItems))
            {
                csvWriterItems.Context.Writer.NewLine = "";


                foreach (SalesOrderItem item in salesOrder.Items.SalesOrderItem)
                {
                    sb.Append(",");
                    sb.AppendLine();//Append("/r");

                    //translate from SalesOrderItem to ImportSalesOrderItem
                    ImportSalesOrderItem importSalesOrderItem = new ImportSalesOrderItem
                    {
                        Flag = "Item",
                        ItemDateScheduled = item.DateScheduledFulfillment,
                        KitItem = item.KitItemFlag,
                        Note = item.Note,
                        ProductDescription = item.Description,
                        ProductNumber = item.ProductNumber,
                        ProductPrice = item.ProductPrice,
                        ProductQuantity = item.Quantity,
                        QuickBooksClassName = item.QuickBooksClassName,
                        RevisionLevel = item.RevisionLevel,
                        ShowItem = "true",
                        SOItemTypeID = item.ItemType,
                        Taxable = item.Taxable,
                        TaxCode = item.TaxCode,
                        UOM = item.UOMCode
                    };
                    csvWriterItems.WriteRecord(importSalesOrderItem);

                    //set the custom fields for the so
                    //write the variable custom fields using the field indicies in our dictionary to define column placement
                    //starts at the end of the regular import property fields and if the index matches then it writes the value
                    for (int i = writeableFieldCount; i < headerFieldCount; i++)
                    {
                        if (salesOrderCustomFieldIndexes.TryGetValue(i, out string value))
                        {
                            csvWriterItems.WriteField(value);
                        }
                        else
                        {
                            csvWriterItems.WriteField("");
                        }
                    }

                    csvWriterItems.Flush();
                }
                rowField.Add(sbItem.ToString());
            }

            importRows.Add(new Row { RowField = rowField });

            ImportRq importRq = new ImportRq();
            importRq.Type = ImportNameConsts.SALES_ORDER_IMPORT;
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);







        }





        /// <summary>
        /// Moves inventory. The provided TagId is used to get all of the active and used tracking fields
        /// for this tag. These are then used in the MoveRq to make sure the tag is moved correctly.
        /// </summary>
        /// <param name="PartNumber"></param>
        /// <param name="BeginLocationWithLG"></param>
        /// <param name="Qty"></param>
        /// <param name="EndLocationWithLG"></param>
        /// <param name="Note"></param>
        /// <param name="TagID">TagID NOT TagNum. Set to 0 if PartTracking is not used</param>
        /// <returns></returns>
        public async Task MoveInventoryImportAsync(string PartNumber, string BeginLocationWithLG, int Qty, string EndLocationWithLG,
                string Note, int TagID)
        {
            //validate
            if (String.IsNullOrEmpty(PartNumber))
            {
                throw new ArgumentNullException("Part Number is required");
            }
            if (String.IsNullOrEmpty(BeginLocationWithLG))
            {
                throw new ArgumentNullException("Beginning Location is required");
            }
            if (String.IsNullOrEmpty(EndLocationWithLG))
            {
                throw new ArgumentNullException("End Location is required");
            }
            if (Qty <= 0)
            {
                throw new ArgumentNullException("Qty to move must be higher than 0");
            }


            ////get headers
            string header = (await getImportHeaderRowAsync(ImportNameConsts.INVENTORY_MOVE)).FirstOrDefault();
            //serialize to csv format


            if (String.IsNullOrEmpty(header))
            {
                throw new Exception("Header row is not available");
            }

            Configuration configuration = new Configuration();
            DefaultClassMap<ImportInventoryMove> custMap;
            int writeableFieldCount = 0;
            int headerFieldCount = 0;


            Dictionary<int, string> trackingIndexes = null;


            List<TagTrackingObject> tagTrackingItems = null;
            try
            {
                tagTrackingItems = await GetTrackingByTag(TagID); //throws keynotfound no results error if 0 rows
            }
            catch (KeyNotFoundException)
            {

            }

            if (tagTrackingItems?.Count > 0)
            {
                //We need to get all of the tracking from the DB



                //we need to deserialize the header row to find out which index column our tracking field ends up in

                using (TextReader textReader = new StringReader(header))
                using (var csvHeader = new CsvReader(textReader))
                {
                    csvHeader.Read();
                    csvHeader.ReadHeader();

                    headerFieldCount = csvHeader.Context.HeaderRecord.Length;

                    trackingIndexes = new Dictionary<int, string>();


                    foreach (TagTrackingObject item in tagTrackingItems)
                    {
                        if (!String.IsNullOrEmpty(item.TrackingLabel))
                        {

                            //get field index for each returned item and add to dictionary
                            try
                            {
                                trackingIndexes.Add(csvHeader.GetFieldIndex("Tracking-" + item.TrackingLabel), item.Info);
                            }
                            catch (CsvHelper.MissingFieldException ex)
                            {
                                throw new ArgumentException("Tracking Name not found.", ex);
                            }
                        }
                    }


                    custMap = new DefaultClassMap<ImportInventoryMove>();
                    custMap.AutoMap();

                    //we are going to set the tracking fields manually since they can vary
                    custMap.Map(m => m.TrackingFieldName).Ignore();
                    custMap.Map(m => m.TrackingValue).Ignore();
                    //custMap.Map(m => m.TrackingValue).Name("Tracking-" + PrimaryTrackingName); //add back once FB fixes header import issue
                    //custMap.Map(m => m.TrackingValue).Index(primaryTrackingColumnIndex);

                    writeableFieldCount = custMap.MemberMaps.Where(m => m.Data.Ignore == false).Count();

                    configuration.RegisterClassMap(custMap);
                }
            }

            StringBuilder sb = new StringBuilder();
            configuration.QuoteAllFields = true;
            List<Row> importRows = new List<Row>();


            using (TextWriter textWriter = new StringWriter(sb))
            using (var csvWriter = new CsvWriter(textWriter, configuration))
            {
                csvWriter.Context.Writer.NewLine = "";
                //List<string> importRows = new List<string>();


                //Write the new headers with only the primary tracking
                //2018.7 doesnt parse header row on import correctly so we are leaving it off, but we still need the full qty of fields
                //csvWriter.WriteHeader<ImportInventoryMove>();
                //csvWriter.NextRecord();
                //importRows.Add(new Row { RowField = sb.ToString() });
                //sb.Clear();

                ImportInventoryMove importInventoryMove = new ImportInventoryMove
                {
                    PartNumber = PartNumber,
                    Qty = Qty,
                    BeginLocation = BeginLocationWithLG,
                    EndLocation = EndLocationWithLG,
                    Note = Note
                };

                csvWriter.WriteRecord(importInventoryMove);

                //write the variable custom fields using the field indicies in our dictionary to define column placement
                for (int i = writeableFieldCount; i < headerFieldCount; i++)
                {
                    if (trackingIndexes.TryGetValue(i, out string value))
                    {
                        csvWriter.WriteField(value);
                    }
                    else
                    {
                        csvWriter.WriteField("");
                    }
                }

                csvWriter.Flush();
            }
            //importRows.Add(new Row { RowField = sb.ToString() });
            importRows.Add(new Row { RowField = new List<string> { sb.ToString() } });


            ImportRq importRq = new ImportRq();
            importRq.Type = "ImportInventoryMove";
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);

        }

        /// <summary>
        /// Cycle count import to adjust inventory for existing location. 
        /// </summary>
        /// <param name="PartNumber"></param>
        /// <param name="Location">Location to adjust. If in users default location group, LG not needed</param>
        /// <param name="NewQty"></param>
        /// <param name="Note">Optional</param>
        /// <param name="Customer">Optional. Pass in null if not needed</param>
        /// <param name="TagID">TagId to load tracking information. 0 if tracking is not used</param>
        /// <returns></returns>
        public async Task CycleInventoryImportAsync(string PartNumber, string Location, int NewQty, string Note, string Customer, int TagID)
        {
            if (String.IsNullOrEmpty(PartNumber))
            {
                throw new ArgumentNullException("Part Number required");
            }
            if (String.IsNullOrEmpty(Location))
            {
                throw new ArgumentNullException("Location required");
            }
            if (NewQty < 0)
            {
                throw new ArgumentException("New qty must be positive");
            }


            Configuration configuration = new Configuration();
            //ImportCycleCountDataClassMap custMap;
            int writeableFieldCount = 0;
            int headerFieldCount = 0;


            Dictionary<int, string> trackingIndexes = null;

            List<TagTrackingObject> tagTrackingItems = null;
            try
            {
                tagTrackingItems = await GetTrackingByTag(TagID); //throws keynotfound no results error if 0 rows
            }
            catch (KeyNotFoundException)
            {

            }

            if (tagTrackingItems?.Count > 0)
            {
                //load tracking info
                ////get headers
                string header = (await getImportHeaderRowAsync(ImportNameConsts.INVENTORY_CYCLE_COUNT)).FirstOrDefault();

                if (String.IsNullOrEmpty(header))
                {
                    throw new Exception("Header row is not available");
                }

                //we need to deserialize the header row to find out which index column our tracking field ends up in

                using (TextReader textReader = new StringReader(header))
                using (var csvHeader = new CsvReader(textReader))
                {
                    csvHeader.Read();
                    csvHeader.ReadHeader();

                    headerFieldCount = csvHeader.Context.HeaderRecord.Length;
                    trackingIndexes = new Dictionary<int, string>();

                    foreach (TagTrackingObject item in tagTrackingItems)
                    {
                        if (!string.IsNullOrEmpty(item.TrackingLabel))
                        {
                            //get field index for each returned item and add to dictionary
                            try
                            {
                                trackingIndexes.Add(csvHeader.GetFieldIndex("Tracking-" + item.TrackingLabel), item.Info);
                            }
                            catch (CsvHelper.MissingFieldException ex)
                            {
                                throw new ArgumentException("Tracking Name not found.", ex);
                            }
                        }
                    }

                }

            }

            StringBuilder sb = new StringBuilder();
            List<Row> importRows = new List<Row>();
            var classMap = new ImportCycleCountDataClassMap();
            writeableFieldCount = classMap.MemberMaps.Where(m => m.Data.Ignore == false).Count();
            configuration.RegisterClassMap<ImportCycleCountDataClassMap>(); //AutoMap in constructior for now


            configuration.QuoteAllFields = true;

            using (TextWriter textWriter = new StringWriter(sb))
            using (var csvWriter = new CsvWriter(textWriter, configuration))
            {
                csvWriter.Context.Writer.NewLine = "";

                ImportCycleCountData importCycleCountData = new ImportCycleCountData
                {
                    PartNumber = PartNumber,
                    Qty = NewQty,
                    Note = Note,
                    Location = Location,
                    Customer = Customer,
                    Date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") //TODO: verify formatting
                };


                csvWriter.WriteRecord(importCycleCountData);

                //write the variable custom fields using the field indicies in our dictionary to define column placement
                for (int i = writeableFieldCount; i < headerFieldCount; i++)
                {
                    if (trackingIndexes.TryGetValue(i, out string value))
                    {
                        csvWriter.WriteField(value);
                    }
                    else
                    {
                        csvWriter.WriteField("");
                    }
                }

                csvWriter.Flush();
            }
            importRows.Add(new Row { RowField = new List<string> { sb.ToString() } });


            ImportRq importRq = new ImportRq();
            importRq.Type = ImportNameConsts.INVENTORY_CYCLE_COUNT;
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);



        }


        /// <summary>
        /// Set a parts default location using the location name and group name
        /// </summary>
        /// <param name="PartNumber">Part to set for</param>
        /// <param name="LocationName">Location Name ex. "A1A"</param>
        /// <param name="LocationGroupName">Location Group Name</param>
        /// <returns></returns>
        public async Task<bool> SetDefaultPartLocationImportAsync(string PartNumber, string LocationName, string LocationGroupName)
        {
            if (String.IsNullOrEmpty(PartNumber))
            {
                throw new ArgumentNullException("Part Number is required");
            }
            if (String.IsNullOrEmpty(LocationName))
            {
                throw new ArgumentNullException("Beginning Location is required");
            }
            if (String.IsNullOrEmpty(LocationGroupName))
            {
                throw new ArgumentNullException("End Location is required");
            }


            ////get headers
            string header = (await getImportHeaderRowAsync(ImportNameConsts.SET_PART_DEFAULT_LOCATION)).FirstOrDefault();


            if (String.IsNullOrEmpty(header))
            {
                throw new Exception("Header row is not available");
            }

            StringBuilder sb = new StringBuilder();
            List<Row> importRows = new List<Row>();
            //configuration.QuoteAllFields = true;

            using (TextWriter textWriter = new StringWriter(sb))
            using (var csvWriter = new CsvWriter(textWriter))
            {
                csvWriter.Context.Writer.NewLine = "";



                //Write the new headers with only the primary tracking
                //2018.7 doesnt parse header row on import correctly so we are leaving it off, but we still need the full qty of fields
                //csvWriter.WriteHeader<ImportPartDefaultLocations>();
                //csvWriter.NextRecord();

                ImportPartDefaultLocations importPartDefaultLocations = new ImportPartDefaultLocations
                {
                    PartNumber = PartNumber,
                    Location = LocationName,
                    LocationGroup = LocationGroupName

                };

                csvWriter.WriteRecord(importPartDefaultLocations);
                csvWriter.Flush();
            }
            importRows.Add(new Row { RowField = new List<string> { sb.ToString() } });


            ImportRq importRq = new ImportRq();
            importRq.Type = ImportNameConsts.SET_PART_DEFAULT_LOCATION;
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);

            return true;
        }


    }
}
