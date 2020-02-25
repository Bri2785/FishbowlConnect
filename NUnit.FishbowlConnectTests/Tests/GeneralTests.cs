using FishbowlConnect;
using FishbowlConnect.Json.Imports;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    class GeneralTests
    {
        const string GoodServerAddress = "192.168.150.9";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.150.9";
        const int DatabasePort = 3301;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";


        [TestCase("ImportSalesOrder")]
        public async Task GetHeaderRowForImport(string importType)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<string> header = await session.getImportHeaderRowAsync(importType);

                Assert.IsNotNull(header);

            }


        }
    }
}
