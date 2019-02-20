using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.MySQL
{
    public class ProductPricingLevel
    {
        public string ProductNumber { get; set; }
        public string Group { get; set; }
        public decimal LevelPrice { get; set; }
        public string LevelPriceCurrency
        {
            get
            {
                return string.Format("{0:C}", LevelPrice);
            }
        }
    }
}
