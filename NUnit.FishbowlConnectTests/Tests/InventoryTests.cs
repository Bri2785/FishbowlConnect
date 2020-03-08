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

        [TestCase("InventoryToAddDecimal", "Stock 100")]
        public async Task AddInventoryImportTest(string partNumber, string LocationName)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession session = new FishbowlSession(config))
            {
                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(new MySqlConfig(
                    DatabaseAddress, DatabasePort.ToString(), DatabaseUser, DatabasePassword,
                    DatabaseName)))
                {
                    //List<InvQtyGroupedByUniqueTagInfoWithTracking> invQties = 
                    //    await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(partNumber,"", FishbowlConnect.Helpers.InventorySearchTermType.Part);

                    //add our inventory
                    List<PartNumAndTracks> partNumAndTracks = await session.GetPartNumberAndTrackingFields(partNumber);
                    foreach (var item in partNumAndTracks)
                    {
                        switch (item.TrackingTypeID)
                        {
                            case 10:
                            case 40:
                                item.TrackingInfo = "9112";
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

                    await session.AddInventoryImportAsync(partNumber, 10.55m, LocationName, "LA", "Test Import Add",
                             partNumAndTracks);

                    List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtiesAfterImport = 
                        await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(partNumber, "", FishbowlConnect.Helpers.InventorySearchTermType.Part);

                    InvQtyGroupedByUniqueTagInfoWithTracking invQty = invQtiesAfterImport
                        .First(iv => iv.PartNumber == partNumber && iv.LocationName == LocationName);

                    Assert.IsTrue(invQty.Qty == 10.55m);


                    //revert
                    await session.CycleInventoryImportAsync(partNumber, "LA-" + LocationName, 0m, "revert add", null, invQty.SimpleTracking);
                }
            }



        }

        [TestCase("InventoryToCycle")]
        [TestCase("InventoryToCycleDecimal")]
        public async Task CycleInventoryImportTest(string partNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {
                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(new MySqlConfig(
                    DatabaseAddress, DatabasePort.ToString(), DatabaseUser, DatabasePassword,
                    DatabaseName)))
                {
                    InvQtyGroupedByUniqueTagInfoWithTracking invQty = (await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(
                        partNumber, "", FishbowlConnect.Helpers.InventorySearchTermType.Part)).First(m => (m.Qty - m.QtyCommitted) > 1);
                    if (invQty != null)
                    {
                        decimal qtyToCycleTo = Math.Round((invQty.Qty - invQty.QtyCommitted) / 2, 2, MidpointRounding.AwayFromZero);

                        await session.CycleInventoryImportAsync(invQty.PartNumber, invQty.LocationFullName, qtyToCycleTo,
                            "Test Cycle Count", null, invQty.SimpleTracking); 

                        InvQtyGroupedByUniqueTagInfoWithTracking newInvQty = (await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(
                        partNumber, "", FishbowlConnect.Helpers.InventorySearchTermType.Part))
                                .Find(m => m.LocationFullName == invQty.LocationFullName
                                    && m.TrackingEncoding == invQty.TrackingEncoding);

                        Assert.IsTrue(newInvQty.Qty == qtyToCycleTo);

                        //revert
                        await session.CycleInventoryImportAsync(invQty.PartNumber, invQty.LocationFullName, invQty.Qty,
                            "Test Cycle Count Revert", null, invQty.SimpleTracking);

                    }
                    else
                    {
                        Assert.Fail("No location where inventory is greater than 1. Select a different part");
                    }
                }

            }
        }

        [TestCase("InventoryToMove")]
        public async Task MoveInventoryImportTest(string partNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyWithTrackings = null;

                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(new MySqlConfig(DatabaseAddress,
                    DatabasePort.ToString(), DatabaseUser, DatabasePassword, DatabaseName)))
                {
                    invQtyWithTrackings = await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(partNumber, "", FishbowlConnect.Helpers.InventorySearchTermType.Part);

                    if (!(invQtyWithTrackings.Count > 0))
                    {
                        Assert.Fail("Part number has no inventory");
                    }

                    InvQtyGroupedByUniqueTagInfoWithTracking invQty = invQtyWithTrackings[0];
                    string fromLocation = invQty.LocationFullName;

                    //to location can be any location (just not the from location), getting first of the list for the default LG
                    LocationSimpleObject toLocationObject = (await session.GetLocationSimpleList("TX"))
                                                                .First(m => m.LocationFullName != fromLocation);
                    string toLocation = toLocationObject.LocationFullName;

                    decimal moveQty = invQty.Qty; //move the full qty

                    decimal ToLocationExistingQty = 0m;
                    try
                    {
                        var toLocationOption = (await db.GetPartTagAndAllTrackingWithDefaultLocation("$L$" + toLocationObject.LocationName,
                            "TX", FishbowlConnect.Helpers.InventorySearchTermType.Part))
                       .Find(m => m.PartNumber == partNumber);
                        if (toLocationOption != null)
                        {
                            ToLocationExistingQty = toLocationOption.Qty;
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        //no qty found
                        ToLocationExistingQty = 0m;
                    }


                    await session.MoveInventoryImportAsync(partNumber, fromLocation, moveQty,
                        toLocation, "Test Move", invQty.SimpleTracking);  

                    //now check the to location qty

                    decimal newqty = (await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(partNumber, "", FishbowlConnect.Helpers.InventorySearchTermType.Part))
                        .Find(m => m.LocationFullName == toLocationObject.LocationFullName).Qty;

                    Assert.IsTrue(newqty == (moveQty + ToLocationExistingQty));

                    //move back

                    await session.MoveInventoryImportAsync(partNumber, toLocation, moveQty,
                        fromLocation, "Test Move Back", invQty.SimpleTracking);


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
