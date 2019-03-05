using FishbowlConnect.MySQL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests
{
    [TestFixture]
    public class InventoryQtyTests
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

        const string ValidPartNumberWithInventory = "ECL-SC";
        const string ValidDefaultLocationGroup = "MainWarehouse";



        //all run against briteideasUpdate DB Date 3-5-19, C:\Program Files\Fishbowl\data\backups


        [TestCase("TestProduct")]
        public async Task InvQtyGroupedWithTrackingShouldReturnNestedLists(string partNumber)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                DatabaseUser, DatabasePassword, DatabaseName);

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {

                List<InvQtyGroupedByTagWithTracking> invQtyGroupedByTags
                    = await db.GetPartTagGroupedWithAllTrackingWithDefaultLocation(partNumber,
                                                ValidDefaultLocationGroup, FishbowlConnect.Helpers.InventorySearchTermType.Part);


                Assert.IsInstanceOf(typeof(List<InvQtyGroupedByTagWithTracking>), invQtyGroupedByTags);

                //should be 3 tags, each with 2 tracking items
                Assert.True(invQtyGroupedByTags.Count == 3);

                InvQtyGroupedByTagWithTracking first = invQtyGroupedByTags.FirstOrDefault();

                Assert.True(first.SimpleTracking.Count == 2);

                Assert.NotNull(first.SimpleTracking[0].TrackingInfo); //will hold the lot number

                Assert.True(first.SimpleTracking.Where(i => i.TrackingLabel == "Lot Number").First().TrackingInfo == "1218");

                Assert.True(!string.IsNullOrEmpty(first.PrimaryTrackingValueAndName));

                Assert.True(first.PrimaryTrackingValueAndName == "1218-Lot#");

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

                List<InvQtyGroupedByTagWithTracking> invQtyGroupedByTags
                    = await db.GetPartTagGroupedWithAllTrackingWithDefaultLocation(partNumber,
                                                ValidDefaultLocationGroup, FishbowlConnect.Helpers.InventorySearchTermType.Part);


                Assert.IsInstanceOf(typeof(List<InvQtyGroupedByTagWithTracking>), invQtyGroupedByTags);

                //should be 1 tag with no tracking info
                Assert.True(invQtyGroupedByTags.Count == 1);

                InvQtyGroupedByTagWithTracking first = invQtyGroupedByTags.FirstOrDefault();

                Assert.IsEmpty(first.SimpleTracking);

                Assert.IsNull(first.PrimaryTrackingValueAndName);

            }




        }


    }
}
