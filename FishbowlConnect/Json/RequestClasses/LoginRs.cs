using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json
{
    public class LoginRs : IRs
    {
        public string UserFullName { get; set; }

        public Module ModuleAccess { get; set; }

        public string ServerVersion { get; set; }
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }
}
