using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.MySQL
{
    public class MySqlConfig
    {
        public string MySqlServerHost { get; set; }
        public string MySqlPort { get; set; }
        public string MySqlUser { get; set; }
        public string MySqlPassword { get; set; }
        public string MySqlDatabase { get; set; }
        public string SSlMode { get; set; }

        public MySqlConfig()
        {

        }

        public MySqlConfig(string host, string port, string user, string password, string database, string sslMode = "None")
        {
            MySqlServerHost = host;
            MySqlPort = port;
            MySqlUser = user;
            MySqlPassword = password;
            MySqlDatabase = database;
            SSlMode = sslMode;
        }

        public override string ToString()
        {
            return @"SERVER=" + MySqlServerHost +
                                ";PORT=" + MySqlPort +
                                ";DATABASE=" + MySqlDatabase +
                                ";UID=" + MySqlUser +
                                ";PASSWORD=" + MySqlPassword +
                                ";SslMode=" + SSlMode;
        }
    }
}
