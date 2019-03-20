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
    public class PickingTests
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string ValidPickNumber = "S71222";

        [TestCase(ValidPickNumber)]
        public async Task LoadPickByNumberReturnsPickObject(string PickNum)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNum);

                Assert.NotNull(pick);
                Assert.True(pick.PickItems.PickItem.Count == 1);

                
            }

        }
        
        //no void pick to return FB back to the original state
    }
}
