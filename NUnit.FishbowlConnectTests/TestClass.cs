using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishbowlConnect;
using System.Threading;
using System.Diagnostics;
using FishbowlConnect.MySQL;
using FishbowlConnect.Interfaces;
using FishbowlConnect.QueryClasses;
using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Exceptions;
using FishbowlConnect.Json.QueryClasses;
using static FishbowlConnect.FBHelperClasses;
using FishbowlConnect.Json.Requests;
using FishbowlConnect.Json.APIObjects;
using System.IO;

namespace NUnit.FishbowlConnectTests
{
    [TestFixture]
    public class TestClass
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";
        const string BadServerAddress = "127.5.4.3";
        const string BadUserName = "sjjsd";
        const string BadPassword = "jfjsdf";
        const string NoUserName = null;
        const string NoPassword = null;

        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 2361;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";
        const string BadDatabaseName = "fndfnd";

        const string ValidPartNumberWithInventory = "ECL-SC";

        [Test]
        public void SessionTest()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession _session = new FishbowlSession(config))
            {
                Assert.IsInstanceOf(typeof(FishbowlSession), _session);
                Assert.IsTrue(_session.IsConnected);
            }

        }


        [TestCase(GoodUserName, GoodPassword, GoodServerAddress)]
        [TestCase(BadUserName, BadPassword, GoodServerAddress)]
        [TestCase(GoodUserName, GoodPassword, BadServerAddress)]
        [TestCase(NoUserName, GoodPassword, GoodServerAddress)]
        [TestCase(GoodUserName, NoPassword, GoodServerAddress)]
        public async Task LoginJsonTest(string UserName, string password, string serveraddress)
        {
            SessionConfig config = new SessionConfig(serveraddress, 28192, UserName, password,5000);
            //for (int i = 0; i < 15; i++)
            //{
            using (FishbowlSession session = new FishbowlSession(config))
            {
                try
                {

                    await session.LoginJson();

                    Assert.IsTrue(session.IsAuthenticated);

                    await session.LogoutJson();

                }
                catch (FishbowlAuthException ae)
                {
                    Assert.IsTrue(ae.StatusCode == "1120"); //bad user name or password
                }
                catch (FishbowlConnectionException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            //Thread.Sleep(1000);
            //}

        }


        [TestCase(DatabaseAddress, DatabasePort, DatabaseUser, DatabasePassword, DatabaseName)]
        [TestCase(BadServerAddress, DatabasePort, DatabaseUser, DatabasePassword, DatabaseName)]
        [TestCase(DatabaseAddress, DatabasePort, BadUserName, DatabasePassword, DatabaseName)]
        [TestCase(DatabaseAddress, DatabasePort, DatabaseUser, BadPassword, DatabaseName)]
        [TestCase(DatabaseAddress, DatabasePort, DatabaseUser, DatabasePassword, BadDatabaseName)]
        [TestCase(DatabaseAddress, DatabasePort, DatabaseUser, DatabasePassword, null)]
        public async Task MySQLConnectionTest(string Address, int port, string User, string password, string database)
        {
            MySqlConfig config = new MySqlConfig(Address, port.ToString(), User, password, database);
            try
            {


                using (var db = await FishbowlMySqlDB.CreateAsync(config))
                {
                    //await db.TestConnection();
                    Assert.IsNotNull(db.Connection);
                    Assert.IsNotNull(db.Connection.ServerVersion);

                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [TestCase("100GCL", true)]
        [TestCase("LED-SB24BL", false)]
        [TestCase("LED-SBddddd", false)]
        public async Task GetPartDefaultLocation(string partNumber, bool locationSet)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Location1 location = await session.GetPartDefaultLocationAsync(partNumber);

                if (locationSet)
                {
                    Assert.IsNotNull(location);
                }
                else
                {
                    Assert.IsNull(location);
                }
            }

        }

        #region Part
        [TestCase("100GCL", "N9A", "Main Warehouse")]
        [TestCase("LED-SB24BL", "N9A", "Main Warehouse")]
        [TestCase("LED-SBddddd", "N9A", "Main Warehouse")]
        public async Task PartSetDefaultLocation(string partNumber, string location, string locationgroup)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {

                Assert.IsTrue(await session.SetDefaultPartLocationImportAsync(partNumber, location, locationgroup));
            }

        }
        [TestCase("100GCL")]
        [TestCase("TestProduct")]
        [TestCase("LED-SBddddd")]
        public async Task PartGetNumberAndTrackingFields(string searchTerm)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);


            using (FishbowlSession session = new FishbowlSession(config))
            {
                try
                {
                    List<PartNumAndTracks> list = await session.GetPartNumberAndTrackingFields(searchTerm);
                    Assert.IsNotNull(list);
                    Assert.IsTrue(list[0].PartNumber.ToUpper() == searchTerm.ToUpper());
                }
                catch (KeyNotFoundException k)
                {
                    //product doesnt exist
                }
            }


        }



        #endregion
        [Test]
        public async Task FBSessionGetLocationGroupListTest()
        {
            SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");


            using (FishbowlSession _session = new FishbowlSession(config))
            {

                var headerRow = await _session.GetLocationGroupList();

                Assert.NotNull(headerRow);
            }

        }


        [Test]
        public async Task FBDBgetInventoryTest()
        {
            MySqlConfig config = new MySqlConfig("192.168.150.2", "3301", "gone", "fishing", "BRITEIDEASUPDATE");

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                ProductAvailableInventory temp = await db.getProductAFS("LED-SBA24BL");
                Assert.IsInstanceOf(typeof(ProductAvailableInventory), temp);

            }


        }
        [Test]
        public async Task FBgetLocationTest()
        {
            SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");

            using (var session = new FishbowlSession(config))
            {
                Location1 loc = await session.GetLocationObject("29044", LocationLookupType.TagNumber);
                Assert.IsInstanceOf(typeof(Location1), loc);

                Location1 loc2 = await session.GetLocationObject("400", LocationLookupType.LocationID);
                Assert.IsInstanceOf(typeof(Location1), loc2);
            }


        }

        //[Test]
        //public async Task sessionMoveInventoryTest()
        //{
        //    SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");

        //    using (var session = new FishbowlSession(config))
        //    {
        //        Location1 loc = await session.GetLocationObject("29044", LocationLookupType.TagNumber);
        //        Assert.IsInstanceOf(typeof(Location1), loc);

        //        Location1 loc2 = await session.GetLocationObject("400", LocationLookupType.LocationID);
        //        Assert.IsInstanceOf(typeof(Location1), loc2);

        //        await session.MoveInventory("100GCL", "10", 716, 29044);
        //    }


        //}

        #region Inventory



        [TestCase(ValidPartNumberWithInventory, 10)]
        public async Task AddInventoryTest(string partNumber, int Qty)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            using (FishbowlSession session = new FishbowlSession(config))
            {
                Assert.True(await session.AddInventory(partNumber, Qty, "A1A", "Main Warehouse"));

            }
        }

        

        #endregion
        [Test]
        public async Task ExecuteQueryTest()
        {
            SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");
            List<TagTrackingObject> tagTrackingItems = null;

            //string headerRow;
            using (FishbowlSession _session = new FishbowlSession(config))
            {
                _session.DebugLevel = FishbowlConnectDebugLevel.Verbose;

                string query = String.Format(@"SELECT tag.id AS tagid, 
                        COALESCE (trackingdate.`info`, trackingdecimal.`info`, 
                            CASE WHEN parttracking.`typeId` = 80 THEN 
	                            CASE WHEN trackinginteger.`info` = 0 THEN 'false'
		                            ELSE 'true'
		                            END
	                            ELSE trackinginteger.`info`
	                            END, 

                            trackingtext.`info`) AS Info,
                            parttracking.`name` AS TrackingLabel
                        FROM tag 
                        LEFT JOIN part ON tag.`partId` = part.id

                        LEFT JOIN parttotracking ON parttotracking.`partId` = part.id 
	                        LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                        LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	
                        LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`
	
                        LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	
                        LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                        WHERE tag.id = {0}", 67622);



                tagTrackingItems = await _session.ExecuteQueryAsync<TagTrackingObject, TagTrackingItemClassMap>(query);

                await _session.LogoutJson();

                string message = _session.DebugMessage;
            }
            Assert.NotNull(tagTrackingItems);
            Assert.IsTrue(tagTrackingItems.Count > 0);


        }

        [Test]
        public async Task FBDBgetSpecsTest()
        {
            MySqlConfig config = new MySqlConfig("192.168.150.2", "3301", "gone", "fishing", "BRITEIDEASUPDATE");

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                List<ProductSpec> temp = await db.getProductSpecs("LED-SBA24BL");
                Assert.IsInstanceOf(typeof(List<ProductSpec>), temp);

            }


        }


        [Test]
        public async Task FBDBgetPartsTest()
        {
            MySqlConfig config = new MySqlConfig("192.168.150.2", "3301", "gone", "fishing", "BRITEIDEASUPDATE");

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                List<PartSimpleObject> temp = await db.GetFBParts();
                Assert.IsInstanceOf(typeof(List<PartSimpleObject>), temp);
                Assert.True(temp.Count > 1);

            }


        }




        [TestCase("100GGR")]
        [TestCase("dfdsf")]
        public async Task FBDBGetPartNumberFromProductOrUPCTest(string numberOrUPC)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(), DatabaseUser, DatabasePassword, DatabaseName);

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                string productNum = await db.GetPartNumberFromProductOrUPC(numberOrUPC);
                Assert.True(!string.IsNullOrEmpty(productNum));
            }


        }
        [Test]
        public async Task FBDBgetProductListTest()
        {
            using (FishbowlMySqlDB db = await FishbowlMySqlDB.CreateAsync(new MySqlConfig(DatabaseAddress,
                        DatabasePort.ToString(), DatabaseUser, DatabasePassword, DatabaseName)))
            {
                List<ProductSimpleObject> productSimples = await db.getFBProducts();
                Assert.IsInstanceOf(typeof(List<ProductSimpleObject>), productSimples);

            }


        }


        [TestCase("100GGR")]
        [TestCase("$L$N9B")]
        [TestCase("LED-G32OPWH")]
        [TestCase("$T$dfghjj")]
        public async Task FBDBGetInvQtyWithAllTracking(string searchTerm)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                DatabaseUser, DatabasePassword, DatabaseName);

            //for (int i = 0; i < 20; i++)
            //{
            //    Debug.WriteLine(i);
            //    if (i == 10)
            //    {
            //        Debug.WriteLine("Halfway");
            //    }
            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                try
                {
                    List<InvQtyWithAllTracking> temp = await db.GetPartTagAndAllTrackingWithDefaultLocation(searchTerm, "Main Warehouse", FishbowlConnect.Helpers.InventorySearchTermType.Product);
                    Assert.IsInstanceOf(typeof(List<InvQtyWithAllTracking>), temp);
                    foreach (InvQtyWithAllTracking item in temp)
                    {
                        Debug.Write(item.PartNumber.ToString());
                        Debug.Write(" - Location: ");
                        Debug.Write(item.LocationName);
                        Debug.Write(" - Default Location: ");
                        Debug.Write(item.DefaultLocationName);
                        Debug.Write(" - Location match: ");
                        Debug.WriteLine(item.DefaultLocationMatchStatus.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            }
            //    Thread.Sleep(500);
            //}



        }

        [TestCase("100GGR")]
        [TestCase("$L$N9B")]
        [TestCase("LED-G32OPWH")]
        [TestCase("$T$dfghjj")]
        public async Task FBDBOutputInvQtyWithPrimaryOrNoTracking(string searchTerm)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, DatabasePort.ToString(),
                DatabaseUser, DatabasePassword, DatabaseName);

            //for (int i = 0; i < 20; i++)
            //{
            //    Debug.WriteLine(i);
            //    if (i == 10)
            //    {
            //        Debug.WriteLine("Halfway");
            //    }
            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                try
                {
                    List<InvQtyWithAllTracking> temp = await db.GetPartTagAndAllTrackingWithDefaultLocation(searchTerm
                        , "Main Warehouse", FishbowlConnect.Helpers.InventorySearchTermType.Product);
                    Assert.IsInstanceOf(typeof(List<InvQtyWithAllTracking>), temp);

                    List<InvQtyWithAllTracking> primaryList = temp
                        .Where(i => i.TrackingInfo == null || (i.TrackingInfo != null && i.IsPrimaryTracking))
                        .ToList();



                    foreach (InvQtyWithAllTracking item in primaryList)
                    {
                        Debug.Write(item.PartNumber.ToString());
                        Debug.Write(" - Location: ");
                        Debug.Write(item.LocationName);
                        Debug.Write(" - TrackingInfo: ");
                        Debug.Write(item.TrackingInfo);
                        Debug.Write(" - IsPrimary: ");
                        Debug.WriteLine(item.IsPrimaryTracking);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            }
            //    Thread.Sleep(500);
            //}



        }




        [TestCase("100GGR")]
        [TestCase("$L$N9B")]
        [TestCase("LED-G32OPWH")]

        public async Task DBGetInvQtyWithTracking(string partNumber)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, "2361",
                DatabaseUser, DatabasePassword, DatabaseName);

            //for (int i = 0; i < 20; i++)
            //{
            //    Debug.WriteLine(i);
            //    if (i == 10)
            //    {
            //        Debug.WriteLine("Halfway");
            //    }
            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                try
                {
                    List<InvQtyWithTracking> temp = await db.GetPartTagAndPrimaryTrackingWithDefaultLocation(partNumber, "Main Warehouse");
                    Assert.IsInstanceOf(typeof(List<InvQtyWithTracking>), temp);
                    foreach (InvQtyWithTracking item in temp)
                    {
                        Debug.Write(item.PartNumber.ToString());
                        Debug.Write(" - Location: ");
                        Debug.Write(item.LocationName);
                        Debug.Write(" - Default Location: ");
                        Debug.Write(item.DefaultLocationName);
                        Debug.Write(" - Location match: ");
                        Debug.WriteLine(item.DefaultLocationMatchStatus.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            }
            //    Thread.Sleep(500);
            //}



        }

        [TestCase("100GCLsdfsdf")]
        [TestCase("60000000000")]
        public async Task CheckPartLocationTrackingIsValid(string partNumber)
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress
                , DatabasePort.ToString(), DatabaseUser, DatabasePassword, DatabaseName);

            //for (int i = 0; i < 20; i++)
            //{
            //    Debug.WriteLine(i);
            //    if (i == 10)
            //    {
            //        Debug.WriteLine("Halfway");
            //    }
            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                try
                {
                    bool valid = await db.CheckPartLocationTrackingIsValid(partNumber);
                    Assert.IsTrue(valid);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

            }
            //}

        }

        [TestCase("LED-IC44WkRS")]
        public async Task GetLastPartCostTest(string partNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            using (FishbowlSession session = new FishbowlSession(config))
            {
                string cost = await session.GetPartLastCost(partNumber);
                Assert.IsNotNull(cost);

                double currency;
                Assert.True(Double.TryParse(cost, out currency));

            }
        }

        [Test]
        public async Task FBExecuteQueryWithoutMap()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession _session = new FishbowlSession(config))
            {
                _session.DebugLevel = FishbowlConnectDebugLevel.Verbose;

                var results = await _session.ExecuteQueryAsync<string>("Select Location.name from location where UPPER(Name) " +
                        "like 'Store Front'");

                Assert.NotNull(results);
            }


        }
        [Test]
        public async Task FBExecuteQueryWithNoResults()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession _session = new FishbowlSession(config))
            {
                _session.DebugLevel = FishbowlConnectDebugLevel.Verbose;

                var results = await _session.ExecuteQueryAsync<string>("Select location.name " +
                    "from location where location.id = 9854");

                Assert.NotNull(results);
            }


        }

        [Test]
        public async Task FBDBInsertMobileReceipts()
        {
            MySqlConfig config = new MySqlConfig("192.168.150.2", "2361", "gone", "fishing", "BRITEIDEASUPDATE");

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                MobileReceipt receipt = new MobileReceipt
                {
                    mrId = 2,
                    description = "container 1",
                    timeStarted = DateTime.Parse("2018-03-30 07:00:00"),
                    timeFinished = DateTime.Parse("2018-03-30 10:00:00"),
                    statusId = 10
                };

                List<IMobileReceiptItem> items = new List<IMobileReceiptItem>();
                items.Add(new MobileReceiptItem
                {
                    mrId = 2,
                    timeScanned = DateTime.Parse("2018-03-30 08:00:00"),
                    upc = "042002480756",
                    statusID = 10
                });
                items.Add(new MobileReceiptItem
                {
                    mrId = 2,
                    timeScanned = DateTime.Parse("2018-03-30 09:00:00"),
                    upc = "042002480695",
                    statusID = 10
                });

                await db.InsertMobileReceipt(receipt, items);


            }


        }



        [TestCase(GoodUserName, GoodPassword, GoodServerAddress)]
        [TestCase(BadUserName, BadPassword, GoodServerAddress)]
        [TestCase(GoodUserName, GoodPassword, BadServerAddress)]
        [TestCase(NoUserName, GoodPassword, GoodServerAddress)]
        [TestCase(GoodUserName, NoPassword, GoodServerAddress)]
        public async Task LoginXMLTest(string UserName, string Password, string ServerAddress)
        {
            SessionConfig config = new SessionConfig(ServerAddress, 28192, UserName, Password);

            using (FishbowlSession _session = new FishbowlSession(config))
            {
                try
                {
                    await _session.Login(UserName, Password, FishbowlSession.LoginMethod.Xml);

                    Assert.IsTrue(_session.IsAuthenticated);
                }
                catch (FishbowlAuthException)
                {
                    Assert.IsFalse(_session.IsAuthenticated);
                }
                catch (FishbowlConnectionException)
                {

                }
            }

        }

        //[Test]
        //public void PartRequestTest()
        //{
        //    SessionConfig config = new SessionConfig("192.168.150.2", 28192, "admin", "does1tall");

        //    using (FishbowlSession _session = new FishbowlSession(config))
        //    {

        //        FishbowlConnect.Json.APIObjects.Part tempPart = _session.GetPartObject("100BCL").Result;
        //        Assert.IsInstanceOf(typeof(FishbowlConnect.Json.APIObjects.Part), tempPart);
        //    }

        //    //Thread.Sleep(100);
        //    using (FishbowlSession _session = new FishbowlSession(config))
        //    {
        //        Debug.WriteLine("Second Request");
        //        Part tempPart2 =  _session.GetPartObject("100GCL").Result;
        //        Assert.IsInstanceOf(typeof(Part), tempPart2);
        //    }
        //}

            [Test]
            public async Task CustomerSerializeTest()
        {
            Customer SingleCustomer = new Customer();

            //SingleCustomer.magentoCustID = row.Field<UInt32>("customer_id");

            //look for default billing account company name first
            //MagentoDefaultAddrInfo DefaultCustomerAddrInfo = await LookupMagentoCompanyName((int)row.Field<UInt32>("customer_id"), DefaultCustomerPassValue.CustomerID);
            //MagentoDefaultAddrInfo DefaultOrderAddrInfo = await LookupMagentoCompanyName((int)row.Field<UInt32>("entity_id"), DefaultCustomerPassValue.OrderEntityID);

            //check for default address under the customer account
            //if (DefaultCustomerAddrInfo != null)
            //{
            //    //there is a defualt address set otherwise return null
            //    if (DefaultCustomerAddrInfo.DefaultAddrID > 0)
            //    {
            //        DefBillAddrID = DefaultCustomerAddrInfo.DefaultAddrID;
            //        DefaultAddrSet = true;
            //    }

            //    if (DefaultCustomerAddrInfo.DefaultCompanyName != null)
            //    {
            //        //default company set on default address
            //        CustomerName = DefaultCustomerAddrInfo.DefaultCompanyName;
            //    }


            //}

            //if company name is still blank and the sales order billing address contains a company name then use that
            ////set company name if applicaple, otherwise last name, first
            //if (DefaultOrderAddrInfo.DefaultCompanyName != null && CustomerName == "")
            //{
            //    //no default billing address company, but there was on on this order
            //    CustomerName = DefaultOrderAddrInfo.DefaultCompanyName;
            //}

            //if (CustomerName == "")//still blank so no default anything
            //{
                //SingleCustomer.Name = "Beer, Holly";
            //}

            //set initial data
            SingleCustomer.Name = "Beer, Holly";
            SingleCustomer.JobDepth = "1";
            SingleCustomer.ActiveFlag = "true";
            SingleCustomer.DefaultSalesman = "admin";
            SingleCustomer.DefPaymentTerms = "Credit Card";
            SingleCustomer.DefaultCarrier = "FedEx-Parcel-Ground";

            //Addresses
            Address BillingAddress = new Address();
            Address ShippingAddress = new Address();

            //DataRow MagBillingAddress;// = new DataRow();
            //DataRow MagShippingAddress;// = new DataRow();

            //if (DefaultAddrSet)
            //{
            //    //get magento Default Billing Address from server
            //    MagBillingAddress = await GetCustomerAddressInfo(DefaultCustomerAddrInfo.DefaultAddrID);
            //    //check for default shipping address
            //    int SAID;

            //    if ((SAID = await GetCustomerDefShippingAddressID((int)row.Field<UInt32>("customer_id"))) != 0)
            //    {
            //        //default set
            //        MagShippingAddress = await GetCustomerAddressInfo(SAID);
            //    }
            //    else
            //    {
            //        MagShippingAddress = MagBillingAddress;
            //    }
            //}
            //else
            //{
            //    //no default address, just use what is on order
            //    MagBillingAddress = await GetOrderAddressInfo((int)row.Field<UInt32>("entity_id"), OrderAddressType.billing);
            //    MagShippingAddress = await GetOrderAddressInfo((int)row.Field<UInt32>("entity_id"), OrderAddressType.shipping);
            //}

            //set billing address info

            BillingAddress.Name = "Bill-To";
            BillingAddress.Attn = "Beer, Holly";
            BillingAddress.Street = "1605 Hunt Dr";
            BillingAddress.City = "Normal";
            BillingAddress.Default = "true";
            BillingAddress.Residential = "false";
            BillingAddress.Type = AddressType.Main;
            BillingAddress.Zip = "61716";

            //using (SqlConnection BIconnection = BI_Integrator.BIConnection())
            //{
            //    BIconnection.Open();
            //    using (SqlCommand command = new SqlCommand(@"SELECT abbr FROM StateAbbr WHERE State = '" +
            //                                MagBillingAddress.Field<string>("region") + "'", BIconnection))
            //    {
                    State AddrState = new State();
                    AddrState.Code = "IL";
                    BillingAddress.State = AddrState;
            //    }
            //}

            Country AddrCountry = new Country();
            AddrCountry.Name = "United States";
            AddrCountry.Code = "US";
            BillingAddress.Country = AddrCountry;

            //List<AddressInformation> ContactInfoList = new List<AddressInformation>();

            //if (MagBillingAddress.Field<string>("telephone") != null)
            //{
            //    AddressInformation AddrContactInfo = new AddressInformation();
            //    AddrContactInfo.Name = CustomerName;
            //    AddrContactInfo.Data = MagBillingAddress.Field<string>("telephone");
            //    AddrContactInfo.Default = "true";
            //    AddrContactInfo.Type = ContactType.Main;
            //    AddrContactInfo.TypeSpecified = true;

            //    ContactInfoList.Add(AddrContactInfo);
            //}

            //if (MagBillingAddress.Field<string>("email") != null)
            //{
            //    AddressInformation AddrContactInfo = new AddressInformation();
            //    AddrContactInfo.Name = CustomerName;
            //    AddrContactInfo.Data = MagBillingAddress.Field<string>("email");
            //    AddrContactInfo.Default = "true";
            //    AddrContactInfo.Type = ContactType.Email;
            //    AddrContactInfo.TypeSpecified = true;

            //    ContactInfoList.Add(AddrContactInfo);
            //}

            //if (ContactInfoList.Count > 0)
            //{
            //    AddressInformationList InfoList = new AddressInformationList();
            //    InfoList.AddressInformation = ContactInfoList;
            //    BillingAddress.AddressInformationList = InfoList;
            //}

            //shipping address information
            ShippingAddress.Name = "Ship-To";
            ShippingAddress.Attn = "Beer, Holly";
            ShippingAddress.Street = "1605 Hunt Dr";
            ShippingAddress.City = "Normal";
            ShippingAddress.Default = "true";
            ShippingAddress.Residential = "false";
            ShippingAddress.Type = AddressType.Ship;
            ShippingAddress.Zip = "61716";

            //using (SqlConnection BIconnection = BI_Integrator.BIConnection())
            //{
            //    BIconnection.Open();
            //    using (SqlCommand command = new SqlCommand(@"SELECT abbr FROM StateAbbr WHERE State = '" +
            //                                MagShippingAddress.Field<string>("region") + "'", BIconnection))
            //    {
                    State ShipAddrState = new State();
                    ShipAddrState.Code = "IL";
                    ShippingAddress.State = ShipAddrState;
            //    }
            //}

            AddrCountry = new Country();
            AddrCountry.Name = "United States";
            AddrCountry.Code = "US";
            ShippingAddress.Country = AddrCountry;


            List<Address> AddressList = new List<Address>();
            AddressList.Add(BillingAddress);
            AddressList.Add(ShippingAddress);

            SingleCustomer.Addresses = new Addresses();
            SingleCustomer.Addresses.Address = new System.Collections.ObjectModel.ObservableCollection<Address>(AddressList);


            SessionConfig config = new SessionConfig("192.168.150.2", 28192, "admin", "does1tall", 10000);
            using (FishbowlSession session = new FishbowlSession(config))
            {
                string Json = await session.CustomerPut(SingleCustomer);
                Debug.WriteLine(Json);
                    }
        }

        [Test]
        public async Task GetCustomerTest()
        {
            SessionConfig config = new SessionConfig("192.168.150.2", 28192, "admin", "does1tall", 10000);
            using (FishbowlSession session = new FishbowlSession(config))
            {
                Customer test = await session.GetCustomerObject("Shadow Lake Dental");
                //Debug.WriteLine(Json);
            }
        }


        [Test]
        public async Task SOPutTest()
        {
            SessionConfig config = new SessionConfig("192.168.150.2", 28192, "admin", "does1tall", 10000);
            //try
            //{

           // Product tempProduct;

            using (FishbowlSession session = new FishbowlSession(config))
            {
                SalesOrder SingleOrder = new SalesOrder();

                SingleOrder.BillTo = new BillType
                {
                    Name = "McKay Landscape Lighting",
                    AddressField = "11440 S 146th Street Suite 104",
                    City = "Omaha",
                    Zip = "68138",
                    Country = "US",
                    State = "NE"
                };
                SingleOrder.Salesman = "admin";
                SingleOrder.Status = "20";
                SingleOrder.Carrier = "Pickup - Main";
                SingleOrder.PaymentTerms = "Net 30";
                SingleOrder.CustomerContact = "McKay Landscape Lighting";
                SingleOrder.CustomerName = "McKay Landscape Lighting";

                SingleOrder.Ship = new ShipType
                {
                    Name = "McKay Landscape Lighting",
                    AddressField = "11440 S 146th Street Suite 104",
                    City = "Omaha",
                    Zip = "68138",
                    Country = "US",
                    State = "NE"
                };

                SingleOrder.IssueFlag = "false";
                SingleOrder.VendorPO = "1000003672";
                SingleOrder.CustomerPO = "MOY";
                
                SalesOrderItem item = new SalesOrderItem
                {
                    ProductNumber= "LED-HWLF",
					Taxable= "false",
					Quantity= "1.0000",
					ProductPrice= "133.5000",
					UOMCode= "ea",
					ItemType= "10",
					NewItemFlag= "false"
                };

                SingleOrder.Items = new Items();
                SingleOrder.Items.SalesOrderItem = new List<SalesOrderItem>();
                SingleOrder.Items.SalesOrderItem.Add(item);

                SingleOrder.EDITxnID = 0;
                SingleOrder.MagIncrementID = "1000003672";
                SingleOrder.MagEntityID = 4133;



                string newNumber = await session.SalesOrderPut(SingleOrder);

            }
        

        }

        [Test]
        public async Task ProductSaveTest()
        {
            SessionConfig config = new SessionConfig("192.168.150.4", 28192, "admin", "does1tall");
            //try
            //{

            Product tempProduct;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                tempProduct = await session.GetProduct("100BCL");

                //tempProduct.Description = "100BCLlights";
                //tempProduct.Details = "";
                tempProduct.UPC = "545151515412514"; //042001340020
                await session.SaveProductNoCustomFields(tempProduct);
            }

            //    using (FishbowlSession _session2 = new FishbowlSession(config))
            //    {
            //        Debug.WriteLine("Second Request");
            //        await _session2.SaveProductNoCustomFields(tempProduct);

            //    //tempProduct = _session.GetProduct("100GCL").Result;
            //    //Assert.IsInstanceOf(typeof(Product), tempProduct);

            //    //tempProduct.Description = "100BCLlights";
            //    //tempProduct.Details = "";
            //    //tempProduct.UPC = "545151515412514"; //042001340020
            //}

            //}
            //catch(Exception e)
            //{
            //    Debug.WriteLine(e);
            //}

        }

        [Test]
        public async Task ProductSavePriceUPCTest()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            Product tempProduct;


            using (FishbowlSession _session = new FishbowlSession(config))
            {

                tempProduct = await _session.GetProduct("100GCL");

                //tempProduct.Description = "100BCLlights";
                //tempProduct.Details = "";
                tempProduct.Price = "10.79"; //10.79
                tempProduct.UPC = "042001340020"; //042001340020
                await _session.SaveProductPriceAndUpc(tempProduct);
            }


        }

        [Test]
        public async Task GetPartInventory()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            using (FishbowlSession _session = new FishbowlSession(config))
            {

                var tempProduct = await _session.GetPartInventory("100GCL");
                Assert.IsInstanceOf<List<InvQty>>(tempProduct);
            }
        }


        [Test]
        public async Task GetProductTest()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            Product tempProduct;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                tempProduct = await session.GetProduct("100GCL");
                Assert.IsInstanceOf<Product>(tempProduct);
            }



        }

        [Test]
        public async Task UpdateImagesInFB()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("//bi-dc1-12r2/macshare/Photos/Database Source/minimized");
            FileInfo[] files = directoryInfo.GetFiles("*.jpg");

            //using (FishbowlMySqlDB dbSession = await FishbowlMySqlDB.CreateAsync(new MySqlConfig("192.168.150.2", "3301", "gone", "fishing", "BRITEIDEASUPDATE")))
            //{
            SessionConfig config = new SessionConfig("192.168.150.2",
                            28192, "admin", "does1tall", 10000);

            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains("_"))
                {
                    Console.WriteLine("Found: " + file.Name);

                    using (FishbowlSession session = new FishbowlSession(config))
                    {
                        try
                        {
                            using (FishbowlMySqlDB dbSesison = await FishbowlMySqlDB.CreateAsync(new MySqlConfig("192.168.150.2", "3301",
                                "gone", "fishing", "BRITEIDEASUPDATE")))
                            {
                                if (dbSesison.getProduct(file.Name.Substring(0, file.Name.Length - 4)) != null)
                                {
                                    //product found
                                    await session.SaveImageToFishbowl(FBHelperClasses.SaveImageType.Part, file.Name.Substring(0, file.Name.Length - 4),
                                        Convert.ToBase64String(File.ReadAllBytes(file.FullName)));
                                    Console.WriteLine("Saved image: " + file.Name);
                                }
                            }


                            //if (await session.GetProduct(file.Name.Substring(0, file.Name.Length - 4)) != null)
                            //{
                            //    //product found
                            //    await session.SaveImageToFishbowl(FBHelperClasses.SaveImageType.Part, file.Name.Substring(0, file.Name.Length - 4),
                            //        Convert.ToBase64String(File.ReadAllBytes(file.FullName)));
                            //    Console.WriteLine("Saved image: " + file.Name);
                            //}
                        }
                        catch (KeyNotFoundException e)
                        {
                            //product not found
                            //return;
                            Debug.WriteLine(e.Message);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                    }
                    Thread.Sleep(500);
                }
                //}
            }
        
}





        [TestCase("A1A", "Main Warehouse")]
        public async Task GetLocationSimpleTest(string LocationName, string LocationGroupName)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
            LocationSimpleObject locationSimple;

            using (FishbowlSession session = new FishbowlSession(config))
            {

                locationSimple = await session.GetLocationSimple(LocationGroupName,
                    LocationName);

                Assert.IsInstanceOf<LocationSimpleObject>(locationSimple);
            }


        }

        [Test]
        public async Task GetPickListTest()
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword, 15000);

            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {

                var waitAndCancelThread = Task.Run(() =>
                {
                    Thread.Sleep(15000); //wait 15 seconds and then cancel if necessary

                    cancellationTokenSource.Cancel();
                });


                try
                {
                    using (FishbowlSession session = new FishbowlSession(config))
                    {

                        List<PickSimpleObject> pickSimpleObjects = await session.GetPickSimpleList(new PickListFilters
                        {
                            CompletelyFulfillable = true,
                            Status = PickStatus.All,
                            LocationGroupName = "Main Warehouse",
                            Username="DESK3"
                        }, "69815", null, cancellationTokenSource.Token);


                        Assert.IsInstanceOf<List<PickSimpleObject>>(pickSimpleObjects);
                    }
                }
                catch(TaskCanceledException e)
                {
                    Debug.WriteLine(e.Message);
                }


                await waitAndCancelThread;
            }
        }

        [TestCase("S47220")]
        [TestCase("S47221")]
        [TestCase("S46068")]
        [TestCase("S45901")]
        public async Task GetPickTest(string PickNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNumber);

                Assert.IsInstanceOf<Pick>(pick);
                Assert.IsNotNull(pick.PickItems);
                Assert.IsTrue(pick?.PickItems?.PickItem.Count > 0);
            }
        }


        [TestCase("S47221")]
        public async Task SavePickTest(string PickNumber)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword, 10000);

            using (FishbowlSession session = new FishbowlSession(config))
            {

                Pick pick = await session.GetPick(PickNumber);

                Assert.IsInstanceOf<Pick>(pick);
                Assert.IsNotNull(pick.PickItems);
                Assert.IsTrue(pick?.PickItems?.PickItem.Count > 0);

                //adjust the pick
                //pick.PickItems.PickItem[1].Status = "40";
                PickItem workingOnItem = pick.PickItems.PickItem.First(m => m.Part.Num == "100GCL" && m.Quantity > 1);
                    workingOnItem.Status = "40";
                //good enough to use everything provided



                Tag tag1 = await session.GetTagObjectAsync(TagID: "67397"); //9 qty
                Tag tag2 = await session.GetTagObjectAsync(TagID: "67405"); //4 qty


                PickItem pickItem1 = MyExtensions.DeepCopyXML(workingOnItem);
                pickItem1.PickItemID = "-1";

                //pickItem1.PropertyChanged += PickItem1_PropertyChanged;

                PickItem pickItem2 = MyExtensions.DeepCopyXML(workingOnItem);
                pickItem2.PickItemID = "-1";


                pickItem1.Location = tag1.Location;
                pickItem1.Tracking = tag1.Tracking;
                pickItem1.Quantity = 6;
                pickItem1.Status = "40";

                pickItem2.Location = tag2.Location;
                pickItem2.Tracking = tag2.Tracking;
                pickItem2.Quantity = 4;
                pickItem2.Status = "40";

                pick.PickItems.PickItem.Remove(workingOnItem);

                pick.PickItems.PickItem.Add(pickItem1);
                pick.PickItems.PickItem.Add(pickItem2);



                Pick returnPick = await session.SavePick(pick);
                Assert.IsInstanceOf(typeof(Pick), returnPick);
            }

        }

        private void PickItem1_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Property Changed " + e.PropertyName);
        }

        //[Test]
        //public async Task GetProductListTest()
        //{
        //    SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);
        //    List<ProductListInfo> tempList;

        //    for (int i = 0; i < 10; i++)
        //    {
        //        Debug.WriteLine(i);

        //        using (FishbowlSession session = new FishbowlSession(config))
        //        {

        //            tempList = await session.GetProductList("GCL");
        //            Assert.IsInstanceOf<List<ProductListInfo>>(tempList);
        //        }
        //        //Thread.Sleep(500);
        //    }


        //}


        //SHIPPING
        [TestCase("S59374")] //valid
        [TestCase("S59383")] //Invalid
        public async Task getShipmentObjectTest(string shipNum)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword, 10000);

            try
            {
                using (FishbowlSession session = new FishbowlSession(config))
                {
                    Shipping shipment = await session.getShipment(shipNum);

                    Assert.IsInstanceOf<Shipping>(shipment);
                    Assert.IsNotNull(shipment.Cartons);
                    Assert.IsTrue(shipment.Cartons?.Carton?.Count > 0);

                }
            }
            catch(FishbowlException fe)
            {
                Debug.WriteLine("Fishbowl Error: " + fe.Message);
            }

        }

        [Test]
        public async Task getShipmentList()
        {
            ShipListFilters shipListFilters = new ShipListFilters();
            shipListFilters.Status = ShipStatus.Entered;

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword, 10000);


            //try
            //{
                using (FishbowlSession session = new FishbowlSession(config))
                {
                    session.DebugLevel = FishbowlConnectDebugLevel.Information;
                    List<ShipSimpleObject> shipSimpleObjects = await session.getShipSimpleList(shipListFilters, "bluegrass");
                    Assert.IsInstanceOf<List<ShipSimpleObject>>(shipSimpleObjects);
                }
            //}
            //catch (FishbowlException fe)
            //{
            //    Debug.WriteLine("Fishbowl Error: " + fe.Message);
            //}


        }


        [Test]
        public async Task DBShipmentImageInsertTest()
        {
            MySqlConfig config = new MySqlConfig(DatabaseAddress, "2361",
                DatabaseUser, DatabasePassword, DatabaseName);

            using (var db = await FishbowlMySqlDB.CreateAsync(config))
            {
                //try
                //{
                    //int record = await db.InsertShipmentImage(49888, "//192.168.150.4/share/shipping/shipmentimages/S59919-01_04.jpg");
                    //Debug.WriteLine(record);
                //}
                //catch (Exception e)
                //{
                //    Debug.WriteLine(e.Message);
                //}

            }
        }


        [Test]
        public async Task TimedTestforQueryExecute()
        {
            ShipListFilters shipListFilters = new ShipListFilters();
            shipListFilters.Status = ShipStatus.AllOpen;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword, 10000);
            for (int i = 0; i < 100; i++)
            {
                
                try
                {
                    using (FishbowlSession session = new FishbowlSession(config))
                    {
                        session.DebugLevel = FishbowlConnectDebugLevel.Information;
                        List<ShipSimpleObject> shipSimpleObjects = await session.getShipSimpleList(shipListFilters);
                        Debug.WriteLine(i);
                    }
                }
                catch (FishbowlException fe)
                {
                    Debug.WriteLine("Fishbowl Error: " + fe.Message);
                }

            }

            stopwatch.Stop();
            Debug.WriteLine("Time elapsed for 100 iterations using FBsession: " + stopwatch.Elapsed.TotalSeconds);

            stopwatch.Reset();
            Thread.Sleep(5000);

            stopwatch.Start();
            MySqlConfig DBconfig = new MySqlConfig(DatabaseAddress, "2361",
                DatabaseUser, DatabasePassword, DatabaseName);
            for (int i = 0; i < 100; i++)
            {

                using (var db = await FishbowlMySqlDB.CreateAsync(DBconfig))
                {
                    try
                    {
                        List<ShipSimpleObject> temp = await db.getShipSimpleList(shipListFilters);
                        Debug.WriteLine(i);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Fishbowl Error: " + e.Message);
                    }

                }
            }

            stopwatch.Stop();
            Debug.WriteLine("Time elapsed for 100 iterations using FBsession: " + stopwatch.Elapsed.TotalSeconds);
        }





        //[Test]
        //public async Task ResilianceTest()
        //{
        //    SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");

        //    using (FishbowlSession _session = new FishbowlSession(config))
        //    {
        //        Customer tempCustomer;
        //        Part tempPart;

        //        try
        //        {
        //            //await _session.Login("bnordstrom", "does1tall");
        //            tempPart = await _session.GetPartObject("100GCL");

        //            Debug.WriteLine("Drop network now");
        //            Thread.Sleep(7000);
        //            tempCustomer = await _session.GetCustomerObject("Brian Nordstrom-Web"); //should throw issue timeout error
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(ex.Message);

        //        }

        //        Debug.WriteLine("Trying to re-connect when off");
        //        await _session.Reset();

        //        Thread.Sleep(10000);

        //        Debug.WriteLine("Turn on network");
        //        Thread.Sleep(7000);

        //        await _session.Reset();

        //        //simulate the isConnected property event before login in
        //        Thread.Sleep(1500);
        //        //Debug.WriteLine("logging in after reset");
        //        //await _session.Login("bnordstrom", "does1tall");
        //        Assert.IsTrue(_session.IsConnected);
        //    }
        //}



        //[Test]
        //public async Task TestReceiveTimeout()
        //{
        //    SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");

        //    using (FishbowlSession _session = new FishbowlSession(config))
        //    {
        //        try
        //        {
        //            //await _session.Login("bnordstrom", "does1tall");


        //            Part tempPart = await _session.GetPartObject("100GCL");


        //            Assert.IsInstanceOf(typeof(Part), tempPart);

        //            Debug.WriteLine("Disconnect network now");
        //            Thread.Sleep(4000);


        //            tempPart = await _session.GetPartObject("100BCL");


        //            Assert.IsInstanceOf(typeof(Part), tempPart);
        //        }

        //        catch (FishbowlException ex)
        //        {

        //            Assert.IsInstanceOf(typeof(FishbowlException), ex);
        //            Assert.IsTrue(ex.StatusCode == "9999"); //issue receive timeout

        //            Debug.WriteLine("Open network");
        //            Thread.Sleep(2000);

        //            Part tempPart = await _session.GetPartObject("100BCL");

        //            Assert.IsFalse(_session.IsConnected);
        //            Assert.IsFalse(_session.IsAuthenticated);
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(ex.Message);
        //        }
        //    }

        //}

        //[Test]
        //public async Task TestNetworkDrop()
        //{
        //    SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");

        //    using (FishbowlSession _session = new FishbowlSession(config))
        //    {
        //        try
        //        {
        //            await _session.Login("bnordstrom", "does1tall", FishbowlSession.LoginMethod.Xml);

        //            Debug.WriteLine("Drop network now");
        //            Thread.Sleep(10000);

        //            Part tempPart = await _session.GetPartObject("100GCL");
        //            Assert.IsInstanceOf(typeof(Part), tempPart);
        //        }

        //        catch (FishbowlException ex)
        //        {

        //            Assert.IsInstanceOf(typeof(FishbowlException), ex);
        //            //Assert.IsTrue(ex.StatusCode == "9999"); //issue receive timeout

        //            Debug.WriteLine("Open network");
        //            Thread.Sleep(10000);

        //            Part tempPart = await _session.GetPartObject("100GCL");

        //            Assert.IsFalse(_session.IsConnected);
        //            Assert.IsFalse(_session.IsAuthenticated);
        //        }
        //        catch (Exception)
        //        {
        //            Debug.WriteLine("Open network");
        //            Thread.Sleep(10000);

        //            Part tempPart = await _session.GetPartObject("100GCL");

        //            Assert.IsFalse(_session.IsConnected);
        //            Assert.IsFalse(_session.IsAuthenticated);
        //        }
        //    }

        //}


        //[Test]
        //public async Task TestDeepCopy()
        //{
        //    try
        //    {


        //        FishbowlSession _session = new FishbowlSession(new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword));

        //        await _session.Login("bnordstrom", "does1tall", FishbowlSession.LoginMethod.Xml);
        //        Pick tempPick = await _session.GetPickDetails("S47191");

        //        PickItem tempItem = MyExtensions.DeepCopy<PickItem>(tempPick.PickItems[0]);
        //        Assert.IsInstanceOf<PickItem>(tempItem);


        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.IsInstanceOf(typeof(FishbowlException), ex);
        //        //throw;
        //    }
        //}

        [Test]
        public void TestGetProduct()
        {
            try
            {
                SessionConfig config = new SessionConfig("192.168.150.4", 28192, "bnordstrom", "does1tall");

                using (FishbowlSession session = new FishbowlSession(config))
                {

                    Product temp = session.GetProduct("100GCL").Result;

                    Assert.IsInstanceOf(typeof(Product), temp);

                }
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf(typeof(FishbowlException), ex);
                //throw;
            }
        }

        [TestCase("00875-SC")]
        public async Task GetPart(string productnumber )
        {
            try
            {
                SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);

                using (FishbowlSession session = new FishbowlSession(config))
                {

                    PartSimpleObject part = await session.GetSimplePart(productnumber);

                    Assert.IsInstanceOf(typeof(PartSimpleObject), part);

                }
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf(typeof(FishbowlException), ex);
                //throw;
            }
        }
    }
}
