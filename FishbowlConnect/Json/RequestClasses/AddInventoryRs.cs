﻿using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.Requests
{
    public class AddInventoryRs :IRs
    {
        public Tag Tag { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
