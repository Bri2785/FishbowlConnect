﻿using FishbowlConnect;
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
        const string DatabaseName = "gcs_copy";
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        //const string ValidPartNumberWithInventory = "CSBL030";
        const string ValidDefaultLocationGroup = "Main";
        //const string ValidPartNumberWithNoInventory = "";
        //const string ValidTrackingWithInventory = "$T$L18-19-8013";
        //const string ValidLocationWithInventory = "$L$WS5C";


        const string ValidPickNumber = "S20-306";

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

        [TestCase(ValidPickNumber)]
        public async Task LoadPickAndPickItemHandlesCorrectly(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);
                Assert.True(pick.PickItems.PickItem.Count >= 1);

                //pick item and save back

                PickItem itemToPick = pick.PickItems.PickItem.Where(pi => pi.Part.Num == "CSBR060").FirstOrDefault();


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
                InvQtyGroupedByUniqueTagInfoWithTracking selectedInvQty =
                            invQtyGrouped
                                .Where(iq => iq.LocationName == "SMT" && iq.TrackingEncoding == "WJpJOpM83Vsm1IV6wQExkg==")
                                .FirstOrDefault();

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
