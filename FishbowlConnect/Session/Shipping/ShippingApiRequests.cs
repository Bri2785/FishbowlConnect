using FishbowlConnect.Json.Requests;
using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Return a shipping object containing the requested shipment
        /// </summary>
        /// <param name="shipmentID">Ship ID</param>
        /// <returns></returns>
        public async Task<Shipping> getShipment(int shipmentID)
        {
            GetShipmentRq getShipmentRq = new GetShipmentRq();

            if (shipmentID <= 0)
            {
                throw new ArgumentException(shipmentID + " is not a valid Shipment ID");
            }

            getShipmentRq.ShipmentID = shipmentID;

            GetShipmentRs getShipmentRs = await IssueJsonRequestAsync<GetShipmentRs>(getShipmentRq);

            return getShipmentRs?.Shipping;
        }

        /// <summary>
        /// Return a shipping object containing the requested shipment
        /// </summary>
        /// <param name="shipmentNum">The shipment number</param>
        /// <returns></returns>
        public async Task<Shipping> getShipment(string shipmentNum)
        {
            GetShipmentRq getShipmentRq = new GetShipmentRq();

            getShipmentRq.ShipmentNum = shipmentNum ?? throw new ArgumentException(shipmentNum + " is not a valid Shipment Num");

            GetShipmentRs getShipmentRs = await IssueJsonRequestAsync<GetShipmentRs>(getShipmentRq);

            return getShipmentRs?.Shipping;

        }

        /// <summary>
        /// Saves shipment changes to FB
        /// </summary>
        /// <param name="shipment">Populated shipping object</param>
        /// <returns></returns>
        public async Task SaveShipment(Shipping shipment)
        {
            SaveShipmentRq saveShipmentRq = new SaveShipmentRq();

            saveShipmentRq.Shipping = shipment ?? throw new ArgumentNullException("Shipment cannot be null");

            SaveShipmentRs saveShipmentRs = await IssueJsonRequestAsync<SaveShipmentRs>(saveShipmentRq);

            
        }

        /// <summary>
        /// Packs a shipment (changes status only)
        /// </summary>
        /// <param name="shipment"></param>
        /// <returns></returns>
        public async Task PackShipment(Shipping shipment)
        {
            shipment.Status = "20";
            await SaveShipment(shipment);


        }

        /// <summary>
        /// Packs a shipment (changes status only)
        /// </summary>
        /// <param name="shipNum"></param>
        /// <returns></returns>
        public async Task PackShipment(string shipNum)
        {
            Shipping shipment = await getShipment(shipNum);
            
            shipment.Status = "20";
            await SaveShipment(shipment);


        }

        /// <summary>
        /// Ships an order in FB. Uploads the provided image as a signature if added. Auto fulfills any service items also
        /// </summary>
        /// <param name="ShipNum">"Shipment number, must include the prefix"</param>
        /// <param name="image">"Base64 encoded image to attach to shipment. Usually used for signatures</param>
        /// <returns></returns>
        public async Task Ship(string ShipNum, string image = null)
        {
            ShipRq shipRq = new ShipRq();

            shipRq.ShipNum = ShipNum ?? throw new ArgumentNullException("Shipment Number must not be blank");
            shipRq.ShipDate = DateTime.Now;
            shipRq.FulfillService = true;

            if (!string.IsNullOrEmpty(image))
            {
                shipRq.Image = image;
            }

            ShipRs shipRs = await IssueJsonRequestAsync<ShipRs>(shipRq);


        }


    }
}
