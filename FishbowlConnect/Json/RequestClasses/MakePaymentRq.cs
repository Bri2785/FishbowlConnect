using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class MakePaymentRq
    {

        /// <remarks/>
        public Payment Payment { get; set; }

        /// <remarks/>
        public string ProcessPayment { get; set; }
    }
}
