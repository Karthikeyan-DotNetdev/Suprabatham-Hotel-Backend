using CommonModels;
using CommonServices;
using laptop_service.Models.MASTERS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace laptop_service.Controllers.MASTERS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/barcodeupdate")]
    [ApiController]
    public class BarcodeUpdate_Controller : ControllerBase
    {
        private static string EscapeSql(string? value) =>
            (value ?? "").Trim().Replace("'", "''");

        [HttpGet]
        [Route("GetBarcodeList")]
        public IActionResult GetBarcodeList()
        {
            try
            {
                DataTable dt = SQLService.GetDataTable(@"
                    SELECT 
                        P.Product_Code,
                        P.Product_Name,
                        ISNULL(P.Short_Name, '') AS Short_Name,
                        ISNULL(P.Barcode, '') AS Barcode,
                        ISNULL(P.Short_Name, '') AS Base_Short_Name,
                        ISNULL(P.Barcode, '') AS Base_Barcode,
                        P.Category_Code,
                        ISNULL(C.Category_Name, '') AS Category_Name,
                        P.Product_Group_Code,
                        ISNULL(PG.Product_Group_Name, '') AS Product_Group_Name,
                        ISNULL(P.Selling_Price, 0) AS Selling_Price,
                        ISNULL(P.Is_Active, 'A') AS Is_Active
                    FROM dbo.M_ProductMaster P WITH (NOLOCK)
                    LEFT JOIN dbo.M_CategoryMaster C WITH (NOLOCK) ON P.Category_Code = C.Category_Code
                    LEFT JOIN dbo.M_Product_Group PG WITH (NOLOCK) ON P.Product_Group_Code = PG.Product_Group_Code
                    WHERE P.Is_Active = 'A'
                    ORDER BY P.Product_Name ASC");

                return new JsonResult(new ApiResponse
                {
                    status = true,
                    data = UtilityService.DataTableToJArray(dt)
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("BulkUpdate")]
        public IActionResult BulkUpdate([FromBody] BulkBarcodeUpdateRequest request)
        {
            try
            {
                if (request?.Items == null || request.Items.Count == 0)
                    return Ok(new { status = false, message = "No items provided." });

                string itemsJson = JsonConvert.SerializeObject(request.Items);
                string safeJson = itemsJson.Replace("'", "''");
                string updatedBy = EscapeSql(request.UpdatedBy ?? "admin");

                DataTable dt = SQLService.GetDataTable($@"
                    EXEC SP_BulkUpdateProductBarcodeShortName
                        @ItemsJson = N'{safeJson}',
                        @UpdatedBy = '{updatedBy}'");

                int rowsAffected = dt?.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Rows_Affected"]) : request.Items.Count;

                return Ok(new
                {
                    status = true,
                    message = $"{rowsAffected} product(s) updated successfully.",
                    rows_affected = rowsAffected
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
    }
}
