using FishbowlConnect;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class InventoryTests
    {
        const string GoodServerAddress = "192.168.150.2";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 3301;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";


        [TestCase("TCB081PK-BranchSet")]
        public async Task AddInventoryWithNoPreviousCostCompletesSuccessfully(string partNumber)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);



            using (FishbowlSession session = new FishbowlSession(config))
            {
                await session.AddInventoryImportAsync(partNumber, 2, "A2A", "Main Warehouse", "Test Note",
                    new List<IPartTrackingFields> { new PartNumAndTracks{ PartNumber = partNumber,
                     TrackingAbbr ="Gen",
                     TrackingID = 5,
                     TrackingInfo = "3",
                     TrackingLabel ="Generation",
                     TrackingPrimaryFlag = false,
                     TrackingSortOrder = 1,
                     TrackingTypeID = 70} });
            }


        }
    }

}
