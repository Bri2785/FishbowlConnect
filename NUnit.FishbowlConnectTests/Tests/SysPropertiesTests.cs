using FishbowlConnect;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class SysPropertiesTests
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 2361;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";

        //const string ValidPartNumberWithInventory = "ECL-SC";

        [Test]
        public async Task AddNewSysPropertyShouldSave()
        {
            string propertyName = "testProperty";

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);



            using (FishbowlSession session = new FishbowlSession(config))
            {
                await session.SetSysProperty(propertyName, "my test string");

                string returnValue = await session.GetSysProperty(propertyName);

                Assert.IsNotNull(returnValue);
            }


        }
    }
}
