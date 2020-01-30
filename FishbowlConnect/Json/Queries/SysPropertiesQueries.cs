using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {

        //public Dictionary<string, string> getSysProperties()
        //{

        //}


        /// <summary>
        /// Get a single property from Fishbowl
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns>sysPropertyValue</returns>
        /// <exception cref="KeyNotFoundException">Throws KeyNotFound when no records returned</exception>
        public async Task<string> GetSysProperty(string propertyName)
        {
            string query = string.Format(@"Select sysValue from sysProperties where sysKey = '{0}'", propertyName);

            return await ExecuteQueryAsync(query);

        }
    }
}
