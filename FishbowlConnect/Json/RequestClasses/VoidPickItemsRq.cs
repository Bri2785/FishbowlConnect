using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.RequestClasses
{
    public class VoidPickItemsRq
    {
        public Pick Pick { get; set; }
        public ItemList ItemList { get; set; }
    }
}
