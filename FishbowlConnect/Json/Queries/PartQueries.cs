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

        public async Task<string> GetPartLastCost(string partNumber)
        {
            string query = String.Format(@"SELECT LastDate.unitcost, part.id
                                FROM part
                                JOIN (SELECT costlayer.`orgTotalCost`/costlayer.`orgQty` AS unitcost, costlayer.partid, costlayer.`dateCreated` AS LastIn
                                FROM costlayer 
                                LEFT JOIN costlayer c ON (c.`partId` = costlayer.`partId` AND costlayer.`dateCreated` < c.`dateCreated`)
                                WHERE c.`dateCreated` IS NULL) LastDate ON LastDate.partid = part.id 
                            WHERE Upper(part.num) = '{0}'", partNumber.ToUpper());

            return await ExecuteQueryAsync(query);

        }

        /// <summary>
        /// Returns a List of PartNumAndTracks objects that hold the part number and all of the currently used tracking fields
        /// Used in cases where the tag number is not known (adding inventory)
        /// </summary>
        /// <param name="partNumber">Can be part number or UPC code</param>
        /// <returns></returns>
        public async Task<List<PartNumAndTracks>> GetPartNumberAndTrackingFields(string partNumber)
        {
            string query = string.Format(@"SELECT part.num as PartNumber
	                            , parttracking.id as TrackingID
	                            , parttracking.`abbr` as TrackingAbbr
	                            , parttracking.`name` as TrackingLabel
	                            , parttracking.`sortOrder` as TrackingSortOrder
	                            , parttracking.`typeId` as TrackingTypeID
	                            , parttotracking.`primaryFlag` as TrackingPrimaryFlag

                            FROM part 

                            LEFT JOIN parttotracking ON (parttotracking.`partId` = part.id)
                            LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                            WHERE (UPPER(part.`num`) LIKE '{0}' OR part.`upc` LIKE '{0}' )
                            Order BY parttracking.sortorder", partNumber.ToUpper());

            return await ExecuteQueryAsync<PartNumAndTracks, PartNumAndTracksClassMap>(query);
        }

        /// <summary>
        /// Gets the part number and description a part with the provided product number. Does not do wild card searched, product number must match
        /// </summary>
        /// <param name="ProductNumber"></param>
        /// <returns>PartSimpleObject</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no record matches</exception>
        public async Task<PartSimpleObject> GetSimplePart(string ProductNumber)
        {
            string query = string.Format(@"SELECT part.num AS Number, part.description AS Description
	                            FROM part
                                JOIN product on product.partid = part.id
	                            WHERE part.`activeFlag` = 1
	                            AND UPPER(product.num) like '{0}'", ProductNumber);

            return (await ExecuteQueryAsync<PartSimpleObject, PartSimpleObjectClassMap>(query))[0];
        }

    }
}
