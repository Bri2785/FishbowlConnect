using FishbowlConnect;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;
using FishbowlConnect.MySQL;
using FishbowlConnect.Helpers;
using System.Diagnostics;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class PickingTests
    {
        const string GoodServerAddress = "192.168.125.26";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.125.26";
        const int DatabasePort = 3305;
        const string DatabaseName = "local_demo";
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        //const string ValidPartNumberWithInventory = "CSBL030";
        const string ValidDefaultLocationGroup = "LA";
        //const string ValidPartNumberWithNoInventory = "";
        //const string ValidTrackingWithInventory = "$T$L18-19-8013";
        //const string ValidLocationWithInventory = "$L$WS5C";


        const string ValidPickNumber = "S10083";

        [TestCase(ValidPickNumber)]
        public async Task LoadPickByNumberReturnsPickObject(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);
                Assert.True(pick.PickItems.PickItem.Count >= 1);


            }

        }

        [TestCase(ValidPickNumber, "SMRT102", "SMT", "1Pubxj6wlO6fTVhPkIHM0A==")]
        [TestCase("S10082", "B200", "Stock 100", "930")]
        public async Task LoadPickAndPickItemHandlesCorrectly(string PickNum, string partNumToPick, string locationToPickFrom, string lotNumber = null, string trackingEncoding = null)
        {
            

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);
                Assert.True(pick.PickItems.PickItem.Count >= 1);

                //pick item and save back

                PickItem itemToPick = pick.PickItems.PickItem.Where(pi => pi.Part.Num == partNumToPick).FirstOrDefault();


                //invQty to pick from
                List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyGrouped = null; //list of inventory tags to choose from

                MySqlConfig dbConfig = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                                                                DatabaseUser, DatabasePassword, DatabaseName);

                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(dbConfig))
                {
                    invQtyGrouped =
                        await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(itemToPick.Part.Num,
                        ValidDefaultLocationGroup, InventorySearchTermType.Part);
                }

                //select which one to use
                InvQtyGroupedByUniqueTagInfoWithTracking selectedInvQty = null;
                if (!string.IsNullOrEmpty(lotNumber))
                {
                    selectedInvQty =
                            invQtyGrouped
                                .Where(iq => iq.LocationName == locationToPickFrom && 
                                    iq.SimpleTracking.Any(st => st.TrackingInfo == lotNumber))
                                .FirstOrDefault();
                }
                else
                {
                    selectedInvQty =
                            invQtyGrouped
                                .Where(iq => iq.LocationName == locationToPickFrom && iq.TrackingEncoding == trackingEncoding)
                                .FirstOrDefault();
                }
                

                //
                //.Where(iq => iq.LocationName == "M1" && iq.TrackingEncoding == "32/wVwpZJxIup2uaQHhHQw==")

                Tag fullTagForTrackingInfo = await session.GetTagObjectAsync(selectedInvQty.SimpleTags[0].TagId.ToString());

                PickItem ItemRemaining = null;
                decimal PickItemRequestedQty = selectedInvQty.Qty;

                foreach (PickItem item in pick.PickItems.PickItem)
                {
                    Debug.WriteLine(item.Quantity);
                    if (item.Status != "40")
                    {

                        if (PickItemRequestedQty >= item.Quantity) //selecting all in available in this invQty line
                        {
                            //we have enough
                            item.Status = "40";
                            item.Location = fullTagForTrackingInfo.Location;
                            item.Tracking = fullTagForTrackingInfo.Tracking;


                            //item.qty is unchanged, full amount
                            selectedInvQty.Qty = selectedInvQty.Qty - item.Quantity;
                            PickItemRequestedQty = PickItemRequestedQty - item.Quantity;
                            //QtyNeededInPartUom = QtyNeededInPartUom - item.Quantity;

                        }
                        else
                        {
                            if (PickItemRequestedQty > 0) //only run if there's more to be set from this requestedQty
                            {

                                //we have no items left to hold the remainder, so split again
                                ItemRemaining = MyExtensions.DeepCopyXML(item);
                                ItemRemaining.Status = "10";
                                ItemRemaining.Quantity = item.Quantity - PickItemRequestedQty; //in part UOM

                                ItemRemaining.PickItemID = "-1";


                                //first item
                                item.Status = "40";
                                item.Quantity = PickItemRequestedQty;


                                //item.Tag = fullTagForTrackingInfo;
                                item.Location = fullTagForTrackingInfo.Location;
                                item.Tracking = fullTagForTrackingInfo.Tracking;

                                //item.SourceTagID = long.Parse(fullTagForTrackingInfo.TagID);


                                selectedInvQty.Qty = selectedInvQty.Qty - PickItemRequestedQty;// itemQtyInProductUom;

                                PickItemRequestedQty = decimal.Zero; //filled this amount

                                //QtyNeededInPartUom = QtyNeededInPartUom - item.Quantity;
                            }

                        }


                    }
                }

                if (ItemRemaining != null)
                {
                    pick.PickItems.PickItem.Add(ItemRemaining);
                }






                await session.SavePick(pick);

                Pick newPick = await session.GetPick(PickNum);



                Assert.NotNull(newPick);
                Assert.True(pick.PickItems.PickItem.Count == newPick.PickItems.PickItem.Count);
                


            }

        }


        [TestCase("S10017")]
        public async Task LoadPickAndPickItemWithProductUomHandlesCorrectly(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);
                Assert.True(pick.PickItems.PickItem.Count >= 1);

                //pick item and save back

                PickItem itemToPick = pick.PickItems.PickItem.Where(pi => pi.Part.Num == "B201").FirstOrDefault();

                decimal QtyNeededInPickItemUom = itemToPick.Quantity; //usually sum if more than 1 pick item

                //invQty to pick from
                List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyGrouped = null; //list of inventory tags to choose from
                ProductSimpleObject scannedProduct = null;


                MySqlConfig dbConfig = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                                                                DatabaseUser, DatabasePassword, DatabaseName);

                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(dbConfig))
                {
                    scannedProduct = await db.getProduct("B201-cs2");

                    invQtyGrouped =
                        await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(scannedProduct.Number,
                        ValidDefaultLocationGroup, InventorySearchTermType.Product);


                }

                //select which one to use
                InvQtyGroupedByUniqueTagInfoWithTracking selectedInvQty =
                            invQtyGrouped
                                .Where(iq => iq.LocationName == "Stock 100")
                                .FirstOrDefault();

                Tag fullTagForTrackingInfo = await session.GetTagObjectAsync(selectedInvQty.SimpleTags[0].TagId.ToString());

                PickItem ItemRemaining = null;
                decimal SpecifiedQtyInProductUom = 1M; // pick 1 of cs2


                if (scannedProduct.UomId != itemToPick.UOM.UOMID)
                {
                    //convert the pick item to match the product
                    //would be foreach in SelectedPickItems if more than one
                    //itemToPick.

                }




                foreach (PickItem item in pick.PickItems.PickItem)
                {
                    Debug.WriteLine(item.Quantity);
                    if (item.Status != "40")
                    {

                        if (SpecifiedQtyInProductUom >= item.Quantity)
                        {
                            //we have enough
                            item.Status = "40";
                            item.Location = fullTagForTrackingInfo.Location;
                            item.Tracking = fullTagForTrackingInfo.Tracking;


                            //item.qty is unchanged, full amount
                            selectedInvQty.Qty = selectedInvQty.Qty - item.Quantity;
                            SpecifiedQtyInProductUom = SpecifiedQtyInProductUom - item.Quantity;


                        }
                        else
                        {
                            if (SpecifiedQtyInProductUom > 0) //only run if there's more to be set from this requestedQty
                            {

                                //we have no items left to hold the remainder, so split again
                                ItemRemaining = MyExtensions.DeepCopyXML(item);
                                ItemRemaining.Status = "10";
                                ItemRemaining.Quantity = item.Quantity - SpecifiedQtyInProductUom; //in part UOM

                                ItemRemaining.PickItemID = "-1";


                                //first item
                                item.Status = "40";
                                item.Quantity = SpecifiedQtyInProductUom;
                                item.Location = fullTagForTrackingInfo.Location;
                                item.Tracking = fullTagForTrackingInfo.Tracking;

                                selectedInvQty.Qty = selectedInvQty.Qty - SpecifiedQtyInProductUom;// itemQtyInProductUom;

                                SpecifiedQtyInProductUom = decimal.Zero; //filled this amount

                                //QtyNeededInPartUom = QtyNeededInPartUom - item.Quantity;
                            }

                        }


                    }
                }

                if (ItemRemaining != null)
                {
                    pick.PickItems.PickItem.Add(ItemRemaining);
                }






                await session.SavePick(pick);

                Pick newPick = await session.GetPick(PickNum);



                Assert.NotNull(newPick);
                Assert.True(pick.PickItems.PickItem.Count == newPick.PickItems.PickItem.Count);



            }

        }


        //no void pick to return FB back to the original state

        [Test]
        public async Task LoadPickListWithFilters()
        {

            PickListFilters pickListFilters = new PickListFilters { Status = PickStatus.AllOpen, Carrier = "Fedex-Parcel-Ground" };
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                List<PickSimpleObject> pickSimpleObjects = await session.GetPickSimpleList(pickListFilters, null,
                     "pick.datescheduled");


                Assert.NotNull(pickSimpleObjects);
                Assert.True(pickSimpleObjects.Count > 0);


            }
        }

        [TestCase(ValidPickNumber)]
        public async Task LoadPickByNumberAndSaveBackLeavesPickUnchanged(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            config.RequestTimeout = 30000;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);

                await session.SavePick(pick);

                Pick newPick = await session.GetPick(PickNum);

                pick.Should().BeEquivalentTo(newPick);



            }

        }

    }
}
