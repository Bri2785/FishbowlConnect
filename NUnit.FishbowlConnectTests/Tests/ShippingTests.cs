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
    public class ShippingTests
    {

        const string GoodServerAddress = "192.168.125.26";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string ValidShipNumber = "S10014";

        [TestCase(ValidShipNumber)]
        public async Task LoadShipmentDetailsAndGetFlattenedListOFItems(string shipNum)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Shipping shipment = await session.getShipment(shipNum);

                Assert.NotNull(shipment);
                Assert.True(shipment.ItemsFlattened.Count == 6);
                //do we group them by product
            }

        }

        [TestCase("S10014")]
        [TestCase("S10017")]
        [TestCase("S10083")]
        public async Task LoadAndPackShipmentIsSuccess(string shipNum)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Shipping shipment = await session.getShipment(shipNum);

                
                Assert.NotNull(shipment);
                shipment.Status = "20"; //packed

                await session.SaveShipment(shipment);

                Shipping packedShipment = await session.getShipment(shipNum);
                Assert.That(packedShipment.Status == "20");
            }

        }


    }
}
