using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Json.QueryClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Returns a list of Active SimpleLocation Object from the locationgroup id provided. 
        /// </summary>
        /// <param name="locationGroupId">FB location group ID</param>
        /// <returns>List of Location Simple Object</returns>
        /// <exception cref="KeyNotFoundException">Throws KeyNotFound when no records returned</exception>
        public async Task<List<LocationSimpleObject>> GetLocationSimpleList(int locationGroupId)
        {
            string query = String.Format(@"Select location.`id` as locationId, 
                                    location.`name` as locationName, 
                                    locationgroup.`name` as LGName, 
                                    locationgroup.`id` as locationGroupId,
                                    tag.`num` AS LocationTagNum
                            FROM location
                            JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`
                            JOIN tag ON tag.`locationId` = location.id AND tag.`typeId` = 10
                            where location.`activeFlag` = 1
                            and location.`locationGroupId` = {0}
                            order by location.name", locationGroupId);


            return await ExecuteQueryAsync<LocationSimpleObject, LocationSimpleObjectClassMap>(query);
        }

        /// <summary>
        /// Returns a list of Active SimpleLocation Object from the locationgroup name provided.
        /// </summary>
        /// <param name="LocationGroupName"></param>
        /// <returns>List of Location Simple Object</returns>
        /// <exception cref="KeyNotFoundException">Throws KeyNotFound when no records returned</exception>
        public async Task<List<LocationSimpleObject>> GetLocationSimpleList(string LocationGroupName)
        {
            string query = String.Format(@"Select location.`id` as locationId, 
                                    location.`name` as locationName, 
                                    locationgroup.`name` as LGName, 
                                    locationgroup.`id` as locationGroupId,
                                    tag.`num` AS LocationTagNum
                            FROM location
                            JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`
                            JOIN tag ON tag.`locationId` = location.id AND tag.`typeId` = 10
                            where location.`activeFlag` = 1
                            and locationgroup.`name` = '{0}'
                            order by location.name", LocationGroupName);


            return await ExecuteQueryAsync<LocationSimpleObject, LocationSimpleObjectClassMap>(query);
        }

        /// <summary>
        /// Gets a single location simple object from the provided location group and location name
        /// </summary>
        /// <param name="LocationGroupName"></param>
        /// <param name="LocationName"></param>
        /// <returns>LocationSimpleObject</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no location found</exception>
        /// <exception cref="ArgumentException">Thrown when more than one location found matching the criteria</exception>
        public async Task<LocationSimpleObject> GetLocationSimple(string LocationGroupName, string LocationName)
        {
            string query = String.Format(@"Select location.`id` as locationId, 
                                    location.`name` as locationName, 
                                    locationgroup.`name` as LGName, 
                                    locationgroup.`id` as locationGroupId,
                                    tag.`num` AS LocationTagNum
                            FROM location
                            JOIN locationgroup ON location.`locationGroupId` = locationgroup.`id`
                            JOIN tag ON tag.`locationId` = location.id AND tag.`typeId` = 10
                            where location.`activeFlag` = 1
                            and locationgroup.`name` = '{0}'
                            AND location.name = '{1}'
                            order by location.name", LocationGroupName,LocationName);


            List<LocationSimpleObject> locations = await ExecuteQueryAsync<LocationSimpleObject, LocationSimpleObjectClassMap>(query);
            if (locations.Count > 1)
            {
                throw new ArgumentException("Multiple locations returned, Check parameters");
            }
            return locations.FirstOrDefault();
        }

        /// <summary>
        /// Gets full list of ACTIVE location groups from Fishbowl
        /// </summary>
        /// <returns>LocationGroupSimpleObject</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no location groups are found</exception>
        public async Task<List<LocationGroupSimpleObject>> GetLocationGroupList()
        {
            string query = @"Select locationgroup.`id` as locationGroupId,
                                    locationgroup.`name` as LocationGroupName
                                    
                            from locationgroup 
                            where locationgroup.`activeFlag` = 1
                            order by locationgroup.name";


            return await ExecuteQueryAsync<LocationGroupSimpleObject, LocationGroupSimpleObjectClassMap>(query);
        }


    }
}
