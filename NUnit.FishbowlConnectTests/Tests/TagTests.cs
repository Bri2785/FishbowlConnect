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
    public class TagTests
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


        const string ValidPickNumber = "T20-11";
        const int ValidTagId = 72124;



        [TestCase(ValidTagId)]
        public async Task GetTagSerializesCorrectly(int tagId)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Tag fullTagForTrackingInfo = await session.GetTagObjectAsync(tagId.ToString());




            }

        }
    }
}
