using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using FishbowlConnect.Helpers;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using MySql.Data.MySqlClient;
using MySqlConnector;


//using FishbowlConnect.MySQL;

namespace FishbowlConnect.MySQL
{
    public class FishbowlMySqlDB : IDisposable
    {
        public MySqlConnection Connection { get; private set; }

        public MySqlConfig Config { get; set; }

        bool disposed = false;


        

        public async static Task<FishbowlMySqlDB> CreateAsync(MySqlConfig mySqlConfig)
        {
            
            var ret = new FishbowlMySqlDB(mySqlConfig);
            await ret.Connection.OpenAsync();
            return ret;
            //return ret.InitializeAsync();
        }

        private async Task<FishbowlMySqlDB> InitializeAsync()
        {
            Connection = new MySqlConnection(
                                @"SERVER=" + Config.MySqlServerHost +
                                ";PORT=" + Config.MySqlPort +
                                ";DATABASE=" + Config.MySqlDatabase +
                                ";UID=" + Config.MySqlUser +
                                ";PASSWORD=" + Config.MySqlPassword);

            await Connection.OpenAsync();
            return this;
        }

        private FishbowlMySqlDB(MySqlConfig mySqlConfig)
        {

            if (mySqlConfig != null)
            {
                if (String.IsNullOrEmpty(mySqlConfig.MySqlServerHost))
                {
                    throw new ArgumentNullException("MySQL Server required");
                }
                if (String.IsNullOrEmpty(mySqlConfig.MySqlPort))
                {
                    throw new ArgumentNullException("MySQL Port required");
                }
                if (String.IsNullOrEmpty(mySqlConfig.MySqlUser))
                {
                    throw new ArgumentNullException("MySQL User required");
                }
                if (String.IsNullOrEmpty(mySqlConfig.MySqlPassword))
                {
                    throw new ArgumentNullException("MySQL Password required");
                }
                if (String.IsNullOrEmpty(mySqlConfig.MySqlDatabase))
                {
                    throw new ArgumentNullException("MySQL Database name required");
                }

                Config = mySqlConfig;
                Connection = new MySqlConnection(
                    @"SERVER=" + Config.MySqlServerHost +
                    ";PORT=" + Config.MySqlPort +
                    ";DATABASE=" + Config.MySqlDatabase +
                    ";UID=" + Config.MySqlUser +
                    ";PASSWORD=" + Config.MySqlPassword);
            }
            else
            {

                throw new ArgumentNullException("Need config to connect");
            }
        }




        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Connection?.Close();
                Connection?.Dispose();
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }


        #region PricingInventory

        public async Task<ProductAvailableInventory> getProductAFS(string ProductNumber)
        {
            if (String.IsNullOrEmpty(ProductNumber))
            {
                throw new ArgumentNullException("Product Number is required");
            }

            //await Load();
            ProductAvailableInventory productAvailableInventory = null;// = new ProductAvailableInventory();

            string query = String.Format(
                        @"SELECT PRODUCT.ID AS PRODUCTID, PRODUCT.NUM AS PRODUCTNUM, PRODUCT.PARTID, 
                        (COALESCE(SUM((QTYONHAND-QTYALLOCATED-QTYNOTAVAILABLE)/COALESCE(FACTOR,1)),0) + COALESCE(qryInventoryOnTransfers.SumofQty,0)) AS InStockAvailable,
                        COALESCE(BOMInventory.MaxBuildInv,0) AS BuildInventory, 
                        ((COALESCE(SUM((QTYONHAND-QTYALLOCATED-QTYNOTAVAILABLE)/COALESCE(FACTOR,1)),0) + COALESCE(qryInventoryOnTransfers.SumofQty,0)) + 
                        COALESCE(BOMInventory.MaxBuildInv,0)) AS AVAILABLEFORSALE
        
                            FROM PRODUCT
	                    LEFT JOIN QTYINVENTORYTOTALS ON (PRODUCT.PARTID = QTYINVENTORYTOTALS.PARTID)
	                    LEFT JOIN (SELECT PART.ID AS PARTID, UOMCONVERSION.FROMUOMID, PRODUCT.ID AS PRODUCTID,
		                        UOMCONVERSION.TOUOMID, UOMCONVERSION.MULTIPLY, UOMCONVERSION.FACTOR
		                        FROM (UOMCONVERSION INNER JOIN PART ON UOMCONVERSION.FROMUOMID = PART.UOMID)
		                        INNER JOIN PRODUCT ON (UOMCONVERSION.TOUOMID = PRODUCT.UOMID)
		                        AND (PART.ID = PRODUCT.PARTID)) PARTTOPRODUOMCONV ON (PRODUCT.ID = PARTTOPRODUOMCONV.ProductID)
	                    LEFT JOIN (SELECT QTYALLOCATEDTOSEND.PARTID, SUM(QTYALLOCATEDTOSEND.QTY) AS SumOfQTY
	                        FROM QTYALLOCATEDTOSEND
	                        GROUP BY QTYALLOCATEDTOSEND.PARTID) qryInventoryOnTransfers ON (Product.PartID = qryInventoryOnTransfers.PartID)

                           
        
                            LEFT JOIN (SELECT part.num AS PRODUCTNUM, MAX(CASE WHEN BOMInventory.maxbuildcount < 0 
				                    THEN 0 
				                    ELSE BOMInventory.maxbuildcount END) AS MaxBuildInv
			                    FROM bom
			                    JOIN bomitem ON bomitem.`bomId` = bom.`id`
			                    JOIN part ON part.id = bomitem.`partId`
			                    JOIN product ON (product.`partId` = part.id AND product.`uomId` = 1)
			                    JOIN CUSTOMINTEGER ON (CUSTOMINTEGER.RECORDID = PRODUCT.ID AND CUSTOMINTEGER.INFO=1 AND CUSTOMINTEGER.CUSTOMFIELDID=31) 
			                    JOIN CUSTOMINTEGER BOMUsedOnline ON (BOMUsedOnline.`recordId` = BOM.`id` AND BOMUsedOnline.`customFieldId` = 11 AND BOMUsedOnline.`info` = 1)
			                    JOIN (SELECT bomid, MIN(FLOOR((CASE WHEN availableinventory.availableforsale < 0 
					                    THEN 0 
					                    ELSE availableinventory.availableforsale
					                    END)/BOMitem.`quantity`)) AS MaxBuildCount
			                    FROM bomitem
			                    JOIN (SELECT SUM(COALESCE(QTYONHAND,0)-COALESCE(QTYALLOCATED,0)-COALESCE(QTYNOTAVAILABLE,0) + 
				                    COALESCE(qryInventoryOnTransfers.SumofQty,0)) AS AVAILABLEFORSALE, part.id AS partid
				
				                    FROM part
				                    LEFT JOIN QTYINVENTORYTOTALS ON part.id = qtyinventorytotals.partid
				                    LEFT JOIN (SELECT QTYALLOCATEDTOSEND.PARTID, SUM(QTYALLOCATEDTOSEND.QTY) AS SumOfQTY
					                    FROM QTYALLOCATEDTOSEND
					                    GROUP BY QTYALLOCATEDTOSEND.PARTID) qryInventoryOnTransfers ON 
						                    (qtyinventorytotals.PartID = qryInventoryOnTransfers.PartID)
					
					                    GROUP BY part.id
				                    ) AvailableInventory ON AvailableInventory.partid = bomitem.`partId`


				                    WHERE bomitem.`typeId` = 20
				
				                    GROUP BY bomid) BOMInventory ON BOMInventory.bomid = bom.id

			                    WHERE -- bom.id = 65
			                    -- and
			                    bomitem.`typeId` = 10
			                    AND bom.`activeFlag` = TRUE
			                    GROUP BY part.num)  BOMInventory ON BOMInventory.ProductNum = Product.num                 
                           
                            WHERE product.num = '{0}'    


                            GROUP BY PRODUCT.ID, PRODUCT.NUM, PRODUCT.PARTID, qryInventoryOnTransfers.SumofQty


                    UNION

                    SELECT KitItem.`kitProductId` AS PRODUCTID, product.num AS PRODUCTNUM, product.partid, COALESCE(ROUND(MIN(QIT.AVAILABLEFORSALE / kititem.`defaultQty`), 0), 0) AS InStockAvailable,
                        0 AS BuildInventory,
                        (COALESCE(ROUND(MIN(QIT.AVAILABLEFORSALE / kititem.`defaultQty`), 0), 0)) AS AVAILABLEFORSALE
                    FROM KitItem
                    JOIN product ON(kititem.`kitProductId` = product.`id` AND product.`activeFlag` = TRUE)
                    JOIN
                    (SELECT PRODUCT.ID AS PRODUCTID, PRODUCT.PARTID,
                                            (SUM((QTYONHAND - QTYALLOCATED - QTYNOTAVAILABLE) / COALESCE(FACTOR, 1)) + COALESCE(qryInventoryOnTransfers.SumofQty, 0)) AS AVAILABLEFORSALE


                                            FROM PRODUCT
                                            LEFT JOIN QTYINVENTORYTOTALS ON(PRODUCT.PARTID = QTYINVENTORYTOTALS.PARTID)
                                            LEFT JOIN(SELECT PART.ID AS PARTID, UOMCONVERSION.FROMUOMID, PRODUCT.ID AS PRODUCTID,
                                                        UOMCONVERSION.TOUOMID, UOMCONVERSION.MULTIPLY, UOMCONVERSION.FACTOR
                                                        FROM(UOMCONVERSION INNER JOIN PART ON UOMCONVERSION.FROMUOMID = PART.UOMID)
                                                        INNER JOIN PRODUCT ON(UOMCONVERSION.TOUOMID = PRODUCT.UOMID)
                                                        AND(PART.ID = PRODUCT.PARTID)) PARTTOPRODUOMCONV ON(PRODUCT.ID = PARTTOPRODUOMCONV.ProductID)
                                            LEFT JOIN(SELECT QTYALLOCATEDTOSEND.PARTID, SUM(QTYALLOCATEDTOSEND.QTY) AS SumOfQTY
                                                FROM QTYALLOCATEDTOSEND
                                                GROUP BY QTYALLOCATEDTOSEND.PARTID) qryInventoryOnTransfers ON(Product.PartID = qryInventoryOnTransfers.PartID)

                                               GROUP BY PRODUCT.ID, PRODUCT.PARTID, qryInventoryOnTransfers.SumofQty) QIT ON QIT.productid = kititem.`productId`
                    WHERE kititem.`kitTypeId` = 10
                            AND product.num = '{0}'
                      GROUP BY kititem.`kitProductId`, product.num, product.partid
                               ", ProductNumber);

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        //TODO: multiple row check (shouldnt happen)
                        productAvailableInventory = new ProductAvailableInventory
                        {
                            AvailableInStock = reader["InStockAvailable"].ToString(),
                            AvailableForSale = (string)reader["AVAILABLEFORSALE"].ToString(),
                            AvailableToBuild = (string)reader["BuildInventory"].ToString()
                        };
                    }
                }
            }

            return productAvailableInventory;

        }

        public async Task<List<ProductDistCase>> getFBProductDistCaseQuantities()
        {
            //await Load();

            List<ProductDistCase> productDistCases = new List<ProductDistCase>();

            string query = @"SELECT Info AS CaseQty, Product.ID AS ProductID, NUM AS ProductNumber 
                            FROM CUSTOMDECIMAL 
                            INNER JOIN PRODUCT ON (CUSTOMDECIMAL.RECORDID = PRODUCT.ID)
                            WHERE CUSTOMDECIMAL.CUSTOMFIELDID = 2
                           ";

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync())
                    {
                        productDistCases.Add(new ProductDistCase
                        {
                            ProductID = reader.GetInt32(1),
                            ProductNumber = reader.GetString(2),
                            CaseQty = reader.GetDecimal(0)
                        });
                    }
                }
            }
            return productDistCases;



        }

        public async Task<List<ProductPricingLevel>> getFBProductPricingLevels(string ProductNumber)
        {
            if (String.IsNullOrEmpty(ProductNumber))
            {
                throw new ArgumentNullException("Product Number is required");
            }
            //await Load();

            List<ProductPricingLevel> productPricingLevels = new List<ProductPricingLevel>();

            string query = String.Format(
                @"SELECT NUM AS ProductNumber, COALESCE(Price,0) AS LevelPrice, 'Retail' AS LevelGroup 
                         FROM PRODUCT
                        WHERE UPPER(Product.Num) = UPPER('{0}')

                    UNION

                    SELECT NUM AS ProductNumber, COALESCE(T2Pricing.RoundedPrice,0) AS LevelPrice, 'Distributor' AS LevelGroup 
                         FROM PRODUCT
                         JOIN (SELECT RoundedPrice, ProductID, product.num AS ProductNumber
                       FROM TIER2ADJPRICING 
                       JOIN Product ON Product.id = Tier2adjpricing.`PRODUCTID`
                       WHERE TIER2ADJPRICING.PRCUSTOMERINCLID = 3 AND TIER2ADJPRICING.PRCUSTOMERINCLTYPEID = 3) T2Pricing ON 
                        (PRODUCT.ID = T2Pricing.PRODUCTID AND UPPER(T2Pricing.ProductNumber) = UPPER('{0}')) 

                    UNION

                    SELECT NUM AS ProductNumber, COALESCE(T3pricing.Tier3RoundedPrice,0) AS LevelPrice, 'Sprinkler House' AS LevelGroup  
                         FROM PRODUCT		
                         JOIN (SELECT DISTINCT Tier3RoundedPrice, ProductID, product.num AS ProductNumber 
                           FROM TIER3ADJPRICING
                           JOIN Product ON Product.id = TIER3ADJPRICING.`PRODUCTID`
                                                WHERE TIER3ADJPRICING.PRCUSTOMERINCLID = 11 AND TIER3ADJPRICING.PRCUSTOMERINCLTYPEID = 3) T3pricing ON 
                            (PRODUCT.ID = T3pricing.PRODUCTID AND UPPER(T3pricing.ProductNumber) = UPPER('{0}'))  
                           ", ProductNumber);

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        productPricingLevels.Add(new ProductPricingLevel
                        {
                            ProductNumber = (string)reader["ProductNumber"],
                            LevelPrice = (decimal)reader["LevelPrice"],
                            Group = (string)reader["LevelGroup"]
                        });
                    }
                }
            }

            if (productPricingLevels?.Count == 0)
            {
                throw new KeyNotFoundException("No pricing levels found");
            }

            return productPricingLevels;


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerm">SearchTerm must contain the special characters for locaiotn and tracking lookup</param>
        /// <returns></returns>
        [Obsolete]
        public async Task<List<InvQtyWithTracking>> GetPartTagAndTracking(string searchTerm)
        {
            if (String.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentNullException("Search Term is required");
            }
            //await Load();

            List<InvQtyWithTracking> invQtywithTrackings = new List<InvQtyWithTracking>();

            string query = null;

            if (searchTerm.Contains("$L$") || searchTerm.Contains("$T$"))
            {
                //change to part only query
            query = String.Format(@"SELECT part.num, tag.`qty`, tag.id AS tagid, tag.`num` AS tagNum, tag.`locationId`, location.`name` AS LocationName,
                location.`pickable` AS locationPickable,
                COALESCE(trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS primaryInfo,
                parttracking.name AS PrimaryTrackingLabel, parttracking.abbr as TrackingAbbr, locationgroup.name AS LocationGroupName,

                1 AS UPCCaseQty

                FROM part
                LEFT JOIN tag ON tag.`partId` = part.id
                JOIN location ON tag.`locationId` = location.`id`
                JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                LEFT JOIN parttotracking ON(parttotracking.`partId` = part.id AND parttotracking.`primaryFlag` = 1)
                LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`

                LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`

                LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`

                LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                /* LOOKUP BY PARTNUM, LOCATIONID, PRIMARYTRACKING */
                WHERE
                (UPPER(CONCAT('$T$', COALESCE(trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`))) LIKE '{0}' OR
                CONCAT('$L$', location.name) LIKE '{0}')
                AND location.`typeId` NOT IN(20, 60, 80)

                ORDER BY location.`name`", searchTerm.ToUpper());
            }
            else
            {
                query = String.Format(
                    @"SELECT part.num, sum(tag.`qty`) as qty, tag.id AS tagid, tag.`num` AS tagNum, tag.`locationId`, location.`name` as LocationName, locationgroup.name as LocationGroupName,
                    location.`pickable` AS locationPickable,
                    COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS primaryInfo,
                    parttracking.name AS PrimaryTrackingLabel, parttracking.abbr as TrackingAbbr, locationgroup.name AS LocationGroupName,

                    Cast((COALESCE(uomconversion.multiply,1) / COALESCE(uomconversion.factor,1)) As SIGNED) AS UPCCaseQty

                    FROM product
                    JOIN part ON product.`partId` = part.id
                    LEFT JOIN uomconversion ON (product.`uomId` = uomconversion.`fromUomId` AND uomconversion.`toUomId` = part.`uomId`)
                    LEFT JOIN tag ON tag.`partId` = part.id
                    JOIN location ON tag.`locationId` = location.`id`
                    JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                    LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id AND parttotracking.`primaryFlag` = 1)
	                    LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                    LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	
                    LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`
	
                    LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	
                    LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`
                    
                    /* LOOKUP BY PARTNUM, LOCATIONID, PRIMARYTRACKING */
                    WHERE
                    (UPPER(product.`num`) LIKE '{0}' OR product.`upc` LIKE '{0}' OR
                    UPPER(CONCAT('$T$', COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`))) LIKE '{0}' OR
                    CONCAT('$L$', location.name) LIKE '{0}'  )
                    AND location.`typeId` NOT IN (20,60,80)
                    GROUP BY part.num, locationId, primaryinfo, primarytrackinglabel, upccaseqty
                    Order By location.name
                           ", searchTerm.ToUpper());

            }

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        invQtywithTrackings.Add(new InvQtyWithTracking
                        {
                            LocationId = (int)reader["locationId"],
                            PartNumber = (string)reader["num"],
                            Qty = (decimal)reader["qty"],
                            TagID = (int)(Int64)reader["tagid"],
                            PrimaryTracking = reader["primaryInfo"] == DBNull.Value ? null : reader["primaryInfo"].ToString(),
                            PrimaryTrackingLabel = reader["primaryTrackingLabel"] == DBNull.Value ? null : reader["primaryTrackingLabel"].ToString(),
                            TrackingAbbr = reader["TrackingAbbr"] == DBNull.Value ? null : reader["TrackingAbbr"].ToString(),
                            LocationName = reader["LocationName"] == DBNull.Value ? null : reader["LocationName"].ToString(),
                            LocationGroupName = reader["LocationGroupName"] == DBNull.Value ? null : reader["LocationGroupName"].ToString(),
                            UPCCaseQty = (int)(Int64)reader["UPCCaseQty"]

                        });
                    }
                }
            }

            if (invQtywithTrackings.Count == 0)
            {
                //check to make sure the part is valid
                if (!(await CheckPartLocationTrackingIsValid(searchTerm)))
                {
                    throw new ArgumentException("Search Term is not found. Please check.");
                }
                else
                {
                    throw new KeyNotFoundException("No records found");
                }
            }


            return invQtywithTrackings;

        }

        /// <summary>
        /// Returns all part and tracking for a location, product, or tracking and alos included the default location for the part
        /// </summary>
        /// <param name="SearchTerm">SearchTerm must contain the special characters for location and tracking lookup</param>
        /// <param name="LocationGroupName">Pass in LG name to get the default location with the record</param>
        /// <returns></returns>
        public async Task<List<InvQtyWithTracking>> GetPartTagAndPrimaryTrackingWithDefaultLocation(string SearchTerm, string LocationGroupName)
        {
            if (String.IsNullOrEmpty(SearchTerm))
            {
                throw new ArgumentNullException("Search Term is required");
            }
            //await Load();

            List<InvQtyWithTracking> invQtywithTrackings = new List<InvQtyWithTracking>();

            string query = null;

            if (SearchTerm.Contains("$L$") || SearchTerm.Contains("$T$"))
            {
                //change to part only query
                query = String.Format(@"SELECT part.num, tag.`qty`, tag.id AS tagid, tag.`num` AS tagNum, tag.`locationId`, location.`name` AS LocationName,
                location.`pickable` AS locationPickable,                
                COALESCE(trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS primaryInfo,
                parttracking.name AS PrimaryTrackingLabel, parttracking.abbr as TrackingAbbr, locationgroup.name AS LocationGroupName,

                1 AS UPCCaseQty, dfl.DefaultLocationName

                FROM part
                LEFT JOIN tag ON tag.`partId` = part.id
                JOIN location ON tag.`locationId` = location.`id`
                JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                LEFT JOIN parttotracking ON(parttotracking.`partId` = part.id AND parttotracking.`primaryFlag` = 1)
                LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`

                LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`

                LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`

                LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                LEFT JOIN (SELECT defaultlocation.`partId`, dflLocation.`name` AS DefaultLocationName
		                FROM defaultlocation
		                JOIN location dflLocation ON dflLocation.`id` = defaultlocation.`locationId`
		                JOIN locationgroup dflGroup ON (dflGroup.`id` = defaultlocation.`locationGroupId` AND dflGroup.`name` = '{1}')
		                ) dfl ON part.`id` = dfl.`partId` 

                /* LOOKUP BY PARTNUM, LOCATIONID, PRIMARYTRACKING */
                WHERE
                (UPPER(CONCAT('$T$', COALESCE(trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`))) LIKE '{0}' OR
                CONCAT('$L$', location.name) LIKE '{0}')
                AND location.`typeId` NOT IN(20, 60, 80)

                ORDER BY location.`name`", SearchTerm.ToUpper(), LocationGroupName);
            }
            else
            {
                query = string.Format(
                    @"SELECT part.num, sum(tag.`qty`) as qty, tag.id AS tagid, tag.`num` AS tagNum, tag.`locationId`, location.`name` as LocationName, locationgroup.name as LocationGroupName,
                    location.`pickable` AS locationPickable,                    
                    COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS primaryInfo,
                    parttracking.name AS PrimaryTrackingLabel, parttracking.abbr as TrackingAbbr, locationgroup.name AS LocationGroupName,

                    Cast((COALESCE(uomconversion.multiply,1) / COALESCE(uomconversion.factor,1)) As SIGNED) AS UPCCaseQty, dfl.DefaultLocationName

                    FROM product
                    JOIN part ON product.`partId` = part.id
                    LEFT JOIN uomconversion ON (product.`uomId` = uomconversion.`fromUomId` AND uomconversion.`toUomId` = part.`uomId`)
                    LEFT JOIN tag ON tag.`partId` = part.id
                    JOIN location ON tag.`locationId` = location.`id`
                    JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                    LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id AND parttotracking.`primaryFlag` = 1)
	                    LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                    LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	
                    LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`
	
                    LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	
                    LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`
                    
                    LEFT JOIN (SELECT defaultlocation.`partId`, dflLocation.`name` AS DefaultLocationName
		                FROM defaultlocation
		                JOIN location dflLocation ON dflLocation.`id` = defaultlocation.`locationId`
		                JOIN locationgroup dflGroup ON (dflGroup.`id` = defaultlocation.`locationGroupId` AND dflGroup.`name` = '{1}')
		                ) dfl ON part.`id` = dfl.`partId` 

                    /* LOOKUP BY PARTNUM, LOCATIONID, PRIMARYTRACKING */
                    WHERE
                    (UPPER(part.`num`) LIKE '{0}' OR product.`upc` LIKE '{0}' OR
                    UPPER(CONCAT('$T$', COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`))) LIKE '{0}' OR
                    CONCAT('$L$', location.name) LIKE '{0}'  )
                    AND location.`typeId` NOT IN (20,60,80)
                    GROUP BY part.num, locationId, tagid, primaryinfo, primarytrackinglabel, upccaseqty
                    Order By location.name
                           ", SearchTerm.ToUpper(), LocationGroupName);

            }

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        invQtywithTrackings.Add(new InvQtyWithTracking
                        {
                            LocationId = (int)reader["locationId"],
                            PartNumber = (string)reader["num"],
                            Qty = (decimal)reader["qty"],
                            TagID = (int)(Int64)reader["tagid"],
                            PrimaryTracking = reader["primaryInfo"] == DBNull.Value ? null : reader["primaryInfo"].ToString(),
                            PrimaryTrackingLabel = reader["primaryTrackingLabel"] == DBNull.Value ? null : reader["primaryTrackingLabel"].ToString(),
                            TrackingAbbr = reader["TrackingAbbr"] == DBNull.Value ? null : reader["TrackingAbbr"].ToString(),
                            LocationName = reader["LocationName"] == DBNull.Value ? null : reader["LocationName"].ToString(),
                            LocationPickable = Convert.ToBoolean((UInt64)reader["locationPickable"]),
                            LocationGroupName = reader["LocationGroupName"] == DBNull.Value ? null : reader["LocationGroupName"].ToString(),
                            UPCCaseQty = (int)(Int64)reader["UPCCaseQty"],
                            DefaultLocationName = reader["DefaultLocationName"] == DBNull.Value ? null : reader["DefaultLocationName"].ToString(),

                        });
                    }
                }
            }

            if (invQtywithTrackings.Count == 0)
            {
                //check to make sure the part is valid
                if (!(await CheckPartLocationTrackingIsValid(SearchTerm)))
                {
                    throw new ArgumentException("Search Term is not found. Please check.");
                }
                else
                {
                    throw new KeyNotFoundException("No records found");
                }
            }


            return invQtywithTrackings;

        }



        /// <summary>
        /// Returns part and all tracking for a location, product, or tracking and also includes the default location for the part
        /// </summary>
        /// <param name="SearchTerm">SearchTerm must contain the special characters for location and tracking lookup</param>
        /// <param name="LocationGroupName">Pass in LG name to get the default location with the record</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when part not found</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no records returned</exception>
        public async Task<List<InvQtyWithAllTracking>> GetPartTagAndAllTrackingWithDefaultLocation(string SearchTerm
            , string LocationGroupName, InventorySearchTermType searchTermType)
        {
            if (String.IsNullOrEmpty(SearchTerm))
            {
                throw new ArgumentNullException("Search Term is required");
            }
            //await Load();

            List<InvQtyWithAllTracking> invQtywithAllTrackings = new List<InvQtyWithAllTracking>();

            string query = null;

            if (SearchTerm.Contains("$L$"))
            {
                query = string.Format(@"SELECT part.num AS PartNumber
	                        , SUM(tag.`qty`) AS Qty
	                        , tag.id AS TagID
	                        , tag.`num` AS tagNum
	                        , tag.`locationId`
	                        , location.`name` AS LocationName
	                        , location.`pickable` AS LocationPickable
	                        #, COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS TrackingInfo
	                        , COALESCE (DATE_FORMAT(trackingdate.`info`,'%m/%d/%Y'), trackingdecimal.`info`, 
                                                    CASE WHEN parttracking.`typeId` = 80 THEN 
	                                                    CASE WHEN trackinginteger.`info` = 0 THEN 'false'
		                                                    ELSE 'true'
		                                                    END
			                            ELSE trackinginteger.`info`
			                            END, 

                                                    trackingtext.`info`) AS TrackingInfo
	
	                        , parttracking.name AS TrackingLabel
	                        , parttracking.`abbr` AS TrackingAbbr
	                        , COALESCE(parttracking.`typeId`,0) AS TrackingTypeID
	                        , COALESCE(parttracking.`id`,0) AS TrackingID
                            , COALESCE(parttracking.sortorder,0) AS TrackingSortOrder
	                        , COALESCE(parttotracking.`primaryFlag`,0) AS IsPrimaryTracking
	
	                        , locationgroup.name AS LocationGroupName
	                        , 1 AS UPCCaseQty
	                        , dfl.DefaultLocationName

                        FROM part 
                        LEFT JOIN tag ON tag.`partId` = part.id
                        JOIN location ON tag.`locationId` = location.`id`
                        JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                        LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id)# AND parttotracking.`primaryFlag` = 1)
                         LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                        LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	
                        LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`
	
                        LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	
                        LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                        LEFT JOIN (SELECT defaultlocation.`partId`, dflLocation.`name` AS DefaultLocationName
		                        FROM defaultlocation
		                        JOIN location dflLocation ON dflLocation.`id` = defaultlocation.`locationId`
		                        JOIN locationgroup dflGroup ON (dflGroup.`id` = defaultlocation.`locationGroupId` AND dflGroup.`name` = '{1}')
		                        ) dfl ON part.`id` = dfl.`partId` 
		
                        /* LOOKUP BY LOCATION */
                        WHERE
                        CONCAT('$L$', location.name) LIKE '{0}'
                        AND location.`typeId` NOT IN (20,60,80)

                        GROUP BY part.num, locationId, tagid, trackinginfo, trackinglabel, upccaseqty
                        ORDER BY location.`name`, tagid, parttracking.sortorder", SearchTerm.ToUpper(), LocationGroupName);
            }
            else if(SearchTerm.Contains("$T$"))
            {
                //change to part only query
                query = string.Format(@"SELECT part.num AS PartNumber
	                                , SUM(tag.`qty`) AS Qty
	                                , tag.id AS TagID
	                                , tag.`num` AS tagNum
	                                , tag.`locationId`
	                                , location.`name` AS LocationName
	                                , location.`pickable` AS LocationPickable
	                                #, COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS TrackingInfo
	                                , COALESCE (DATE_FORMAT(trackingdate.`info`,'%m/%d/%Y'), trackingdecimal.`info`, 
                                                            CASE WHEN parttracking.`typeId` = 80 THEN 
	                                                            CASE WHEN trackinginteger.`info` = 0 THEN 'false'
		                                                            ELSE 'true'
		                                                            END
			                                    ELSE trackinginteger.`info`
			                                    END, 

                                                            trackingtext.`info`) AS TrackingInfo
	
	                                , parttracking.name AS TrackingLabel
	                                , parttracking.`abbr` AS TrackingAbbr
	                                , COALESCE(parttracking.`typeId`,0) AS TrackingTypeID
	                                , COALESCE(parttracking.`id`,0) AS TrackingID
                                    , COALESCE(parttracking.sortorder,0) AS TrackingSortOrder
	                                , COALESCE(parttotracking.`primaryFlag`,0) AS IsPrimaryTracking
	
	                                , locationgroup.name AS LocationGroupName
	                                , 1 AS UPCCaseQty
	                                , dfl.DefaultLocationName

                                FROM part 
                                LEFT JOIN tag ON tag.`partId` = part.id

                                join (select tag.id as TagID
	                                from part
	                                left JOIN tag ON tag.`partId` = part.id
	                                LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id)
	                                #LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`
	                                LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	                                LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`	
	                                LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	                                LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`
	                                WHERE
                                        (UPPER(CONCAT('$T$', COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`))) LIKE '{0}')) 
		                                trackingInfoTag on trackingInfoTag.TagID = tag.id



                                JOIN location ON tag.`locationId` = location.`id`
                                JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                                LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id)# AND parttotracking.`primaryFlag` = 1)
                                 LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                                LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	
                                LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`
	
                                LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	
                                LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                                LEFT JOIN (SELECT defaultlocation.`partId`, dflLocation.`name` AS DefaultLocationName
		                                FROM defaultlocation
		                                JOIN location dflLocation ON dflLocation.`id` = defaultlocation.`locationId`
		                                JOIN locationgroup dflGroup ON (dflGroup.`id` = defaultlocation.`locationGroupId` AND dflGroup.`name` = '{1}')
		                                ) dfl ON part.`id` = dfl.`partId` 
		
                                /* LOOKUP BY TRACKING */
                                WHERE
                                location.`typeId` NOT IN (20,60,80)

                                GROUP BY part.num, locationId, tagid, trackinginfo, trackinglabel, upccaseqty
                                ORDER BY location.`name`, tagid, parttracking.sortorder", SearchTerm.ToUpper(), LocationGroupName);
            }
            else
            {
                switch (searchTermType)
                {
                    case InventorySearchTermType.Part:
                        query = string.Format(@"SELECT part.num AS PartNumber
                                , SUM(tag.`qty`) AS Qty
                                , tag.id AS TagID
                                , tag.`num` AS tagNum
                                , tag.`locationId`
	                            , location.`name` AS LocationName
                                , location.`pickable` AS LocationPickable
                                , COALESCE(DATE_FORMAT(trackingdate.`info`, '%m/%d/%Y'), trackingdecimal.`info`,
                                                        CASE WHEN parttracking.`typeId` = 80 THEN

                                                            CASE WHEN trackinginteger.`info` = 0 THEN 'false'
                                                                ELSE 'true'
                                                                END

                                            ELSE trackinginteger.`info`
                                            END,

                                                        trackingtext.`info`) AS TrackingInfo

                                , parttracking.name AS TrackingLabel
                                , parttracking.`abbr` AS TrackingAbbr
                                , COALESCE(parttracking.`typeId`, 0) AS TrackingTypeID
                                , COALESCE(parttracking.`id`, 0) AS TrackingID
                                , COALESCE(parttracking.sortorder, 0) AS TrackingSortOrder
                                , COALESCE(parttotracking.`primaryFlag`, 0) AS IsPrimaryTracking

                                , locationgroup.name AS LocationGroupName
                                , 0 AS UPCCaseQty
                                , dfl.DefaultLocationName

                            FROM part
                            #JOIN product ON product.`partId` = part.id
                            #LEFT JOIN uomconversion ON (product.`uomId` = uomconversion.`fromUomId` AND uomconversion.`toUomId` = part.`uomId`)
                            LEFT JOIN tag ON tag.`partId` = part.id
                            LEFT JOIN location ON tag.`locationId` = location.`id`
                            LEFT JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                            LEFT JOIN parttotracking ON(parttotracking.`partId` = part.id)# AND parttotracking.`primaryFlag` = 1)
                                LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                            LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`


                            LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`


                            LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`


                            LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                            LEFT JOIN(SELECT defaultlocation.`partId`, dflLocation.`name` AS DefaultLocationName

                                    FROM defaultlocation

                                    JOIN location dflLocation ON dflLocation.`id` = defaultlocation.`locationId`

                                    JOIN locationgroup dflGroup ON(dflGroup.`id` = defaultlocation.`locationGroupId` AND dflGroup.`name` = '{1}')
                                    ) dfl ON part.`id` = dfl.`partId` 

                            /* LOOKUP BY PART NUM OR PART UPC */
                            WHERE
                            (UPPER(part.`num`) LIKE '{0}' OR part.`upc` LIKE '{0}' )
                            AND(location.`typeId` NOT IN(20, 60, 80) OR location.`typeId` IS NULL)

                            GROUP BY part.num, locationId, trackinginfo, trackinglabel, upccaseqty
                            ORDER BY location.`name`, tagid, parttracking.sortorder", SearchTerm.ToUpper(), LocationGroupName);


                        break;

                    case InventorySearchTermType.Product:
                        query = string.Format(@"SELECT part.num AS PartNumber
	                                    , SUM(tag.`qty`) AS Qty
	                                    , tag.id AS TagID
	                                    , tag.`num` AS tagNum
	                                    , tag.`locationId`
	                                    , location.`name` AS LocationName
	                                    , location.`pickable` AS LocationPickable
	                                    #, COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS TrackingInfo
	                                    , COALESCE (DATE_FORMAT(trackingdate.`info`,'%m/%d/%Y'), trackingdecimal.`info`, 
                                                                CASE WHEN parttracking.`typeId` = 80 THEN 
	                                                                CASE WHEN trackinginteger.`info` = 0 THEN 'false'
		                                                                ELSE 'true'
		                                                                END
			                                        ELSE trackinginteger.`info`
			                                        END, 

                                                                trackingtext.`info`) AS TrackingInfo
	
	                                    , parttracking.name AS TrackingLabel
	                                    , parttracking.`abbr` AS TrackingAbbr
	                                    , COALESCE(parttracking.`typeId`,0) AS TrackingTypeID
	                                    , COALESCE(parttracking.`id`,0) AS TrackingID
                                        , COALESCE(parttracking.sortorder,0) AS TrackingSortOrder
	                                    , COALESCE(parttotracking.`primaryFlag`,0) AS IsPrimaryTracking
	
	                                    , locationgroup.name AS LocationGroupName
	                                    , CAST((COALESCE(uomconversion.multiply,1) / COALESCE(uomconversion.factor,1)) AS SIGNED) AS UPCCaseQty
	                                    , dfl.DefaultLocationName

                                    FROM product
                                    JOIN part ON product.`partId` = part.id
                                    LEFT JOIN uomconversion ON (product.`uomId` = uomconversion.`fromUomId` AND uomconversion.`toUomId` = part.`uomId`)
                                    LEFT JOIN tag ON tag.`partId` = part.id
                                    LEFT JOIN location ON tag.`locationId` = location.`id`
                                    LEFT JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`

                                    LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id)# AND parttotracking.`primaryFlag` = 1)
                                     LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                                    LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	
                                    LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`
	
                                    LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	
                                    LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                                    LEFT JOIN (SELECT defaultlocation.`partId`, dflLocation.`name` AS DefaultLocationName
		                                    FROM defaultlocation
		                                    JOIN location dflLocation ON dflLocation.`id` = defaultlocation.`locationId`
		                                    JOIN locationgroup dflGroup ON (dflGroup.`id` = defaultlocation.`locationGroupId` AND dflGroup.`name` = '{1}')
		                                    ) dfl ON part.`id` = dfl.`partId` 
		
                                    /* LOOKUP BY PRODUCT NUM OR UPC */
                                    WHERE
                                    (UPPER(product.`num`) LIKE '{0}' OR product.`upc` LIKE '{0}' )
                                    AND (location.`typeId` NOT IN (20,60,80) OR location.`typeId` IS NULL)

                                    GROUP BY part.num, locationId, tagid, trackinginfo, trackinglabel, upccaseqty
                                    ORDER BY location.`name`, tagid, parttracking.sortorder", SearchTerm.ToUpper(), LocationGroupName);


                        break;
                    
                }
                

            }

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        invQtywithAllTrackings.Add(new InvQtyWithAllTracking
                        {
                            PartNumber = (string)reader["PartNumber"],
                            Qty = reader["qty"] == DBNull.Value ? 0 : (decimal)reader["qty"],
                            TagID = reader["tagid"] == DBNull.Value ? 0 : (int)(Int64)reader["tagid"],
                            LocationId = reader["locationId"] == DBNull.Value ? 0 : (int)reader["locationId"],
                            LocationName = reader["LocationName"] == DBNull.Value ? null : reader["LocationName"].ToString(),
                            LocationPickable = reader["locationPickable"] == DBNull.Value ? false : Convert.ToBoolean((UInt64)reader["locationPickable"]),
                            TrackingInfo = reader["TrackingInfo"] == DBNull.Value ? null : reader["TrackingInfo"].ToString(),
                            TrackingLabel = reader["TrackingLabel"] == DBNull.Value ? null : reader["TrackingLabel"].ToString(),
                            TrackingAbbr = reader["TrackingAbbr"] == DBNull.Value ? null : reader["TrackingAbbr"].ToString(),

                            TrackingTypeID = (int)(long)reader["TrackingTypeID"],
                            TrackingID = (int)(long)reader["TrackingID"],
                            TrackingSortOrder = (int)(long)reader["TrackingSortOrder"],
                            IsPrimaryTracking = Convert.ToBoolean((decimal)reader["IsPrimaryTracking"]),

                            LocationGroupName = reader["LocationGroupName"] == DBNull.Value ? null : reader["LocationGroupName"].ToString(),
                            UPCCaseQty = (int)(Int64)reader["UPCCaseQty"],
                            DefaultLocationName = reader["DefaultLocationName"] == DBNull.Value ? null : reader["DefaultLocationName"].ToString(),

                        });
                    }
                }
            }

            if (invQtywithAllTrackings.Count == 0)
            {
                //check to make sure the part is valid
                if (!(await CheckPartLocationTrackingIsValid(SearchTerm)))
                {
                    throw new ArgumentException("Search Term is not found. Please check.");
                }
                else
                {
                    throw new KeyNotFoundException("No records returned");
                }
            }


            return invQtywithAllTrackings;

        }


        /// <summary>
        /// Checks FB to see if the supplied location, part number or tracking info is valid
        /// </summary>
        /// <param name="searchTerm">SearchTerm must contain the special characters for locaiotn and tracking lookup</param>
        /// <returns></returns>
        public async Task<bool> CheckPartLocationTrackingIsValid(string searchTerm)
        {
            if (String.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentNullException("Search Term is required");
            }
            //await Load();

            string returnedInfo = null;
            string query = String.Format(
                @"SELECT part.num, 'Part' AS returnType
                    FROM part
                    JOIN product ON part.id = product.`partId`
                        WHERE
                        (UPPER(part.`num`) = '{0}' OR product.`upc` = '{0}')
    
                UNION

                SELECT location.name , 'Location' AS returnType
                FROM location
                WHERE CONCAT('$L$', location.name) = '{0}'

                UNION

                SELECT COALESCE (trackingdate.`info`, trackingdecimal.`info`, trackinginteger.`info`, trackingtext.`info`) AS Info, 'Tracking' AS returnType

                FROM part 
                LEFT JOIN tag ON tag.`partId` = part.id
                LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id AND parttotracking.`primaryFlag` = 1)
	                LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`
                LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`	
                LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`	
                LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`	
                LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                WHERE UPPER(CONCAT('$T$', COALESCE (trackingdate.`info`, trackingdecimal.`info`, 
                    trackinginteger.`info`, trackingtext.`info`))) LIKE '{0}'", searchTerm.ToUpper());

            using (var result = new MySqlCommand(query, Connection))
            {
                returnedInfo = (string) await result.ExecuteScalarAsync();

            }

            if (returnedInfo == null)
            {
                //nothing found, invalid
                return false;
            }
            return true;
        }

        public async Task<string> GetPartNumberFromProductOrUPC(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                throw new ArgumentNullException("Search term required");
            }
            string query = string.Format(@"Select Part.num as PartNumber 
                                from part
                                join product on part.id = product.partid
                                where product.num like '{0}' OR product.upc like '{0}'",searchTerm);

            using (var result = new MySqlCommand(query, Connection))
            {
                return (string)await result.ExecuteScalarAsync();

            }
        }

        #endregion

        #region Receiving

        public async Task InsertMobileReceipt(IMobileReceipt mobileReceipt, List<IMobileReceiptItem> items)
        {
            //open connection to mysql
            //start transaction
            //insert MR
            //loop and insert MRI

            //await Load();

            MySqlTransaction MrTransaction = await Connection.BeginTransactionAsync();

            using (var MRtransactionCommand = new MySqlCommand(Connection, MrTransaction))
            {
                //insert the mobile receipt first
                MRtransactionCommand.CommandText = String.Format(@"INSERT INTO `briteideasupdate`.`bi_mr` (
                                  `mr_id`, `description`, `time_started`, `time_finished`, `time_uploaded`, `status_id` )
                                VALUES (
                                    '{0}',  '{1}',  '{2}',  '{3}',  '{4}',  '{5}'
                                  )", mobileReceipt.mrId, mobileReceipt.description, mobileReceipt.timeStarted.ToString("yyyy-MM-dd HH:mm:ss"),
                                  mobileReceipt.timeFinished.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                  mobileReceipt.statusId);

                await MRtransactionCommand.ExecuteNonQueryAsync();
                //long newReceiptId = MRtransactionCommand.LastInsertedId;

                //Debug.WriteLine(newReceiptId);

                try
                {
                    //disable checks temporarily since we are still in the transaction
                    MRtransactionCommand.CommandText = "SET FOREIGN_KEY_CHECKS = 0";
                    await MRtransactionCommand.ExecuteNonQueryAsync();


                    //insert mobile receipt items
                    foreach (IMobileReceiptItem item in items)
                    {
                        MRtransactionCommand.CommandText = String.Format(@"INSERT INTO `briteideasupdate`.`bi_mri` (
                                  `mrid`, `upc`, `timeScanned`, `statusId` )
                                VALUES (
                                    '{0}',  '{1}',  '{2}',  '{3}'
                                  )", item.mrId, item.upc, item.timeScanned.ToString("yyyy-MM-dd HH:mm:ss"), item.statusID);

                        await MRtransactionCommand.ExecuteNonQueryAsync();

                    }


                    MRtransactionCommand.CommandText = "SET FOREIGN_KEY_CHECKS = 1";
                    await MRtransactionCommand.ExecuteNonQueryAsync();

                    await MrTransaction.CommitAsync();
                }
                catch (MySqlException e)
                {
                    //await MriTransaction.RollbackAsync(); //if items fail rollback parent also
                    await MrTransaction.RollbackAsync();
                    throw e;
                }
                //}
            }



        }

        #endregion


        //     public static string GetCustomerNameFromAccountNumber(int AccountID)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT Name FROM Customer WHERE AccountID = " + AccountID
        //                        , connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             return TempFBData.Rows[0].Field<string>("Name");
        //         }

        //     }

        //     #region products
        //     public static DataTable FBProductsSoldOnline()
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT PRODUCT.ID, PRODUCT.NUM as PRODUCTNUMBER, PRODUCT.DESCRIPTION AS name, 
        //                         PRODUCT.DESCRIPTION AS description, PRODUCT.DESCRIPTION AS short_description, 
        //                         PRODUCT.WEIGHT AS weight, PRODUCT.PRICE AS price
        //                         FROM PRODUCT
        //                         INNER JOIN CUSTOMINTEGER ON (CUSTOMINTEGER.RECORDID = PRODUCT.ID AND CUSTOMINTEGER.INFO=1 AND CUSTOMINTEGER.CUSTOMFIELDID=31)
        //                         WHERE PRODUCT.ACTIVEFLAG=1
        //                         ORDER BY PRODUCT.NUM
        //                        ", connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             return TempFBData;

        //         }
        //     }

        #region Products And Parts

        public async Task<List<ProductSpec>> getProductSpecs(string ProductNumber)
        {
            if (String.IsNullOrEmpty(ProductNumber))
            {
                throw new ArgumentNullException("Product Number is required");
            }

            //await Load();

            List<ProductSpec> productSpecs = new List<ProductSpec>();

            string query = String.Format(

                        @"SELECT 'Number' AS specName, PRODUCT.NUM AS specValue
	                        FROM PRODUCT
	                        WHERE product.num = '{0}'

                        UNION
                        SELECT 'Weight' AS specName, CONCAT(ROUND(Product.Weight,2), ' lbs') AS specValue
	                        FROM PRODUCT
	                        WHERE product.num = '{0}'

                        UNION
                        SELECT 'Amperage' AS specName, tAmperage.info AS specValue
	                        FROM PRODUCT
	                        JOIN customvarchar AS tAmperage ON (product.id = tAmperage.recordId AND tAmperage.customFieldId = 23)
	                        WHERE product.num = '{0}'
                        UNION
                        SELECT 'Dimensions' AS specName, tDimensions.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customvarchar AS tDimensions ON (tDimensions.recordid = product.id AND tDimensions.customfieldid = 22 )
	                        WHERE product.num = '{0}'
                        UNION
                        SELECT 'Number of Lights' AS specName, tNumber_Of_Lights.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customdecimal AS tNumber_Of_Lights ON (tNumber_Of_Lights.recordid = product.id AND tNumber_Of_Lights.customfieldid = 26 )
	                        WHERE product.num = '{0}'
                        UNION
                        SELECT 'Watts' AS specName, tWatts.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customdecimal AS tWatts ON (tWatts.recordid = product.id AND tWatts.customfieldid = 27 )
	                        WHERE product.num = '{0}'	
                        UNION
                        SELECT 'Tip Count' AS specName, tTip_Count.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customdecimal AS tTip_Count ON (tTip_Count.recordid = product.id AND tTip_Count.customfieldid = 30 )
	                        WHERE product.num = '{0}'		

                        UNION
                        SELECT 'Light Color' AS specName, tLight_Color.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customset AS tLight_Color ON (tLight_Color.recordid = product.id AND tLight_Color.customfieldid = 42 )
	                        WHERE product.num = '{0}'	
	
                        UNION
                        SELECT 'Wire Color' AS specName, tWire_Color.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customset AS tWire_Color ON (tWire_Color.recordid = product.id AND tWire_Color.customfieldid = 43 )
	                        WHERE product.num = '{0}'	
                        UNION
                        SELECT 'Bulb Spacing' AS specName, tBulb_Spacing.info AS specValue
	                        FROM PRODUCT
	                        JOIN customvarchar AS tBulb_Spacing ON (tBulb_Spacing.recordid = product.id AND tBulb_Spacing.customfieldid = 44 ) 
	                        WHERE product.num = '{0}'
                        UNION
                        SELECT 'Lead Length' AS specName, tLead_Length.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customvarchar AS tLead_Length ON (tLead_Length.recordid = product.id AND tLead_Length.customfieldid = 45 )
	                        WHERE product.num = '{0}'	
                        UNION
                        SELECT 'Tail Length' AS specName, tTail_Length.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customvarchar AS tTail_Length ON (tTail_Length.recordid = product.id AND tTail_Length.customfieldid = 46  )
	                        WHERE product.num = '{0}'		
                        UNION
                        SELECT 'Wire Gauge' AS specName, tWire_Gauge.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customset AS tWire_Gauge ON (tWire_Gauge.recordid = product.id AND tWire_Gauge.customfieldid = 47 )
	                        WHERE product.num = '{0}'  
                        UNION
                        SELECT 'Male Plug Type' AS specName, tMale_Plug_Type.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customset AS tMale_Plug_Type ON (tMale_Plug_Type.recordid = product.id AND tMale_Plug_Type.customfieldid = 48 )
	                        WHERE product.num = '{0}'     
                        UNION
                        SELECT 'Female Plug Type' AS specName, tFemale_Plug_Type.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customset AS tFemale_Plug_Type ON (tFemale_Plug_Type.recordid = product.id AND tFemale_Plug_Type.customfieldid = 49 )
	                        WHERE product.num = '{0}'      
                        UNION
                        SELECT 'Voltage' AS specName, tVoltage.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customvarchar AS tVoltage ON (tVoltage.recordid = product.id AND tVoltage.customfieldid = 52 )
	                        WHERE product.num = '{0}'   
                        UNION
                        SELECT 'Bulb Type' AS specName, tBulb_Type.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customset AS tBulb_Type ON (tBulb_Type.recordid = product.id AND tBulb_Type.customfieldid = 53 )
	                        WHERE product.num = '{0}'   
                        UNION
                        SELECT 'Average Hours' AS specName, tAverage_Hours.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customvarchar AS tAverage_Hours ON (tAverage_Hours.recordid = product.id AND tAverage_Hours.customfieldid = 54 )
	                        WHERE product.num = '{0}'           
                        UNION
                        SELECT 'Usage' AS specName, tUSAGE.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customset AS tUSAGE ON (tUSAGE.recordid = product.id AND tUSAGE.customfieldid = 55 )
	                        WHERE product.num = '{0}'   
                        UNION
                        SELECT 'String Length' AS specName, tString_Length.info AS specValue
	                        FROM PRODUCT
	                        JOIN  customvarchar AS tString_Length ON (tString_Length.recordid = product.id AND tString_Length.customfieldid = 57  )
	                        WHERE product.num = '{0}'
                        UNION
                        SELECT 'Max Strings Connected' AS specName, tMax_Strings_Connected.info AS specValue
	                        FROM PRODUCT
	                        JOIN  custominteger AS tMax_Strings_Connected ON (tMax_Strings_Connected.recordid = product.id AND tMax_Strings_Connected.customfieldid = 56 )
	                        WHERE product.num = '{0}'        
	
                                ", ProductNumber);

            using (var result = new MySqlCommand(query,Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        productSpecs.Add(new ProductSpec
                        {
                            SpecName = reader["specName"].ToString(),
                            SpecValue = reader["specValue"].ToString()

                        });
                    }
                }


            }

            if (productSpecs?.Count == 0)
            {
                throw new KeyNotFoundException("No specs found");
            }


            return productSpecs;

        
    }
        /// <summary>
        /// Get a list of active products from the fishbowl database. Only returns inventoried and non-inventoried part types
        /// </summary>
        /// <returns>List of ProductSimpleObject Objects</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no records are found</exception>
        public async Task<List<ProductSimpleObject>> getFBProducts()
        {
            List<ProductSimpleObject> ProductSimpleObjects = new List<ProductSimpleObject>();

            string query = @"SELECT product.num AS ProductNumber, product.description AS ProductDescription #, producttree.`name` AS ProductTree
                                FROM product
                                LEFT JOIN part ON(product.partid = part.id AND part.`typeId` IN(10, 30))
                                #LEFT JOIN producttotree ON product.id = producttotree.`productId`
	                            #    LEFT JOIN producttree ON producttree.`id` = producttotree.`productTreeId`
                                WHERE product.`activeFlag` = 1
                                ORDER BY product.num #producttree.name, product.num";

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ProductSimpleObjects.Add(new ProductSimpleObject
                        {
                            Number = (string)reader["ProductNumber"],
                            Description = (string)reader["ProductDescription"]
                        });
                    }
                }
            }

            if (ProductSimpleObjects?.Count == 0)
            {
                throw new KeyNotFoundException("No products found");
            }
            return ProductSimpleObjects;


        }

        /// <summary>
        /// Gets a simple Product Object from the provided Number or UPC code
        /// </summary>
        /// <param name="productNumOrUPC"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown when no records found</exception>
        /// <exception cref="ArgumentException">Thrown when more than one product returned</exception>
        public async Task<ProductSimpleObject> getProduct(string productNumOrUPC)
        {
            //await Load();

            ProductSimpleObject ProductSimpleObject = null;// = new ProductSimpleObject();

            string query = String.Format(@"SELECT product.id, product.num, product.description, product.`upc`, product.price
                                FROM product
                                WHERE Upper(product.num) LIKE '{0}' OR Upper(product.`upc`) LIKE '{0}'", productNumOrUPC.ToUpper());

            int rowCount= 0;
            using (var result = new MySqlCommand(query, Connection))
            {
                
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        rowCount++;

                        ProductSimpleObject = new ProductSimpleObject { 
                            Number = (string)reader["num"],
                            Description = (string)reader["description"],
                            UPC = (string)reader["upc"],
                            Price = (decimal)reader["price"],
                            Id = (int)reader["id"]
                            
                        };
                    }
                }
            }

            if (ProductSimpleObject == null)
            {
                throw new KeyNotFoundException(String.Format("{0} not found", productNumOrUPC));
            }
            else if (rowCount > 1)
            {
                throw new ArgumentException("More than one item returned. Check FB");
            }


            return ProductSimpleObject;


        }


        /// <summary>
        /// Get a list of active part from the fishbowl database. Only returns inventoried and non-inventoried part types
        /// </summary>
        /// <returns>List of PartSimple Objects</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no records are found</exception>
        public async Task<List<PartSimpleObject>> GetFBParts()
        {
            //await Load();

            List<PartSimpleObject> partSimples = new List<PartSimpleObject>();

            string query = @"SELECT part.num AS PartNumber, part.description AS PartDescription
	                            FROM part
	                            WHERE part.`activeFlag` = 1
	                            AND part.`typeId` IN(10, 30)
	                            ORDER BY part.num";

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        partSimples.Add(new PartSimpleObject
                        {
                            Number = (string)reader["PartNumber"],
                            Description = (string)reader["PartDescription"]
                        });
                    }
                }
            }

            if (partSimples?.Count == 0)
            {
                throw new KeyNotFoundException("No parts found");
            }
            return partSimples;


        }




        //     public static bool FBProductIsKit(string ProductNum)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             //DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"Select kitFlag from product where product.num like '" + ProductNum + @"'"
        //                        , connFB))
        //             {
        //                 //Debug.WriteLine(command.ExecuteScalar().GetType().ToString());
        //                 if ((UInt64)command.ExecuteScalar() == 1)
        //                 {
        //                     return true;
        //                 }
        //                 return false;
        //                 //FBDataA.SelectCommand = command;
        //                 //FBDataA.Fill(TempFBData);

        //             }
        //             //FBDataA.Dispose();
        //             //return TempFBData;

        //         }
        //     }

        //     public static List<SalesOrderItem> GetFBKitProductItems(string ProductNum, decimal qtyOrdered)
        //     {
        //         List<SalesOrderItem> KitItemList = new List<SalesOrderItem>();

        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT product.num AS sku, defaultQty AS qty_ordered, product.taxableFlag
        //                         FROM kititem 
        //                         JOIN product ON product.id = kititem.`ProductId`
        //                         JOIN product parentProduct ON Parentproduct.id = kititem.`kitProductId`
        //                         WHERE parentproduct.num LIKE '" + ProductNum + @"'"
        //                        , connFB))
        //             {

        //                 //return (bool)command.ExecuteScalar();
        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();

        //             foreach (DataRow item in TempFBData.Rows)
        //             {
        //                 SalesOrderItem SingleItem = new SalesOrderItem();
        //                 SingleItem.ProductNumber = item.Field<string>("sku");
        //                 SingleItem.Quantity = Decimal.Multiply(item.Field<decimal>("qty_ordered"), qtyOrdered).ToString();
        //                 SingleItem.ProductPrice = "0";
        //                 SingleItem.ItemType = "10";
        //                 SingleItem.NewItemFlag = "false";
        //                 SingleItem.UOMCode = "ea";
        //                 if (item.Field<UInt64>("taxableFlag") == 0)
        //                 {
        //                     SingleItem.Taxable = "false";
        //                 }
        //                 else
        //                 {
        //                     SingleItem.Taxable = "true";
        //                 }

        //                 SingleItem.KitItemFlag = "true";


        //                 KitItemList.Add(SingleItem);
        //             }


        //             return KitItemList;

        //         }

        //     }

        //     public static DataTable GetProductFBCategories(string ProductNumber)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //            //using this connection to lookup data
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();

        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT PRODUCT.ID, PRODUCT.NUM, CUSTOMSET.INFO AS CAT1, 
        //                         CUSTOMSET_1.INFO AS CAT2, CUSTOMSET_2.INFO AS CAT3, 
        //                         CUSTOMSET_3.INFO AS CAT4
        //                         FROM CUSTOMSET 
        //                         RIGHT JOIN PRODUCT ON CUSTOMSET.RECORDID = PRODUCT.ID AND CUSTOMSET.CUSTOMFIELDID = 63
        //                         LEFT JOIN CUSTOMSET AS CUSTOMSET_1 ON PRODUCT.ID = CUSTOMSET_1.RECORDID AND CUSTOMSET_1.CUSTOMFIELDID = 64
        //                         LEFT JOIN CUSTOMSET AS CUSTOMSET_2 ON PRODUCT.ID = CUSTOMSET_2.RECORDID AND CUSTOMSET_2.CUSTOMFIELDID = 65
        //                         LEFT JOIN CUSTOMSET AS CUSTOMSET_3 ON PRODUCT.ID = CUSTOMSET_3.RECORDID AND CUSTOMSET_3.CUSTOMFIELDID = 66
        //                         WHERE PRODUCT.NUM = '" + ProductNumber + "'" 
        //                        , connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             return TempFBData;

        //         }
        //     }

        //public static string GetProductDescription(string ProductNumber)
        //{
        //    using (MySqlConnection connFB = FishBowlConnection())
        //    {
        //        string description;

        //        connFB.Open();

        //        using (MySqlCommand command = new MySqlCommand(
        //                @"SELECT DESCRIPTION FROM PRODUCT WHERE NUM = '" + ProductNumber + "'"
        //                   , connFB))
        //        {
        //            description = (string)command.ExecuteScalar();

        //        }
        //        return description;

        //    }

        //}
        #endregion

        #region Shipping

        [Obsolete("Use the FB session call instead of direct to DB")]
        public async Task<List<ShipSimpleObject>> getShipSimpleList(ShipListFilters shipFilters = null, string searchTerm = null)
        {
            List<ShipSimpleObject> shipSimpleObjects = new List<ShipSimpleObject>();

            string WhereClause = "";
            string LimitClause = "";

            if (shipFilters != null)
            {

                if (shipFilters.Status != null)
                {
                    if (shipFilters.Status == ShipStatus.AllOpen)
                    {
                        WhereClause += " ship.StatusID in (10,20) ";
                    }
                    else
                    {
                        WhereClause += " ship.StatusID = " + (int)shipFilters.Status;
                    }

                    if (shipFilters.Status == ShipStatus.Shipped && String.IsNullOrEmpty(searchTerm))
                    {
                        //only load the first 50
                        LimitClause = " Limit 50";
                    }
                }

            }

            if (!String.IsNullOrEmpty(searchTerm))
            {
                WhereClause += " AND (UPPER(COALESCE(so.`billToName`, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name)) LIKE " +
                    "'%" + searchTerm.ToUpper() + "%' OR UPPER(ship.num) LIKE '%" + searchTerm.ToUpper() + "%')";

            }

            string query = @"SELECT ship.num AS ShipNum, 
	                            COALESCE(so.`billToName`, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name) AS OrderInfo, 
	                            carrier.`name` AS Carrier, 
	                            ship.`statusId` AS ShipStatusID,
	                            ship.`dateShipped` AS DateShipped,
	                            ship.`cartonCount` AS CartonCount
	
                            FROM ship
                            JOIN carrier ON ship.`carrierId` = carrier.id

                            LEFT JOIN so ON ship.`soId` = so.id
                            LEFT JOIN po ON ship.`poId` = po.`id`
                            LEFT JOIN xo ON ship.`xoId` = xo.`id`

                                LEFT JOIN locationgroup xoToLG ON xo.`shipToLGId` = xoToLG.`id`
                                LEFT JOIN locationgroup xoFromLG ON xo.`fromLGId` = xoFromLG.`id`
                                LEFT JOIN vendor ON vendor.id = po.`vendorId`

                            WHERE " + WhereClause +
                            @" ORDER BY COALESCE(so.`billToName`, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name), ship.num " +
                            LimitClause;

            using (var result = new MySqlCommand(query, Connection))
            {
                using (var reader = await result.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        shipSimpleObjects.Add(new ShipSimpleObject
                        {
                            ShipNum = reader["ShipNum"] == DBNull.Value ? null : reader["ShipNum"].ToString(),
                            OrderInfo = reader["OrderInfo"] == DBNull.Value ? null : reader["OrderInfo"].ToString(),
                            Carrier = reader["Carrier"] == DBNull.Value ? null : reader["Carrier"].ToString(),
                            ShipStatus = (ShipStatus)(int)reader["ShipStatusID"],
                            DateShipped = reader["DateShipped"] == DBNull.Value ? null : ((DateTime)reader["DateShipped"]).ToShortDateString(),
                            CartonCount = (Int32)reader["CartonCount"]

                        });
                    }
                }
            }

            return shipSimpleObjects;

        }

        /// <summary>
        /// Inserts an image path tied to a shipment into the FB database
        /// </summary>
        /// <param name="shipId">Shipment ID</param>
        /// <param name="imagePath">UNC Path to the image including file extension </param>
        /// <returns>The id of the newly created image</returns>
        public async Task<int> InsertShipmentImage(int shipId, string imagePath, int fileNumber)
        {
            MySqlTransaction insertTransaction = await Connection.BeginTransactionAsync();

            using (var insertTransactionCommand = new MySqlCommand(Connection, insertTransaction))
            {

                insertTransactionCommand.CommandText = string.Format(@"Insert into shipimages (path, recordid, filenumber) 
                                                        VALUES('{0}', '{1}', '{2}') ",
                                                        imagePath, shipId, fileNumber);

                try
                {
                    await insertTransactionCommand.ExecuteNonQueryAsync();


                    insertTransactionCommand.CommandText = "Select LAST_INSERT_ID()";

                    int newID = (int) (ulong)await insertTransactionCommand.ExecuteScalarAsync();

                    await insertTransaction.CommitAsync();

                    return newID;

                }
                catch (MySqlException e)
                {
                    await insertTransaction.RollbackAsync();
                    throw e;
                }
            }



        }

        public async Task DeleteShipmentImage(int imageId)
        {

            MySqlTransaction deleteTransaction = await Connection.BeginTransactionAsync();

            using (var deleteTransactionCommand = new MySqlCommand(Connection, deleteTransaction))
            {

                deleteTransactionCommand.CommandText = string.Format(@"Delete from shipimages where shipimages.id = {0}",
                                                        imageId);

                try
                {
                    await deleteTransactionCommand.ExecuteNonQueryAsync();
                    await deleteTransaction.CommitAsync();
                }
                catch (MySqlException e)
                {
                    await deleteTransaction.RollbackAsync();
                    throw e;
                }
            }

        }

        #endregion






        //     public static DataTable FBShipmentsFromOrderNum(string FBOrderNumber)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT so.id AS soid, ship.id AS shipid, billtoname, fb_soid, mage_orderid, carrier.name
        //                         FROM briteideasupdate.so
        //                         JOIN briteideasupdate.ship ON briteideasupdate.ship.soId = so.id
        //                         JOIN briteideasupdate.carrier on carrier.id = ship.carrierid
        //                         LEFT JOIN fb_link.shipments_fb_to_mage ON briteideasupdate.ship.id = fb_link.shipments_fb_to_mage.fb_shipid
        //                         WHERE so.num = " + FBOrderNumber + @"
        //                         AND fb_link.shipments_fb_to_mage.fb_shipid IS NULL
        //                        ", connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             return TempFBData;

        //         }
        //     }

        //     public static DataTable FBShipmentItems(int FbShipID)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT product.num AS sku, ship.id AS shipid, 
        //                             SUM(shipitem.qtyshipped) AS TotalShipped
        //                         FROM briteideasupdate.ship
        //                         JOIN briteideasupdate.shipitem ON shipitem.shipid = ship.id
        //                         JOIN briteideasupdate.product ON product.id = shipitem.itemId
        //                         JOIN briteideasupdate.shipcarton ON shipcarton.id = shipitem.shipCartonId
        //                     WHERE ship.id = " + FbShipID +
        //                     @" GROUP BY product.num"
        //                        , connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             return TempFBData;

        //         }
        //     }

        //     public static DataTable FBShipmentCartons(int FbShipID)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             //Returns the carton info
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT ship.id AS shipid, shipcarton.id AS shipcartonid, 
        //                             shipcarton.trackingNum, carrier.name AS carrier
        //                         FROM briteideasupdate.ship
        //                         JOIN briteideasupdate.carrier ON ship.carrierid = carrier.id
        //                         JOIN briteideasupdate.shipitem ON shipitem.shipid = ship.id
        //                         JOIN briteideasupdate.shipcarton ON shipcarton.id = shipitem.shipCartonId
        //                     WHERE ship.id = " + FbShipID
        //                        , connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             return TempFBData;

        //         }
        //     }


        //     public static void UpdateFB_link_shipmentsTable(int fbSoid, int fbShipId, int mageOrderId, int Mage_ShipID)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             string sqlStatement = @"INSERT INTO fb_link.shipments_fb_to_mage (fb_soid, fb_shipid, mage_orderid, mage_shipmentid) 
        //                                     SELECT " + fbSoid + " AS expr1, " +
        //                                                 fbShipId + " as expr2, " + 
        //                                                 mageOrderId + " as expr3, " + 
        //                                                 Mage_ShipID + " as expr4";

        //             using (MySqlCommand command = new MySqlCommand(
        //                     sqlStatement, connFB))
        //             {
        //                 command.ExecuteNonQuery();

        //             }


        //         }
        //     }
        //     #endregion





        //     //public static void ExportSOPI()
        //     public static bool CheckPOUsedBefore(string PONumber)
        //     {
        //         DataTable TempFBData = new DataTable();
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             //using this connection to lookup data
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();


        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT so.customerpo from so where UPPER(so.customerpo) LIKE '" + PONumber.ToUpper() + "'"
        //                        , connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             //return TempFBData;

        //         }


        //         if (TempFBData.Rows.Count > 0)
        //         {
        //             return true;
        //         }
        //         return false;

        //     }

        //     public static void ExportSOPI()
        //     {
        //         ExportSOPI(DateTime.Today);
        //     }

        //     public static void ExportSOPI(DateTime DateShipped)
        //     {
        //         //get customer IDs for customers that have Export SOPI checked
        //         string CustomerIDs = "";

        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT RECORDID FROM CUSTOMINTEGER WHERE CUSTOMFIELDID = 81"
        //                        , connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }

        //             foreach (DataRow row in TempFBData.Rows)
        //             {
        //                 //create "IN" string for following queries
        //                 CustomerIDs += row.Field<int>("RecordID").ToString() + ",";

        //             }
        //             //trim trailing comma
        //             //char[] TrimChars = {','};


        //             //now get S line , use shipID from S line to get remaining lines

        //             FBDataA = new MySqlDataAdapter();
        //             TempFBData = new DataTable();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT 'S' AS RowType, '856SOPI' AS TransactionType, Customer.name AS CustomerName, 
        //                         SO.num AS OrderNumber, Carrier.SCAC, '' AS Col6, Ship.BillofLading, 
        //                         DATE_FORMAT(ship.`dateShipped`,'%Y%m%d') AS ScheduledDelivery, 
        //                         DATE_FORMAT(ship.`dateShipped`,'%Y%m%d') AS ShipDate, 
        //                         so.shiptoname AS shiptoname, so.shiptoaddress, '' AS shiptoaddress2, 
        //                         so.shiptocity, stateconst.code AS statecode, so.shiptozip, so.shiptoname AS addresscode, 
        //                         '' AS Col17, '' AS Col18, '' AS Col19,
        //                         cartonweight.totalweight, 'Pounds' AS WeightUOM, Ship.cartoncount, '' AS Col23, '' AS Col24, 'Brite Ideas Decorating' AS ShipFromName, 
        //                         '2011 n 156th street' AS ShipFromAddress, '' AS ShipFromAddress2, 'Omaha' AS ShipfromCity, 'NE' AS shipFromState, 
        //                         '68116' AS ShipfromZip, '' AS Col31, '' AS Col32, '' AS Col33, '' AS Col34, '' AS Col35, '' AS Col36, '' AS Col37, '' AS Col38,
        //                         'CC' AS STATUS, '' AS Col40, (LPAD(EXTRACT(HOUR FROM ship.dateshipped), 2, '0') || ':' || LPAD(EXTRACT(MINUTE FROM ship.dateshipped), 2, '0') ) AS TimeShipped, Carrier.Name AS CARRIERNAME, 
        //                         so.customerid, ship.id AS SHIPID
        //                     FROM SHIP
        //                     INNER JOIN (SELECT ShipID, SUM(shipcarton.freightweight) AS totalweight FROM shipcarton GROUP BY ShipID) cartonweight ON cartonweight.shipid = ship.id
        //                     INNER JOIN carrier ON carrier.id = ship.carrierid
        //                     INNER JOIN SO ON so.id = ship.soid
        //                     INNER JOIN stateconst ON stateconst.id = so.shiptostateID
        //                     INNER JOIN customer ON so.customerid = customer.id
        //                     WHERE Customer.id IN(" + CustomerIDs.TrimEnd(new char[] { ',', ' ' }) + ") AND CAST(ship.dateshipped AS Date) = '" + DateTime.Today.ToString("yyyy-MM-dd") + "'"
        //                        , connFB))
        //             {
        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             using (StreamWriter sw = new StreamWriter(@"\\bi-accounting\acct\TrueCommerce\Import\EDISOPI.txt",false))
        //                 {
        //                     foreach (DataRow row in TempFBData.Rows)
        //                     {

        //                         sw.WriteLine(String.Join("\t", Array.ConvertAll(row.ItemArray, p => (p ?? String.Empty).ToString()), 0, 42));

        //                         //now get the o line to write
        //                         using (DataTable OLine = new DataTable())
        //                         {
        //                             MySqlDataAdapter OLineA = new MySqlDataAdapter();
        //                             using (MySqlCommand command = new MySqlCommand(
        //                                 @"SELECT 'O' as rowType, so.customerPO, 
        //                                 DATE_FORMAT(ship.`dateCreated`,'%Y%m%d') AS PODate, 
        //                                 '' AS col4,
        //                                 cartonweight.totalweight, 
        //                                 '' as col6, '' as col7, ship.cartoncount, ship.id as shipid
        //                                 From Ship
        //                                 Inner Join (Select ShipID, Sum(shipcarton.freightweight) as totalweight From shipcarton Group BY ShipID) cartonweight ON cartonweight.shipid = ship.id
        //                                 inner join so on so.id = ship.soid
        //                                 WHERE Ship.ID = " + row.Field<int>("shipID")
        //                                    , connFB))
        //                             {
        //                                 OLineA.SelectCommand = command;
        //                                 OLineA.Fill(OLine);
        //                                 OLineA.Dispose();
        //                             }
        //                             //write out oline
        //                             foreach (DataRow oRow in OLine.Rows)
        //                             {
        //                              sw.WriteLine(String.Join("\t", Array.ConvertAll(oRow.ItemArray, p => (p ?? String.Empty).ToString()), 0, 8));

        //                             }

        //                         }
        //                         //now write p line
        //                         using (DataTable pLine = new DataTable())
        //                         {
        //                             MySqlDataAdapter pLineA = new MySqlDataAdapter();
        //                             using (MySqlCommand command = new MySqlCommand(
        //                                 @"SELECT 'P' AS RowType, SHIPCARTON.SSCC, 
        //                                     shipitemsum.itemcount, '' AS Col4, '' AS Col5, '' AS Col6, 
        //                                     '' AS Col7, '' AS Col8, '' AS Col9, '' AS Col10, 
        //                                     '' AS Col11, '' AS Col12, 
        //                                     SHIPCARTON.TRACKINGNUM, SHIPCARTON.ID AS ShipcartonID, SHIPCARTON.SHIPID
        //                                     FROM SHIPCARTON 
        //                                     INNER JOIN (SELECT shipcartonid, count(ID) AS itemcount FROM SHIPITEM GROUP BY shipcartonid)  AS shipitemsum ON SHIPCARTON.id = shipitemsum.shipcartonid

        //                                     WHERE ShipCarton.ShipID = " + row.Field<int>("shipID")
        //                                    , connFB))
        //                             {
        //                                 pLineA.SelectCommand = command;
        //                                 pLineA.Fill(pLine);
        //                                 pLineA.Dispose();
        //                             }
        //                             //write out pline
        //                             foreach (DataRow pRow in pLine.Rows)
        //                             {
        //                                 sw.WriteLine(String.Join("\t", Array.ConvertAll(pRow.ItemArray, p => (p ?? String.Empty).ToString()), 0, 13));


        //                                 //write the P line then get and write all of the i line items in that package
        //                                 using (DataTable iLine = new DataTable())
        //                                 {
        //                                     MySqlDataAdapter iLineA = new MySqlDataAdapter();
        //                                     using (MySqlCommand command = new MySqlCommand(
        //                                         @"SELECT 'I' AS RowType, PRODUCT.NUM, 
        //                                         'null' AS Col3, PRODUCT.UPC, PRODUCT.DESCRIPTION, 
        //                                         SOITEM.QTYTOFULFILL, UOM.NAME, SHIPITEM.QTYSHIPPED, 
        //                                         'null' AS Col9, 'null' AS Col10, 'null' AS Col11, 
        //                                         SOITEM.SOLINEITEM, 'null' AS col13, 'null' AS col14, 
        //                                         'null' AS col15, SHIPITEM.SHIPID ,SHIPITEM.SHIPCARTONID
        //                                         FROM SHIPITEM 
        //                                         INNER JOIN (PRODUCT INNER JOIN (UOM INNER JOIN SOITEM ON UOM.ID = SOITEM.UOMID) ON PRODUCT.ID = SOITEM.PRODUCTID) ON SHIPITEM.soITEMID = SOITEM.ID

        //                                         WHERE ShipItem.shipcartonID = " + pRow.Field<int>("ShipcartonID")
        //                                            , connFB))
        //                                     {
        //                                         iLineA.SelectCommand = command;
        //                                         iLineA.Fill(iLine);
        //                                         iLineA.Dispose();
        //                                     }
        //                                     //write out iline
        //                                     foreach (DataRow iRow in iLine.Rows)
        //                                     {
        //                                         sw.WriteLine(String.Join("\t", Array.ConvertAll(iRow.ItemArray, p => (p ?? String.Empty).ToString()), 0, 15));

        //                                     }
        //                                 }
        //                         }
        //                         }

        //                         //now write all i lines


        //                     }
        //                     TempFBData.Dispose();
        //             }





        //         }
        //     }

        //     public static void UpdateSOEmailandPhone(string Email, string Phone, string OrderNum)
        //     {
        //         //DataTable TempFBData = new DataTable();
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             //using this connection to lookup data
        //            // MySqlDataAdapter FBDataA = new MySqlDataAdapter();


        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"UPDATE so
        //                         SET email = '" + Email + @"' , phone =  '" + Phone + @"' where so.num = '" + OrderNum + "'"
        //                        , connFB))
        //             {


        //                 command.ExecuteScalar();
        //                 //FBDataA.Fill(TempFBData);

        //             }
        //             //FBDataA.Dispose();
        //             //return TempFBData;

        //         }


        //     }

        //     public static DataTable Get846ProductInventorybyBuyer(int CFSellsOnID)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT PRODUCT.NUM AS VENDORNUM, Product.Description, 
        //                   COALESCE(IF(SUM((QtyONHand-QTYAllocated-QTYnotAvailable-COALESCE(locked.qtyLockedinDisplay,0))/COALESCE(Factor,1))<0,0,SUM((QtyONHand-QTYAllocated-QTYnotAvailable-COALESCE(locked.qtyLockedinDisplay,0))/COALESCE(Factor,1))),0) AS AFS,
        //                         COALESCE(SUM(QTYONORDER),0) AS QTYONORDER
        //                     FROM Product
        //                     LEFT JOIN (SELECT PART.ID, UOMCONVERSION.FROMUOMID, PRODUCT.ID AS ProductID, UOMCONVERSION.TOUOMID, UOMCONVERSION.MULTIPLY, UOMCONVERSION.FACTOR
        //                             FROM (UOMCONVERSION INNER JOIN PART ON UOMCONVERSION.FROMUOMID = PART.UOMID)
        //                             INNER JOIN PRODUCT ON (UOMCONVERSION.TOUOMID = PRODUCT.UOMID) AND (PART.ID = PRODUCT.PARTID)) PartToProdUOMConv
        //                             ON Product.ID = PartToProdUOMConv.ProductID
        //                     LEFT JOIN QTYINVENTORYTOTALS ON PRODUCT.PARTID = QTYINVENTORYTOTALS.PARTID

        //                     LEFT JOIN (SELECT partid, qty AS qtyLockedinDisplay 
        //	                FROM qohview 
        //	                JOIN part ON qohview.`PARTID` = part.`id`
        //	                WHERE locationid = 839) locked ON product.`partId` = locked.partid

        //                     JOIN (SELECT RecordID, INFO FROM CustomInteger WHERE CustomInteger.CUSTOMFIELDID = 83) ExportInv ON ExportInv.RecordID = Product.ID
        //                     JOIN (SELECT RecordID, INFO FROM CustomInteger WHERE CustomInteger.CUSTOMFIELDID = " + CFSellsOnID + @") SellOn on SellOn.RecordID = Product.ID
        //                     GROUP BY PRODUCT.NUM, Product.description
        //                        ", connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();
        //             return TempFBData;
        //         }




        //     }

        //     public static int[] GetFBProductPartIDNumber(string ProductNumber)
        //     {
        //         using (MySqlConnection connFB = FishBowlConnection())
        //         {
        //             MySqlDataAdapter FBDataA = new MySqlDataAdapter();
        //             DataTable TempFBData = new DataTable();
        //             //Using this connection to lookup data
        //             connFB.Open();

        //             int[] TempID = new int[2];

        //             using (MySqlCommand command = new MySqlCommand(
        //                     @"SELECT ID as ProductID, PartID
        //                         FROM PRODUCT
        //                         Where num = '" + ProductNumber + "' AND KitFlag = 0"
        //                        , connFB))
        //             {


        //                 FBDataA.SelectCommand = command;
        //                 FBDataA.Fill(TempFBData);

        //             }
        //             FBDataA.Dispose();

        //             if (TempFBData.Rows.Count > 0)
        //             {
        //                 TempID[0] = TempFBData.Rows[0].Field<int>("ProductID");
        //                 TempID[1] = TempFBData.Rows[0].Field<int>("PartID");
        //                 TempFBData.Dispose();
        //                 return TempID;
        //             }
        //             else
        //             {
        //                 TempFBData.Dispose();
        //                 return null;
        //             }


        //         }
        //     }


    }
}
