using FishbowlConnect;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class ProductTests
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";
        const string BadServerAddress = "127.5.4.3";
        const string BadUserName = "sjjsd";
        const string BadPassword = "jfjsdf";
        const string NoUserName = null;
        const string NoPassword = null;

        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 2361;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";
        const string BadDatabaseName = "fndfnd";

        const string ValidProductNumber = "LED-C70WW";

        //all run against briteideasUpdate DB Date 3-5-19, C:\Program Files\Fishbowl\data\backups

        [TestCase(ValidProductNumber)]
        public async Task ProductRequestedByNumReturnsProductObject(string productNumber)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Product product = await session.GetProduct(productNumber);

                Assert.NotNull(product);
                Assert.True(product.Num == productNumber);
            }


        }

        [TestCase(ValidProductNumber)]
        public async Task WhenImportingProductPriceAndUpcChangesSave(string productNumber)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Product product = await session.GetProduct(productNumber);

                product.Price = "30";
                product.UPC = "042000123456";

                //save product back to FB
                await session.SaveProductPriceAndUpc(product);

                Product newProduct = await session.GetProduct(productNumber);

                Assert.True(newProduct.Price == "30");
                Assert.True(newProduct.UPC == "042000123456");

                product.Price = "23";
                product.UPC = "042002420554";


                //save product back to FB
                await session.SaveProductPriceAndUpc(product);

                Product originalProduct = await session.GetProduct(productNumber);

                Assert.True(originalProduct.Price == "23");
                Assert.True(originalProduct.UPC == "042002420554");
            }


        }

        [TestCase("TestProduct")]
        public async Task RequestProductUomConversionsReturnsList(string PartNumber)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                List<PartToProductUomConversion> conversions = await session.GetProductUomConverisons(PartNumber);

                Assert.NotNull(conversions);
                Assert.True(conversions.Count == 3);

            }


        }

    }
}
