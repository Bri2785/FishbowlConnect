using FishbowlConnect;
using FishbowlConnect.Json.APIObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class UomTests

    {

        const string GoodServerAddress = "192.168.125.26";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.125.26";
        const int DatabasePort = 3305;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "local_demo";

        [TestCase(22,1)]
        public async Task getUomConversion(int fromUomId, int toUomId)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);



            using (FishbowlSession session = new FishbowlSession(config))
            {
                UOMConversion uOMConversion =  await session.GetUOMConversion(fromUomId, toUomId);

                Assert.IsNotNull(uOMConversion);
            }
        }

        [TestCase(9)]
        public async Task getUomConversions(int fromUomId)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);



            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<UOMConversion> uOMConversions = await session.GetUOMConversions(fromUomId);

                Assert.IsNotNull(uOMConversions);

                Assert.True(uOMConversions.Count > 0);
            }
        }
    }
}
