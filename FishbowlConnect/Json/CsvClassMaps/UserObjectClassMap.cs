using CsvHelper.Configuration;
using FishbowlConnect.Json.QueryClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishbowlConnect.Json.CsvClassMaps
{
    public class UserObjectClassMap : ClassMap<UserObject>
    {
        public UserObjectClassMap()
        {
            AutoMap();
        }
    }
}
