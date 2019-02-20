using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Json.QueryClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        public async Task<List<UserObject>> GetUserList()
        {
            string query = @"select Concat(sysuser.`firstName`, ' ' , sysuser.`lastName`) as FullName, sysuser.`username` as UserName" +
                " from sysuser where sysuser.`activeFlag` = 1 order by firstname";

            return await ExecuteQueryAsync<UserObject, UserObjectClassMap>(query);


        }
    }
}
