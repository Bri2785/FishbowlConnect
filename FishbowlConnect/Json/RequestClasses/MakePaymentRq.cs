using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class MakePaymentRq
    {

        /// <remarks/>
        public Payment Payment { get; set; }

        /// <remarks/>
        public string ProcessPayment { get; set; }
    }
}
