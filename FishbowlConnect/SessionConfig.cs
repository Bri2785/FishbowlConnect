using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect
{
    public class SessionConfig
    {
        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string APIUser { get; set; }
        public string APIPassword { get; set; }
        public int RequestTimeout { get; set; }

        public SessionConfig()
        {

        }
        public SessionConfig(string ServerAddress, int port, string apiUser, string apiPassword, int RequestTimeout = 5000)
        {
            this.ServerAddress = ServerAddress;
            this.ServerPort = port;
            this.APIUser = apiUser;
            this.APIPassword = apiPassword;
            this.RequestTimeout = RequestTimeout;
        }
    }
}
