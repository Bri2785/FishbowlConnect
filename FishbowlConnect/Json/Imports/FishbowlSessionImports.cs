using CsvHelper;
using CsvHelper.Configuration;
using FishbowlConnect.Exceptions;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Json;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.Imports;
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

                TextReader textReader = new StringReader(header);
                var csvHeader = new CsvReader(textReader);
                csvHeader.Read();
                csvHeader.ReadHeader();

                headerFieldCount = csvHeader.Context.HeaderRecord.Length;

                trackingIndexes = new Dictionary<int, string>();


                foreach (IPartTrackingFields item in partTrackingInfo)
                {
                    if (!String.IsNullOrEmpty(item.TrackingLabel))
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

            StringBuilder sb = new StringBuilder();
            TextWriter textWriter = new StringWriter(sb);
            configuration.QuoteAllFields = true;

            var csvWriter = new CsvWriter(textWriter, configuration);
            csvWriter.Context.Writer.NewLine = "";
            //List<string> importRows = new List<string>();
            List<Row> importRows = new List<Row>();


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
            importRows.Add(new Row { RowField = new List<string> { sb.ToString() } });


            ImportRq importRq = new ImportRq();
            importRq.Type = ImportNameConsts.INVENTORY_ADD;
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);

        }


        /// <summary>
        /// Takes a class object and converts it to the CSV import version that FB requires and submits it to FB
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

            TextReader textReader = new StringReader(soHeader);
            var csvHeader = new CsvReader(textReader);
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

            //}

            //TODO: add using blocks

            StringBuilder sb = new StringBuilder();
            TextWriter textWriter = new StringWriter(sb);
            configuration.QuoteAllFields = true;

            var csvWriter = new CsvWriter(textWriter, configuration);
            csvWriter.Context.Writer.NewLine = "";

            //each sales order including the line items need to be on a single concatenated row
            List<Row> importRows = new List<Row>();
            List<string> rowField = new List<string>();

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


            //if (salesOrder.CustomField?.Count() > 0)
            //{

            //we need to deserialize the header row to find out which index column our custom field ends up in

            textReader = new StringReader(soItemHeader);
            csvHeader = new CsvReader(textReader);
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

            //configuration.RegisterClassMap(soItemsMap);

            configurationItems.RegisterClassMap(soItemsMap);

            //}



            StringBuilder sbItem = new StringBuilder();
            TextWriter textWriterItems = new StringWriter(sbItem);
            configurationItems.QuoteAllFields = true;

            var csvWriterItems = new CsvWriter(textWriterItems, configurationItems);
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
                //csvWriterItems.Flush();
                //importRows.Add(new Row { RowField = sbItem.ToString() });
                rowField.Add(sbItem.ToString());
            }

            importRows.Add(new Row { RowField = rowField });

            ImportRq importRq = new ImportRq();
            importRq.Type = ImportNameConsts.SALES_ORDER_IMPORT;
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);







        }


    }
}
