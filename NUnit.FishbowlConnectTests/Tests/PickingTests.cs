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
using System.Threading;

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

                                ItemRemaining.PickItemID = -1;


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


        [TestCase("S71243", "03403", "F41B")]
        public async Task LoadPickAndPickItemWithCommitOnlyHandlesCorrectly(string PickNum, string partNumToPick, string locationToPickFrom, string lotNumber = null, string trackingEncoding = null)
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
                                .Where(iq => iq.LocationName == locationToPickFrom && iq.TrackingEncoding == (trackingEncoding ?? ""))
                                .FirstOrDefault();
                }


                //
                //.Where(iq => iq.LocationName == "M1" && iq.TrackingEncoding == "32/wVwpZJxIup2uaQHhHQw==")

                Tag fullTagForTrackingInfo = await session.GetTagObjectAsync(selectedInvQty.SimpleTags[0].TagId.ToString());

                PickItem ItemRemaining = null;
                decimal PickItemRequestedQty = itemToPick.Quantity;

                //pick.StatusID = "30"; //all committed

                foreach (PickItem item in pick.PickItems.PickItem)
                {
                    Debug.WriteLine(item.Quantity);
                    if (item.Status != "40" && item.Status != "30") //add committed ignore
                    {

                        if (PickItemRequestedQty >= item.Quantity) //selecting all in available in this invQty line
                        {
                            //we have enough
                            item.Status = "30";
                            item.Location = fullTagForTrackingInfo.Location;
                            item.Tracking = fullTagForTrackingInfo.Tracking;
                            item.Tag = fullTagForTrackingInfo; //required for committing

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

                                ItemRemaining.PickItemID = -1;


                                //first item
                                item.Status = "30";
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

                                ItemRemaining.PickItemID = -1;


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



        [TestCase("S10085")]
        public async Task LoadPickByNumberResetItemsHandlesCorrectly(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            config.RequestTimeout = 30000;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);

                List<PickItem> SelectedPickItems = MyExtensions.DeepCopyXML(pick.PickItems.PickItem.ToList());

                var grouped = SelectedPickItems.OrderByDescending(pi => pi.PickItemID)
                                        .GroupBy(pi => pi, new PickItemComparerIncludingTracking())
                                        ;


                Assert.That(grouped.Count() == 3); //line item 1, line item 2, and the short qty

                foreach (var group in grouped)
                {
                    Assert.That(group.Key.PickItemID > 0);
                }


                //await session.SavePick(pick);


            }

        }

        [TestCase(62759)]
        public async Task VoidPickReturnsVoidedPick(int pickId)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            config.RequestTimeout = 30000;

            using (FishbowlSession session = new FishbowlSession(config))
            {
                VoidPickResponse voidResponse = await session.VoidPick(pickId);
                Assert.NotNull(voidResponse);
                Assert.NotNull(voidResponse.VoidedPick);
                Assert.That(voidResponse.VoidedPick.StatusID == "10");
                //TODO: add unvoidable check test for different pick
            }
        }

        [TestCase("S10084")]
        public async Task LoadPickByNumberResetItemsWithTrackingHandlesCorrectly(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            config.RequestTimeout = 30000;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);

                List<PickItem> SelectedPickItems = MyExtensions.DeepCopyXML(pick.PickItems.PickItem.ToList());

                var grouped = SelectedPickItems.OrderByDescending(pi => pi.PickItemID)
                                        .GroupBy(pi => pi, new PickItemComparerIncludingTracking())
                                        ;


                Assert.That(grouped.Count() == 2); //(line item 1 and line item 2), and then line item 3
                Dictionary<PickItem, decimal> dictQty = new Dictionary<PickItem, decimal>();

                foreach (var group in grouped)
                {
                    Assert.That(group.Key.PickItemID > 0);

                    decimal totalGroupQty = 0M;
                    foreach (var item in group)
                    {
                        totalGroupQty += item.Quantity;
                    }

                    dictQty.Add(group.Key, totalGroupQty);
                    group.Key.Quantity = group.Sum(pig => pig.Quantity);
                }

                ///The linq query is re-executed everytime. the key is still part of the list since we used the full item and it still
                ///holds a reference. So when we sum the items in the list after we summed the total list, the sums compound 
                ///and wont equal in the Assert. It still works, since we are only interested in the first grouping and we can use that for our 
                ///printint, etc

                Assert.That(grouped.Select(g => g.Key).ToList().Count == 2);
                foreach (var key in grouped.Select(g => g.Key).ToList())
                {
                    Assert.That(dictQty.GetValueOrDefault(key) == key.Quantity);
                }
            }

        }


        [TestCase("S10086")] //single tracking
        [TestCase("S10088")] //all tracking and 2 items
        [TestCase("S10085")]
        public async Task PickAllItemsAndFindInReturnedSavedPick(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Pick OriginalPick = MyExtensions.DeepCopyXML(pick);

                Assert.NotNull(pick);
                Assert.True(pick.PickItems.PickItem.Count > 0);


                do //loop through all pick items, select a part, pick those, select the next part, etc
                {
                    string partNumToPick;
                    try
                    {
                        partNumToPick = pick.PickItems.PickItem.Where(i => i.Status != "40"
                                                                                   && i.Status != "30"
                                                                                   && i.Status != "5").FirstOrDefault().Part?.Num;
                    }
                    catch (Exception)
                    {
                        throw new ArgumentNullException("No open lines to pick");
                    }

                    //get other items that match
                    List<PickItem> matches = pick.PickItems.PickItem
                    .Where(i => i.Part.Num == partNumToPick
                    && i.Status != "40"
                    && i.Status != "30"
                    && i.Status != "5").ToList();

                    List<PickItem> SelectedPickItems = MyExtensions.DeepCopyXML(matches);


                    //invQty to pick from
                    List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyGrouped = null; //list of inventory tags to choose from

                    MySqlConfig dbConfig = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                                                                    DatabaseUser, DatabasePassword, DatabaseName);

                    using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(dbConfig))
                    {
                        invQtyGrouped =
                            await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(SelectedPickItems[0]?.Part?.Num,
                            null, InventorySearchTermType.Part);
                    }


                    do //loop through selected sub set items and pick all
                    {
                        //select which one to use
                        InvQtyGroupedByUniqueTagInfoWithTracking selectedInvQty = null;

                        //grab any line
                        selectedInvQty = invQtyGrouped.FirstOrDefault();

                        Tag fullTagForTrackingInfo = await session.GetTagObjectAsync(selectedInvQty.SimpleTags[0].TagId.ToString());

                        PickItem ItemRemaining = null;
                        decimal PickItemRequestedQty = 0m;

                        if (selectedInvQty.Qty.CompareTo(SelectedPickItems.Sum(pi => pi.Quantity)) <= 0)
                        {
                            PickItemRequestedQty = selectedInvQty.Qty;
                        }
                        else
                        {
                            PickItemRequestedQty = SelectedPickItems.Sum(pi => pi.Quantity);
                        }

                        foreach (PickItem item in SelectedPickItems)
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

                                }
                                else
                                {
                                    if (PickItemRequestedQty > 0) //only run if there's more to be set from this requestedQty
                                    {

                                        //we have no items left to hold the remainder, so split again
                                        ItemRemaining = MyExtensions.DeepCopyXML(item);
                                        ItemRemaining.Status = "10";
                                        ItemRemaining.Quantity = item.Quantity - PickItemRequestedQty; //in part UOM

                                        ItemRemaining.PickItemID = -1;


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

                        if (selectedInvQty.Qty == 0)
                        {
                            invQtyGrouped.Remove(selectedInvQty);
                        }

                        if (ItemRemaining != null)
                        {
                            SelectedPickItems.Add(ItemRemaining);
                        }



                    } while (SelectedPickItems.Where(i => i.Status != "40"
                                                        && i.Status != "30"
                                                        && i.Status != "5").ToList().Count > 0);



                    //replace the original selected pick items with our finished pick item list
                    foreach (var item in matches)
                    {
                        pick.PickItems.PickItem.Remove(item);
                    }
                    foreach (var item in SelectedPickItems)
                    {
                        pick.PickItems.PickItem.Add(item);
                    }


                    //Selcted pick items contain our split ones and may not have a pickItemId.
                    //save the pick and then see if we can find the returned saved ones in the list
                    //to get the pickItemID to use for printing

                    pick = await session.SavePick(pick);

                    //SelectedPickItems.Where(si => pick.PickItems.PickItem.Any(p => PickItemMatches(si, p))).Count();

                    foreach (var item in SelectedPickItems)
                    {
                        //find single line in returned pick
                        var matchcount = pick.PickItems.PickItem.Where(p => PickItemMatches(item, p) == true).Count();
                        Assert.That(matchcount == 1);

                        var matchItem = pick.PickItems.PickItem.FirstOrDefault(p => PickItemMatches(item, p) == true);
                        Assert.That(matchItem != null); //finds the first one. This will not error if there are more than one match
                                                        //Case where user selects 2 partial picks with the same everything.
                    }

                    Thread.Sleep(2000); //pause before picking again


                    //keep picking until all items are picked
                } while (pick.PickItems.PickItem.Where(i => i.Status != "40"
                                                            && i.Status != "30"
                                                            && i.Status != "5").ToList().Count > 0);





                await session.VoidPick(pick.PickID);

                Pick newPick = await session.GetPick(PickNum);
                Assert.NotNull(newPick);
                Assert.True(OriginalPick.PickItems.PickItem.Count == newPick.PickItems.PickItem.Count);


            }


        }

        [TestCase("S10087")]
        public async Task VerifyPickedTrackingEncodingMatchesFishbowl(string PickNum)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);
                Assert.True(pick.PickItems.PickItem.Count >= 1);


                string partNumToPick = pick.PickItems.PickItem.Where(i => i.Status != "40"
                                                        && i.Status != "5").FirstOrDefault().Part?.Num;

                PickItem item = pick.PickItems.PickItem.FirstOrDefault();

                //invQty to pick from
                List<InvQtyGroupedByUniqueTagInfoWithTracking> invQtyGrouped = null; //list of inventory tags to choose from

                MySqlConfig dbConfig = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                                                                DatabaseUser, DatabasePassword, DatabaseName);

                using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(dbConfig))
                {
                    invQtyGrouped =
                        await db.GetPartInvGroupedWithAllTrackingWithDefaultLocation(partNumToPick,
                        ValidDefaultLocationGroup, InventorySearchTermType.Part);
                }


                InvQtyGroupedByUniqueTagInfoWithTracking selectedInvQty = null;

                //grab only line
                selectedInvQty = invQtyGrouped.FirstOrDefault();

                Tag fullTagForTrackingInfo = await session.GetTagObjectAsync(selectedInvQty.SimpleTags[0].TagId.ToString());


                decimal PickItemRequestedQty = 20;

                if (PickItemRequestedQty >= item.Quantity) //selecting all in available in this invQty line
                {
                    //we have enough
                    item.Status = "40";
                    item.Location = fullTagForTrackingInfo.Location;
                    item.Tracking = fullTagForTrackingInfo.Tracking;


                    //item.qty is unchanged, full amount
                    selectedInvQty.Qty = selectedInvQty.Qty - item.Quantity;
                    PickItemRequestedQty = PickItemRequestedQty - item.Quantity;

                }

                PickItem preSaveItem = MyExtensions.DeepCopyXML(item);

                pick = await session.SavePick(pick);

                Assert.That(pick.PickItems.PickItem.FirstOrDefault().Tag.Tracking.getEncoding() == preSaveItem.Tracking.getEncoding());

                Assert.That(PickItemMatches(preSaveItem, pick.PickItems.PickItem.FirstOrDefault()));

                Assert.That(pick.PickItems.PickItem.Where(p => PickItemMatches(item, p)).Count() == 1);



                Pick newPick = (await session.VoidPick(pick.PickID)).VoidedPick;


                Assert.NotNull(newPick);
                Assert.True(pick.PickItems.PickItem.Count == newPick.PickItems.PickItem.Count);


            }

        }

        private bool PickItemMatches(PickItem beforeSave, PickItem afterSave)
        {
            bool locationMatches = false;
            if (beforeSave.Location != null && afterSave.Location != null)
            {
                locationMatches = beforeSave.Location.FullLocation.Equals(afterSave.Location.FullLocation);
            }
            else
            {
                locationMatches = true; //both null
            }
            bool matches =
            beforeSave.Part?.PartID == afterSave.Part?.PartID &&
                beforeSave.SoItemId == afterSave.SoItemId &&
                beforeSave.PoItemId == afterSave.PoItemId &&
                beforeSave.XoItemId == afterSave.XoItemId &&
                beforeSave.WoItemId == afterSave.WoItemId &&
                beforeSave.Status.Equals(afterSave.Status) &&
                beforeSave.Quantity.Equals(afterSave.Quantity) &&
                locationMatches  &&
                beforeSave.Tracking?.getEncoding() == afterSave.Tracking?.getEncoding();
            return matches;
        }

        [TestCase(25)]
        public async Task VoidPickReturnsUnvoidableMessage(int pickId)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            config.RequestTimeout = 30000;

            using (FishbowlSession session = new FishbowlSession(config))
            {
                VoidPickResponse voidResponse = await session.VoidPick(pickId);
                Assert.NotNull(voidResponse);
                Assert.NotNull(voidResponse.VoidedPick);
                Assert.That(voidResponse.VoidedPick.StatusID == "10");
                Assert.That(!string.IsNullOrEmpty(voidResponse.UnVoidableItems));
                Debug.WriteLine(voidResponse.UnVoidableItems);

            }

        }

        [TestCase("S10083")]
        public async Task VoidPickItemReturnsReloadedPickWithVoidedItem(string pickNum)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            config.RequestTimeout = 30000;

            using (FishbowlSession session = new FishbowlSession(config))
            {
                Pick pick = await session.GetPick(pickNum);

                List<PickItem> itemsToVoid = new List<PickItem>();
                PickItem itemToVoid = pick.PickItems.PickItem.FirstOrDefault(i => i.PickItemID == 171074);
                itemsToVoid.Add(itemToVoid);



                VoidPickResponse voidResponse = await session.VoidPickItems(pick, itemsToVoid);
                Assert.NotNull(voidResponse);
                Assert.NotNull(voidResponse.VoidedPick);
                Assert.That(voidResponse.VoidedPick.PickItems.PickItem
                                .FirstOrDefault(i => i.PickItemID == itemToVoid.PickItemID)?.Status == "10");
                Assert.That(string.IsNullOrEmpty(voidResponse.UnVoidableItems));

            }

        }

        [TestCase("S10013")]
        public async Task VoidPickItemReturnsUnvoidableMessage(string pickNum)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            config.RequestTimeout = 30000;

            using (FishbowlSession session = new FishbowlSession(config))
            {
                Pick pick = await session.GetPick(pickNum);

                List<PickItem> itemsToVoid = new List<PickItem>();
                PickItem itemToVoid = pick.PickItems.PickItem.FirstOrDefault(i => i.Status == "40");
                itemsToVoid.Add(itemToVoid);



                VoidPickResponse voidResponse = await session.VoidPickItems(pick, itemsToVoid);
                Assert.NotNull(voidResponse);
                Assert.NotNull(voidResponse.VoidedPick);
                Assert.That(voidResponse.VoidedPick.PickItems.PickItem
                                .FirstOrDefault(i => i.PickItemID == itemToVoid.PickItemID)?.Status == "40"); //not voided since shipped
                Assert.That(!string.IsNullOrEmpty(voidResponse.UnVoidableItems));
                Debug.WriteLine(voidResponse.UnVoidableItems);

            }

        }
    }
}
