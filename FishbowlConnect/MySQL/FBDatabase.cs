using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace REEL
{ 
    public class FBDatabase
    {
        private static MySqlConnection _mySQLConnection;

        public MySqlConnection MySQLConnection
        {
            get
            {
                if (_mySQLConnection == null)
                {
                    _mySQLConnection = new MySqlConnection(@"SERVER=192.168.150.2;PORT=3305;DATABASE=briteideasupdate;UID=gone;PASSWORD=fishing");

                }
                if (_mySQLConnection.State != ConnectionState.Open)
                {
                    _mySQLConnection.Open();
                }
                return _mySQLConnection;
            }
        }

        public FBDatabase()
        {
            //add in to check if null, if so create new. if it still exists (old connection) but is closed then reopen

            _mySQLConnection = new MySqlConnection(@"SERVER=192.168.150.2;PORT=3305;DATABASE=briteideasupdate;UID=gone;PASSWORD=fishing");

            _mySQLConnection.Open();

            if (_mySQLConnection.State != ConnectionState.Open)
            {
                _mySQLConnection = null;
            }

        }

        public void CloseConnection()
        {
            _mySQLConnection.Close();
            _mySQLConnection.Dispose();
        }
    }
}
