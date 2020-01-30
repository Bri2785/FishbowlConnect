using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.RequestClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {

        public async Task SetSysProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("System property name required.");
            }
            if (string.IsNullOrEmpty(propertyValue))
            {
                throw new ArgumentNullException("System property value required.");
            }

            SetSystemPropertyRq rq = new SetSystemPropertyRq();

            SystemProperty prop = new SystemProperty { Name = propertyName, Value = propertyValue };
            rq.PropertyList = new PropertyList { SystemProperty = new List<SystemProperty> { prop } };

            await IssueJsonRequestAsync<SetSystemPropertyRs>(rq);

        }
    }
}
