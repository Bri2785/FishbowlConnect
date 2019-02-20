using FishbowlConnect.Json.APIObjects;
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
        /// <summary>
        /// Gets a list of the picks from FB that match the specified filters
        /// </summary>
        /// <param name="PickFilters"><see cref="PickListFilters"/></param>
        /// <returns>List of Simple Pick Objects</returns>
        /// <exception cref="KeyNotFoundException">Throws when no records returned</exception>
        public async Task<List<PickSimpleObject>> GetPickSimpleList(PickListFilters PickFilters = null, string searchTerm = null)
        {
            List<PickSimpleObject> PickList = new List<PickSimpleObject>();

            //we need a page variable to keep track of how many we are loading at a time
            //i.e. page 1 loads records 1-15
            //page 2 loads 16-30 and adds them to the existing collection


            string WhereClause = "";
            string LimitClause = "";

            if (PickFilters != null)
            {
                if (PickFilters.CompletelyFulfillable)
                {
                    WhereClause += " AND ItemsNotFulfillable Is null";
                }
                if (!string.IsNullOrEmpty(PickFilters.LocationGroupName))
                {
                    WhereClause += " AND LocationGroup.Name = '" + PickFilters.LocationGroupName + "'";
                }
                //if (PickFilters.Status != null)
                //{
                if (PickFilters.Status == PickStatus.AllOpen)
                {
                    WhereClause += " AND Pick.StatusID in (10,20,30) ";
                }
                else if (PickFilters.Status == PickStatus.All   )
                {
                    WhereClause += " AND Pick.StatusID in (10,20,30,40) ";
                }
                else
                {
                    WhereClause += " AND Pick.StatusID = " + (int)PickFilters.Status;
                }

                if ((PickFilters.Status == PickStatus.Finished || PickFilters.Status == PickStatus.All) && String.IsNullOrEmpty(searchTerm))
                {
                    //only load the first 50
                    LimitClause = " Limit 50";
                }

                //}
                if (!string.IsNullOrEmpty(PickFilters.Username))
                {
                    WhereClause += " AND SysUser.UserName = '" + PickFilters.Username + "'";
                }
            }


            if (!String.IsNullOrEmpty(searchTerm))
            {
                WhereClause += " AND (UPPER(COALESCE(so.CUSTOMERCONTACT, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name)) LIKE " +
                    "'%" + searchTerm.ToUpper() + "%' OR UPPER(pick.num) LIKE '%" + searchTerm.ToUpper() + "%')";

            }

            string query = @"select pick.id, pick.num, pick.StatusID, pick.DateScheduled, Pick.locationgroupid, 
                                Pick.priority, SysUser.username, 
                                COALESCE(so.CUSTOMERCONTACT, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name) AS CUSTOMERCONTACT,
                                Locationgroup.Name as LGName, 
                                Coalesce(ItemsNotFulfillable, 0) as ItemsNotFulfillable , ItemCount.NumberOfItems
                            from pick
                            join sysuser on pick.userID = sysuser.id
                            LEFT JOIN so ON (
				                CASE WHEN INSTR(Pick.num,'-') > 0 THEN
					                SUBSTRING(pick.num FROM 1 FOR (INSTR(Pick.num,'-')-1))
				                ELSE Pick.num 
				                END  = CONCAT('S',SO.NUM))
				
			                    LEFT JOIN xo ON (
				                CASE WHEN INSTR(Pick.num,'-') > 0 THEN
					                SUBSTRING(pick.num FROM 1 FOR (INSTR(Pick.num,'-')-1))
				                ELSE Pick.num 
				                END  = CONCAT('T',xo.NUM))
				
			                    LEFT JOIN po ON (
				                CASE WHEN INSTR(Pick.num,'-') > 0 THEN
					                SUBSTRING(pick.num FROM 1 FOR (INSTR(Pick.num,'-')-1))
				                ELSE Pick.num
				                END  = CONCAT('P',po.NUM))	

                                LEFT JOIN locationgroup xoToLG ON xo.`shipToLGId` = xoToLG.`id`
			                    LEFT JOIN locationgroup xoFromLG ON xo.`fromLGId` = xoFromLG.`id`
			                    LEFT JOIN vendor ON vendor.id = po.`vendorId`

                            join Locationgroup on (pick.locationgroupid = locationgroup.id)
                            Left Join (select PickItem.PICKID, Count(StatusID) as ItemsNotFulfillable
                                    from pickitem
                                    where StatusID = 5
                                    Group By Pickitem.PickID) INF on Pick.ID = INF.pickid
                            Join (select PickID, Count(*) as NumberOfItems
                                    from pickitem
                                    Group By Pickitem.PickID) ItemCount on Pick.ID = ItemCount.pickid 
                            Where " +
                            WhereClause.Substring(4) +
                            " Order By pick.dateCreated, pick.num" +
                            LimitClause; //TODO: if all filters are null then remove the where from the query

            return await ExecuteQueryAsync<PickSimpleObject, PickSimpleObjectClassMap>(query);




        }



    }
}
