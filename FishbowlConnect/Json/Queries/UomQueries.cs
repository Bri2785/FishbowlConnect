using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.CsvClassMaps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUomId"></param>
        /// <param name="toUomId"></param>
        /// <returns>Single matching UomConversion</returns>
        /// <exception cref="KeyNotFoundException">Throw when no records found</exception>
        public async Task<UOMConversion> GetUOMConversion(int fromUomId, int toUomId)
        {
            string query = string.Format(@"SELECT uomconversion.*
                                                , uom.`integral`
                                                , uom.`code` AS toUomCode
                                            FROM uomconversion
                                            JOIN uom ON uom.id = uomconversion.`toUomId`
                                            WHERE uomconversion.`fromUomId` = {0}
                                                AND uomconversion.`toUomId` = {1}", fromUomId, toUomId);


            return (await ExecuteQueryAsync<UOMConversion, UomConversionClassMap>(query))[0]; //should only be 1. If none, error is thrown
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toUomId"></param>
        /// <returns>List of all conversion going to the provided Uom id. Match your from to the fromUomid field to get the correct factors</returns>
        /// <exception cref="KeyNotFoundException">Throw when no records found</exception>
        public async Task<List<UOMConversion>> GetUOMConversions(int toUomId)
        {
            string query = string.Format(@"SELECT uomconversion.*
                                                , uom.`integral`
                                                , uom.`code` AS toUomCode
                                            FROM uomconversion
                                            JOIN uom ON uom.id = uomconversion.`toUomId`
                                            WHERE uomconversion.`toUomId` = {0}"
                                                , toUomId);


            return (await ExecuteQueryAsync<UOMConversion, UomConversionClassMap>(query)); //should only be 1. If none, error is thrown
        }

    }
}
