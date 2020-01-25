using FishbowlConnect.MySQL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class InventoryQtyTests
    {
        const string BadServerAddress = "127.5.4.3";
        const string BadUserName = "sjjsd";
        const string BadPassword = "jfjsdf";
        const string NoUserName = null;
        const string NoPassword = null;

        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
       
        const string BadDatabaseName = "fndfnd";


        //const string GoodServerAddress = "192.168.150.2";
        //const string GoodUserName = "admin";
        //const string GoodPassword = "does1tall";
        //const string DatabaseAddress = "192.168.150.2";
        //const int DatabasePort = 3301;
        //const string DatabaseName = "briteideasupdate";
        //const string ValidPartNumberWithInventory = "ECL-SC";
        //const string ValidDefaultLocationGroup = "Main Warehouse";

        const string ValidProductNumberWithInventory = "ML60BLcs6";

        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";
        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 2361;
        const string DatabaseName = "gcs_copy_1_23_20";
        const string ValidPartNumberWithInventory = "CSBL030";
        const string ValidDefaultLocationGroup = "Main";



        //all run against briteideasUpdate DB Date 3-5-19, C:\Program Files\Fishbowl\data\backups


        [TestCase(ValidPartNumberWithInventory)]
        public async Task InvQtyGroupedWithTrackingShouldReturnNestedLists(string partNumber)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                DatabaseUser, DatabasePassword, DatabaseName);

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {

                List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyGroupedByTags
                    = await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(partNumber,
                                                ValidDefaultLocationGroup, FishbowlConnect.Helpers.InventorySearchTermType.Part);


                Assert.IsInstanceOf(typeof(List<InvQtyGroupedByUniqueTagInfoWithTracking>), invQtyGroupedByTags);

                //should be 3 tags, each with 2 tracking items
                Assert.True(invQtyGroupedByTags.Count == 2);

                InvQtyGroupedByUniqueTagInfoWithTracking first = invQtyGroupedByTags.FirstOrDefault();

                Assert.True(first.SimpleTracking.Count == 11);

                Assert.NotNull(first.SimpleTracking[0].TrackingInfo); //will hold the lot number

                Assert.True(first.SimpleTracking.Where(i => i.TrackingLabel == "Lot Number").First().TrackingInfo == "L18-19-8013");

                Assert.True(!string.IsNullOrEmpty(first.PrimaryTrackingValueAndName));

                Assert.True(first.PrimaryTrackingValueAndName == "L18-19-8013-Lot#");

            }




        }

        [TestCase(ValidProductNumberWithInventory)]
        public async Task InvQtyGroupedWithNoTrackingShouldReturnSingleListForProducts(string productNumber)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                DatabaseUser, DatabasePassword, DatabaseName);

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {

                List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyGroupedByTags
                    = await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(productNumber,
                                                ValidDefaultLocationGroup, FishbowlConnect.Helpers.InventorySearchTermType.Product);


                Assert.IsInstanceOf(typeof(List<InvQtyGroupedByUniqueTagInfoWithTracking>), invQtyGroupedByTags);

                //should be 1 tag with no tracking info
                Assert.True(invQtyGroupedByTags.Count == 2);

                InvQtyGroupedByUniqueTagInfoWithTracking first = invQtyGroupedByTags.FirstOrDefault();

                Assert.IsEmpty(first.SimpleTracking);

                Assert.IsNull(first.PrimaryTrackingValueAndName);

            }




        }

        [TestCase("TestProductcs4")]
        [TestCase("ECM-SC")]//no tracking, multiple products
        public async Task InvQtyGroupedWithNoTrackingShouldReturnSingleList(string partNumber)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                DatabaseUser, DatabasePassword, DatabaseName);

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {

                List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyGroupedByTags
                    = await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(partNumber,
                                                ValidDefaultLocationGroup, FishbowlConnect.Helpers.InventorySearchTermType.Part);


                Assert.IsInstanceOf(typeof(List<InvQtyGroupedByUniqueTagInfoWithTracking>), invQtyGroupedByTags);

                //should be 1 tag with no tracking info
                Assert.True(invQtyGroupedByTags.Count == 1);

                InvQtyGroupedByUniqueTagInfoWithTracking first = invQtyGroupedByTags.FirstOrDefault();

                Assert.IsEmpty(first.SimpleTracking);

                Assert.IsNull(first.PrimaryTrackingValueAndName);

            }




        }

        [TestCase("100GGR")]
        public async Task InvQtySearchedByPartOrProductShouldHaveDefaultLocationCorrect(string searchTerm)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                DatabaseUser, DatabasePassword, DatabaseName);

            using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(config))
            {
                List<InvQtyWithAllTracking> list;

                    list = await db.GetPartTagAndAllTrackingWithDefaultLocation(searchTerm, "Main Warehouse"
                        , FishbowlConnect.Helpers.InventorySearchTermType.Part);


                foreach (var item in list)
                {
                    Debug.WriteLine(item.LocationName + " - " + item.DefaultLocationMatchStatus.ToString());
                }

                list = await db.GetPartTagAndAllTrackingWithDefaultLocation(searchTerm, "Main Warehouse"
                        , FishbowlConnect.Helpers.InventorySearchTermType.Product);


                foreach (var item in list)
                {
                    Debug.WriteLine(item.LocationName + " - " + item.DefaultLocationMatchStatus.ToString());
                }

            }
        }

    }
}
