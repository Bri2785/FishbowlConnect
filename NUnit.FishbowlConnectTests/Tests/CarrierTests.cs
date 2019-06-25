using FishbowlConnect;
using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    public class CarrierTests
    {
        [TestCase("FDX2D")]
        public async Task GetCarrierFromCodeExecuteQueryTest(string code)
        {
            SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");
            
            using (FishbowlSession session = new FishbowlSession(config))
            {
                session.DebugLevel = FishbowlConnectDebugLevel.Verbose;

                string query = string.Format(@"SELECT carrier.name AS CarrierName
                , carrierservice.`code` AS CarrierCode
                , carrierservice.`name` AS CarrierService
                FROM carrier
                LEFT JOIN carrierservice ON carrier.`id` = carrierservice.`carrierId`
                WHERE UPPER(carrierservice.code) LIKE '{0}'", code.ToUpper());



                List<CarrierSimple> carrierSimple = await session.ExecuteQueryAsync<CarrierSimple, CarrierSimpleClassMap>(query);

                if (carrierSimple?.Count > 1)
                {
                    throw new Exception("More than one carrier result returned, check the code supplied - " + code);
                }
                Assert.IsNotNull(carrierSimple);
            }



        }
    }
}
