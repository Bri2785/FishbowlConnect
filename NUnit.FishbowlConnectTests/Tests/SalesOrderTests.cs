using FishbowlConnect;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{
    [TestFixture]
    public class SalesOrderTests
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 2361;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";


        [TestCase("71224")]
        public async Task GetSalesOrderObjectSuccessfully(string salesOrderNumber)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession session = new FishbowlSession(config))
            {
                SalesOrder so = await session.GetSalesOrder(salesOrderNumber);

                Assert.IsNotNull(so);

            }


        }

        [TestCase("71224")]
        public async Task GetSalesOrderAndSaveItBackUsingImport(string salesOrderNumber)
        {

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession session = new FishbowlSession(config))
            {
                SalesOrder so = await session.GetSalesOrder(salesOrderNumber);

                Assert.IsNotNull(so);

                //clear number so FB generates a new one
                so.Number = null;

                so.CustomField = new List<CustomField>();
                CustomField customField = new CustomField
                {
                    Name = "Repair Order",
                    Info = "1"
                };
                so.CustomField.Add(customField);

                so.CarrierName = "Fedex";
                so.CarrierService = "2 Day";
                await session.ImportSalesOrderAsync(so);

                //check the so import response to see if it returns the new number -> it does not, will need different method

                //SalesOrder newSO = await session.GetSalesOrder("71227");

                //Assert.IsNotNull(newSO);
            }


        }

    }

}
