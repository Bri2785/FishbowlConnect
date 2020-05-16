using FishbowlConnect;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class PartTests
    {
        const string GoodServerAddress = "192.168.125.205";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.125.205";
        const int DatabasePort = 3305;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "local_demo_20_2";

        [Test]
        public async Task GetPartDefaultLocationsReturnsAllLgs()
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<PartDefaultLocationObject> locations = await session.GetPartDefaultLocations("APP010");

                Assert.True(locations.Count == 6);

            }


        }
    }
}
