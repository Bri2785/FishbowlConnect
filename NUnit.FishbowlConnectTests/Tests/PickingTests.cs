using FishbowlConnect;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class PickingTests
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "holl30";

        const string ValidPickNumber = "S19-7478";

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

        //no void pick to return FB back to the original state

        [Test]
        public async Task LoadPickListWithFilters( )
        {

            PickListFilters pickListFilters = new PickListFilters { Status = PickStatus.AllOpen, Carrier = "Fedex-Parcel-Ground" };
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                List < PickSimpleObject > pickSimpleObjects = await session.GetPickSimpleList(pickListFilters,null,
                     "pick.datescheduled");


                Assert.NotNull(pickSimpleObjects);
                Assert.True(pickSimpleObjects.Count > 0);


            }
        }

        [TestCase(ValidPickNumber)]
        public async Task LoadPickByNumberAndSaveBackLeavesPickUnchanged(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


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
