using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FishbowlConnect.Json.Requests;
using FishbowlConnect.Json;
using FishbowlConnect.Json.Imports;
using System.IO;
using FishbowlConnect.Json.CsvClassMaps;
using CsvHelper.Configuration;
using CsvHelper;
using System.Linq;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {

        public async Task<Product> GetProduct(string ProductNumber)
        {
            return await this.GetProduct(ProductNumber, false);

        }

        public async Task<Product> GetProduct(string ProductNumber, bool GetImage)
        {
            ProductGetRq ProductGetRq = new ProductGetRq();

            ProductGetRq.Number = ProductNumber;
            ProductGetRq.GetImage = GetImage.ToString();

            ProductGetRs ProductGetRs = await this.IssueJsonRequestAsync<ProductGetRs>(ProductGetRq);

            return ProductGetRs.Product;

        }


        public async Task<bool> SaveNewProduct(Product newProduct)
        {
            ProductSaveRqType ProductSaveRq = new ProductSaveRqType();
            ProductSaveRq.Product = newProduct;

            ProductSaveRsType productSaveRs = await IssueXMLRequestAsync<ProductSaveRsType>(ProductSaveRq);

            return true;


        }
        public async Task<bool> ImportNewProduct(Product newProduct)
        {
            ImportRqType ProductImportRq = new ImportRqType();

            ProductImportRq.Type = "ImportProduct";

            ProductImportRq.Rows = new Rows();
            ProductImportRq.Rows.Row = new List<string>();
            ProductImportRq.Rows.Row.Add(ImportHeaders.IMPORT_PRODUCT_HEADER_NO_CUSTOM);

            ProductImportRq.Rows.Row.Add(@"Misc Retail Item," + newProduct.Num + "," + newProduct.Description + "," + newProduct.Details +
                                                    ",ea," + newProduct.Price + ",,true,true,true,false,,," + newProduct.UPC + ",,Sale,,0,lbs,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,"
                                            );

            ImportRsType productImportRs = await IssueXMLRequestAsync<ImportRsType>(ProductImportRq);

            if (productImportRs.statusCode != "1000")
            {
                throw new Exception(productImportRs.statusCode + " - " + Utilities.StatusCodeMessage(productImportRs.statusCode));
            }
            return true;


        }

        [Obsolete("Need to fix to make sure all fields are supported")]
        public async Task SaveProductNoCustomFields(Product product)
        {
            ImportRqType ProductImportRq = new ImportRqType();

            ProductImportRq.Type = "ImportProduct";

            ProductImportRq.Rows = new Rows();
            ProductImportRq.Rows.Row = new List<string>();
            ProductImportRq.Rows.Row.Add(ImportHeaders.IMPORT_PRODUCT_HEADER_NO_CUSTOM);

            ProductImportRq.Rows.Row.Add(
                product.Part.Num + "," +
                product.Num + "," +
                product.Description + "," +
                product.Details + "," +
                product.UOM.Name + "," +
                product.Price + "," +
                product.ActiveFlag + "," +
                product.TaxableFlag + "," +
                product.ShowSOComboFlag + "," +
                product.URL + "," +
                product.UPC + "," +
                product.SKU + "," +
                product.DefaultSOItemType + "," +
                product.Weight + "," +
                product.WeightUOMID + "," +
                product.Width + "," +
                product.Height + "," +
                product.Len + "," +
                product.SizeUOMID + "," +
                "true"

                );

            ImportRsType productImportRs = await IssueXMLRequestAsync<ImportRsType>(ProductImportRq);

            if (productImportRs.statusCode != "1000")
            {
                throw new Exception(productImportRs.statusCode + " - " + Utilities.StatusCodeMessage(productImportRs.statusCode));
            }


        }

        /// <summary>
        /// Saves updated price and UPC to FB. The product object supplied must have any required fields specified or it will return an error
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task SaveProductPriceAndUpc(Product product)
        {

            ImportProductPricingAndUpc import = new ImportProductPricingAndUpc
            {
                PartNumber = product.Part.Num,
                ProductNumber = product.Num,
                ProductUPC = product.UPC,
                Price = product.Price
            };

            int headerFieldCount = 0;
            Dictionary<int, string> cFieldIndexes = null;

            if (product.CustomFields.CustomField.Where(cf => cf.RequiredFlag == "True").ToList().Count > 0)
            {
                string header = await getImportHeaderRowAsync(ImportNameConsts.PRODUCT_SAVE_PRICE_AND_UPC);

                if (string.IsNullOrEmpty(header))
                {
                    throw new Exception("Header row is not available");
                }

                //we need to deserialize the header row to find out which index column our required custom fields are in

                TextReader textReader = new StringReader(header);
                var csvHeader = new CsvReader(textReader);
                csvHeader.Read();
                csvHeader.ReadHeader();

                headerFieldCount = csvHeader.Context.HeaderRecord.Length;
                cFieldIndexes = new Dictionary<int, string>();


                foreach (var property in import.GetType().GetProperties())
                {
                    //get field index for each returned item and add to dictionary
                    try
                    {
                        cFieldIndexes.Add(csvHeader.GetFieldIndex(property.Name), property.GetValue(import).ToString());
                    }
                    catch (CsvHelper.MissingFieldException ex)
                    {
                        throw new ArgumentException("Class Property Name not found.", ex);
                    }
                }

                foreach (var item in product.CustomFields.CustomField.Where(cf => cf.RequiredFlag == "True"))
                {
                    if (!string.IsNullOrEmpty(item.Info))
                    {
                        //get field index for each returned item and add to dictionary
                        try
                        {
                            cFieldIndexes.Add(csvHeader.GetFieldIndex("CF-" + item.Name), item.Info);
                        }
                        catch (CsvHelper.MissingFieldException ex)
                        {
                            throw new ArgumentException("Custom Field Name not found.", ex);
                        }
                    }
                }

            }

            Configuration configuration = new Configuration();

            StringBuilder sb = new StringBuilder();
            TextWriter textWriter = new StringWriter(sb);

            configuration.RegisterClassMap<ImportProductPricingAndUpcClassMap>(); //AutoMap in constructor for now

            configuration.QuoteAllFields = true;

            var csvWriter = new CsvWriter(textWriter, configuration);
            csvWriter.Context.Writer.NewLine = "";
            List<Row> importRows = new List<Row>();

            //write all of the fields using the field indicies in our dictionary to define column placement
            for (int i = 0; i < headerFieldCount; i++)
            {
                if (cFieldIndexes.TryGetValue(i, out string value))
                {
                    csvWriter.WriteField(value);
                }
                else
                {
                    csvWriter.WriteField("");
                }
            }


            csvWriter.Flush();
            importRows.Add(new Row { RowField = sb.ToString() });


            ImportRq importRq = new ImportRq();
            importRq.Type = ImportNameConsts.PRODUCT_SAVE_PRICE_AND_UPC;
            importRq.Rows = importRows;

            await IssueJsonRequestAsync<ImportRs>(importRq);


        }

        //public UOM GetUOMObject()
        //{
        //    UOM UOMRs = DeserializeFromXMLString<UOM>(@"<UOM>
        //                 <UOMID>1</UOMID>
        //                 <Name>Each</Name>
        //                 <Code>ea</Code>
        //                 <Integral>true</Integral>
        //                 <Active>true</Active>
        //                 <Type>Count</Type>
        //                <UOMConversions>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>17</ToUOMID>
        //                 <ToUOMCode>cs1K</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>1000.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>19</ToUOMID>
        //                 <ToUOMCode>cs2</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>2.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>18</ToUOMID>
        //                 <ToUOMCode>cs25</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>25.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>21</ToUOMID>
        //                 <ToUOMCode>cs6</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>6.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>20</ToUOMID>
        //                 <ToUOMCode>cs4</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>4.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>23</ToUOMID>
        //                 <ToUOMCode>cs100</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>100.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>22</ToUOMID>
        //                 <ToUOMCode>cs200</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>200.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>25</ToUOMID>
        //                 <ToUOMCode>cs12</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>12.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>24</ToUOMID>
        //                 <ToUOMCode>cs50</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>50.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>27</ToUOMID>
        //                 <ToUOMCode>cs3</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>3.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>26</ToUOMID>
        //                 <ToUOMCode>cs10</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>10.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>29</ToUOMID>
        //                 <ToUOMCode>rl1k</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>1000.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>34</ToUOMID>
        //                 <ToUOMCode>cs5</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>5.0</ConversionFactor>
        //                 </UOMConversion>
        //                <UOMConversion>
        //                 <MainUOMID>1</MainUOMID>
        //                 <ToUOMID>35</ToUOMID>
        //                 <ToUOMCode>cs500</ToUOMCode>
        //                 <ConversionMultiply>1.0</ConversionMultiply>
        //                 <ConversionFactor>500.0</ConversionFactor>
        //                 </UOMConversion>
        //                 </UOMConversions>
        //                 </UOM>
        //                ");
        //    return UOMRs;
        //}
        
    }
}