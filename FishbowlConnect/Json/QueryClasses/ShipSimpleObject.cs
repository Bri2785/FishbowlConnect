using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.QueryClasses
{
    public class ShipSimpleObject
    {
        string _dateShipped;

        public int ShipId { get; set; }
        public string ShipNum { get; set; }
        public string OrderInfo { get; set; }
        public string PONumber { get; set; }
        public string Carrier { get; set; }


        public ShipStatus ShipStatus { get; set; }
        public string DateShipped {
            get
            {
                return Convert.ToDateTime(_dateShipped).ToShortDateString();
            }
            set
            {
                _dateShipped = value;
            }
        }
        public int CartonCount { get; set; }


    }
}
