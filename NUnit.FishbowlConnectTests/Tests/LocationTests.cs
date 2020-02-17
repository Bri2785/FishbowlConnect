using FishbowlConnect;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class LocationTests
    {
        const string GoodServerAddress = "192.168.125.26";
        const string GoodUserName = "bnordstrom";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.125.26";
        const int DatabasePort = 3305;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "local_demo";




        [Test]
        public async Task GetLocationGroupListTest()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession _session = new FishbowlSession(config))
            {

                var lGList = await _session.GetLocationGroupList();

                Assert.NotNull(lGList);
            }

        }

        [Test]
        public async Task GetLocationGroupListByUserIdTest()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession _session = new FishbowlSession(config))
            {
                
                var lGList = await _session.GetUserLocationGroupList("6");

                Assert.NotNull(lGList);
            }

        }
        [Test]
        public async Task GetLocationGroupListByUserNameTest()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession _session = new FishbowlSession(config))
            {

                var lGList = await _session.GetUserLocationGroupList("bnordstrom");

                Assert.NotNull(lGList);

                Assert.IsTrue(lGList.Count == 2);
            }

        }

        [TestCase("A1A", "Main Warehouse")]
        public async Task GetLocationSimpleTest(string LocationName, string LocationGroupName)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            LocationSimpleObject locationSimple;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                locationSimple = await session.GetLocationSimple(LocationGroupName,
                    LocationName);

                Assert.IsInstanceOf<LocationSimpleObject>(locationSimple);
            }


        }

        [TestCase("Main Warehouse")]
        public async Task GetLocationSimpleListTest(string LocationGroupName)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            List<LocationSimpleObject> locationSimple;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                locationSimple = await session.GetLocationSimpleList(LocationGroupName);

                Assert.IsInstanceOf<List<LocationSimpleObject>>(locationSimple);
            }


        }


    }
}
