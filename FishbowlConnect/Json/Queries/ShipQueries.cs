using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Json.QueryClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Returns a simple list of shipments matching provided filters
        /// </summary>
        /// <param name="shipFilters"></param>
        /// <param name="searchTerm"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<ShipSimpleObject>> getShipSimpleList(ShipListFilters shipFilters = null, 
            string searchTerm = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            //List<ShipSimpleObject> shipSimpleObjects = new List<ShipSimpleObject>();

            string WhereClause = "";
            string LimitClause = "";

            if (shipFilters != null)
            {

                //if (shipFilters.Status != null)
                //{
                if (shipFilters.Status == ShipStatus.AllOpen)
                {
                    WhereClause += " ship.StatusID in (10,20) ";
                }
                else if (shipFilters.Status == ShipStatus.All)
                {
                    WhereClause += " ship.StatusID in (10, 20, 30, 40) ";
                }
                else
                {
                    WhereClause += " ship.StatusID = " + (int)shipFilters.Status;
                }

                if ((shipFilters.Status == ShipStatus.Shipped || shipFilters.Status == ShipStatus.All) && String.IsNullOrEmpty(searchTerm))
                {
                    //only load the first 50
                    LimitClause = " Limit 50";
                }
                //}

            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                WhereClause += " AND (UPPER(COALESCE(so.`billToName`, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name)) LIKE " +
                    "'%" + searchTerm.ToUpper() + "%' OR UPPER(ship.num) LIKE '%" + searchTerm.ToUpper() + "%')";

            }

            string query = @"SELECT ship.id as ShipId, ship.num AS ShipNum, 
	                            COALESCE(so.`billToName`, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name) AS OrderInfo, 
                                COALESCE(so.`customerPO`, po.`customerSO`) AS PONumber,
	                            carrier.`name` AS Carrier, 
	                            ship.`statusId` AS ShipStatusID,
	                            ship.`dateShipped` AS DateShipped,
	                            ship.`cartonCount` AS CartonCount,
                                ship.shipToId AS CustomerId
	
                            FROM ship
                            JOIN carrier ON ship.`carrierId` = carrier.id

                            LEFT JOIN so ON ship.`soId` = so.id
                            LEFT JOIN po ON ship.`poId` = po.`id`
                            LEFT JOIN xo ON ship.`xoId` = xo.`id`

                                LEFT JOIN locationgroup xoToLG ON xo.`shipToLGId` = xoToLG.`id`
                                LEFT JOIN locationgroup xoFromLG ON xo.`fromLGId` = xoFromLG.`id`
                                LEFT JOIN vendor ON vendor.id = po.`vendorId`

                            WHERE " + WhereClause +
                            @" ORDER BY COALESCE(so.`billToName`, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name), ship.id " +
                            LimitClause;

            return await ExecuteQueryAsync<ShipSimpleObject, ShipSimpleObjectClassMap>(query, cancellationToken);

        }

        /// <summary>
        /// Returns list of base64 shipment images from the database
        /// Will throw error if table doesn't exist
        /// </summary>
        /// <param name="shipId">shipment id number</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<FishbowlImage>> GetShipmentImageList(int shipId, string tableName,
            CancellationToken cancellationToken = default)
        {
            string query = string.Format(@"select
                                  `id`,
                                  `imageFull`,
                                  `recordid`,
                                    tableName,
                                    type
                                from
                                  `imageapi`
                                where recordid = {0}
                                AND tableName = '{1}'", shipId, tableName);

            return await ExecuteQueryAsync<FishbowlImage, ImageClassMap>(query, cancellationToken);
        }

        /// <summary>
        /// Returns list of base64 shipment images from the database
        /// Will throw error if table doesn't exist
        /// </summary>
        /// <param name="shipNum">Shipment Number</param>
        /// <param name="tableName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<FishbowlImage>> GetShipmentImageList(string shipNum, string tableName,
            CancellationToken cancellationToken = default)
        {
            string query = string.Format(@"select
                                  `id`,
                                  `imageFull`,
                                  `recordid`,
                                    tableName,
                                    type
                                from
                                  `reelimages`
                                join ship on ship.id = reelimages.recordid
                                where ship.num = '{0}'
                                AND reelimages.tableName = '{1}'", shipNum, tableName);

            return await ExecuteQueryAsync<FishbowlImage, ImageClassMap>(query, cancellationToken);
        }


    }
}
