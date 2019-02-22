using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Json.QueryClasses;
using FishbowlConnect.Logging;
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
        /// Gets a list of the picks from FB that match the specified filters
        /// </summary>
        /// <param name="PickFilters"><see cref="PickListFilters"/></param>
        /// <returns>List of Simple Pick Objects</returns>
        /// <exception cref="KeyNotFoundException">Throws when no records returned</exception>
        public async Task<List<PickSimpleObject>> GetPickSimpleList(PickListFilters PickFilters = null, 
            string searchTerm = null, CancellationToken cancellationToken = default(CancellationToken))
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
                else if (PickFilters.Status == PickStatus.All)
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
                    WhereClause += " AND UPPER(SysUser.UserName) = '" + PickFilters.Username.ToUpper() + "'";
                }
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                WhereClause += " AND (UPPER(COALESCE(so.CUSTOMERCONTACT, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name)) LIKE " +
                    "'%" + searchTerm.ToUpper() + "%' OR UPPER(pick.num) LIKE '%" + searchTerm.ToUpper() + "%')";

            }

            string query = string.Format(@"select Pick.ID as PickID
		            , pick.num AS PickNumber
		            , pick.StatusID as PickStatusID
		            , pick.DateScheduled
		            , Pick.locationgroupid
		            , Pick.priority
		            , SysUser.username
		            , COALESCE(so.CUSTOMERCONTACT, CONCAT(xoFromLG.name,' -> ', xoToLG.name), vendor.name) AS OrderInfo
		            , Locationgroup.Name AS LGName
		            , COALESCE(ItemsNotFulfillable, 0) AS ItemsNotFulfillable 
		            , ItemCount.NumberOfItems
                                
                            from pick
                            join pickitem on pickitem.`pickId` = pick.id
                            
                            join sysuser on pick.userID = sysuser.id
                            
                            LEFT JOIN so ON (pickitem.`orderId` = so.id and pickitem.`orderTypeId` = 20)
		
			    LEFT JOIN xo ON (pickitem.`orderId` = xo.id and pickitem.`orderTypeId` = 40)
		
			    LEFT JOIN po ON (pickitem.`orderId` = po.id and pickitem.`orderTypeId` = 10)
			    
			    Left Join wo on (pickitem.`orderId` = wo.id and pickitem.`orderTypeId` = 30)	

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
                            
                            Where  {0} 
                            
                            
                            GROUP BY 1,2,3,4,5,6,7,8,9,10,11
                            Order By pick.dateScheduled DESC, pick.num 
		                    {1}",
                            WhereClause.Substring(4), LimitClause); 

            
            return await ExecuteQueryAsync<PickSimpleObject, PickSimpleObjectClassMap>(query, cancellationToken);




        }



    }
}
