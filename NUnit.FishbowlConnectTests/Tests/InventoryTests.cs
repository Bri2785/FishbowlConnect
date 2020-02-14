using FishbowlConnect;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using FishbowlConnect.MySQL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class InventoryTests
    {
        const string GoodServerAddress = "192.168.125.26";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.125.26";
        const int DatabasePort = 3305;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "local_demo";

        const string ValidPartNumberWithInventory = "ECL-SC";

        [TestCase("TCB081PK-BranchSet")]
        public async Task AddInventoryWithNoPreviousCostCompletesSuccessfully(string partNumber)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);



            using (FishbowlSession session = new FishbowlSession(config))
            {
                await session.AddInventoryImportAsync(partNumber, 2, "A2A", "Main Warehouse", "Test Note",
                    new List<IPartTrackingFields> { new PartNumAndTracks{ PartNumber = partNumber,
                     TrackingAbbr ="Gen",
                     TrackingID = 5,
                     TrackingInfo = "3",
                     TrackingLabel ="Generation",
                     TrackingPrimaryFlag = false,
                     TrackingSortOrder = 1,
                     TrackingTypeID = 70} });
            }


        }

        [TestCase("PB100", "Stock 100")]
        public async Task AddInventoryImportTest(string partNumber, string LocationName)
        {
            //TODO:convert to new InvQtyWithTracking Format
            //SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<InvQty> invQties = await session.GetPartInventory(partNumber);

                //get the total qty in our location
                int locationInventory = (int)invQties
                    .Where(l => l.Location.Name == LocationName)
                    .Sum(m => decimal.Parse(m.QtyAvailable));


                //add our inventory
                List<PartNumAndTracks> partNumAndTracks = await session.GetPartNumberAndTrackingFields(partNumber);
                foreach (var item in partNumAndTracks)
                {
                    switch (item.TrackingTypeID)
                    {
                        case 10:
                        case 40:
                            item.TrackingInfo = "Test text field";
                            break;
                        case 20:
                        case 30:
                            item.TrackingInfo = "1/1/2018"; //regular MySQL Date format doesnt work here
                            break;
                        case 50:
                        case 60:
                            item.TrackingInfo = "2";
                            break;
                        case 70:
                            item.TrackingInfo = "2";
                            break;
                        case 80:
                            item.TrackingInfo = "true";
                            break;
                        default:
                            item.TrackingInfo = "";
                            break;

                    }
                }

                await session.AddInventoryImportAsync(partNumber, 10, LocationName, "SLC", "Test Import Add",
                         partNumAndTracks);

                List<InvQty> invQtiesAfterImport = await session.GetPartInventory(partNumber);

                //get the total qty in our location
                int locationNewInventory = (int)invQtiesAfterImport
                    .Where(l => l.Location.Name == LocationName)
                    .Sum(m => decimal.Parse(m.QtyAvailable));

                Assert.IsTrue(locationNewInventory == locationInventory + 10);


            }



        }

        [TestCase(ValidPartNumberWithInventory)]
        public async Task CycleInventoryImportTest(string partNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {
                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(new MySqlConfig(
                    DatabaseAddress, DatabasePort.ToString(), DatabaseUser, DatabasePassword,
                    DatabaseName)))
                {
                    InvQtyWithAllTracking invQty = (await db.GetPartTagAndAllTrackingWithDefaultLocation(
                        partNumber, "Main Warehouse", FishbowlConnect.Helpers.InventorySearchTermType.Part)).First(m => m.Qty > 1);
                    if (invQty != null)
                    {


                        await session.CycleInventoryImportAsync(invQty.PartNumber, invQty.LocationFullName, (int)invQty.Qty - 1,
                            "Test Cycle Count", null, null); //TODO: add tracking values

                        InvQtyWithAllTracking newInvQty = (await db.GetPartTagAndAllTrackingWithDefaultLocation(
                        partNumber, "Main Warehouse", FishbowlConnect.Helpers.InventorySearchTermType.Part)).Find(m => m.TagID == invQty.TagID);

                        Assert.IsTrue(newInvQty.Qty == invQty.Qty - 1);
                    }
                    else
                    {
                        Assert.Fail("No location where inventory is greater than 1. Select a different part");
                    }
                }

            }
        }

        [TestCase("ECL-SC")]
        public async Task MoveInventoryImportTest(string partNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            //for (int i = 0; i < 20; i++)
            //{

            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<InvQtyWithAllTracking> invQtyWithTrackings = null;

                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(new MySqlConfig(DatabaseAddress,
                    DatabasePort.ToString(), DatabaseUser, DatabasePassword, DatabaseName)))
                {
                    invQtyWithTrackings = await db.GetPartTagAndAllTrackingWithDefaultLocation(partNumber,"Main Warehouse", FishbowlConnect.Helpers.InventorySearchTermType.Part);

                    if (!(invQtyWithTrackings.Count > 0))
                    {
                        Assert.Fail("Part number has no inventory");
                    }

                    InvQtyWithAllTracking invQty = invQtyWithTrackings[0];
                    string fromLocation = invQty.LocationFullName;

                    //to location can be any location (just not the from location), getting first of the list for the default LG
                    LocationSimpleObject toLocationObject = (await session.GetLocationSimpleList(4))
                                                                .First(m => m.LocationFullName != fromLocation);
                    string toLocation = toLocationObject.LocationFullName;

                    int moveQty = (int)invQty.Qty; //move the full qty

                    int ToLocationExistingQty = 0;
                    try
                    {
                        var toLocationOption = (await db.GetPartTagAndTracking("$L$" + toLocationObject.LocationName))
                       .Find(m => m.PartNumber == partNumber);
                        if (toLocationOption != null)
                        {
                            ToLocationExistingQty = (int)toLocationOption.Qty;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        //no qty found
                        ToLocationExistingQty = 0;
                    }


                    await session.MoveInventoryImportAsync(partNumber, fromLocation, moveQty,
                        toLocation, "Test Move", null);  //TODO: add tracking values

                    //now check the to location qty

                    int newqty = (int)(await db.GetPartTagAndAllTrackingWithDefaultLocation(partNumber, "Main Warehouse", FishbowlConnect.Helpers.InventorySearchTermType.Part))
                        .Find(m => m.LocationFullName == toLocationObject.LocationFullName).Qty;

                    Assert.IsTrue(newqty == (moveQty + ToLocationExistingQty));
                }

            }
            //}
        }




        [TestCase("100GCL")]
        public async Task CycleInventoryImportBadTest(string partNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                await session.CycleInventoryImportAsync(partNumber, "A1", 10,
                    "Test Cycle Count", null, null);  //TODO: add tracking values


            }
        }
    }

}
