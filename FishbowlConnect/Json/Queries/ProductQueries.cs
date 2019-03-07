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
        public async Task<List<PartToProductUomConversion>> GetProductUomConverisons(string PartNumber)
        {
            string query = string.Format(@"Select part.`num` as PartNumber
	                                    , product.`num` as ProductNumber
	                                    , product.`upc`
	                                    , Coalesce(uomconversion.`factor`,1) as factor
	                                    , Coalesce(uomconversion.`multiply`,1) as multiply
	                                    , uom.`code` as ProductUomCode
	
                                    from part
                                    join product on product.partid = part.`id`
                                    left join uomconversion on uomconversion.`fromUomId` = part.`uomId` and uomconversion.`toUomId` = product.`uomId`
                                    left join uom on uomconversion.`toUomId` = uom.`id`

                                    where part.num = '{0}'", PartNumber.Replace("'","''")); //escape quotes

            return await ExecuteQueryAsync<PartToProductUomConversion, PartToProductUomConverisonClassMap>(query);

            
        }
    }
}
