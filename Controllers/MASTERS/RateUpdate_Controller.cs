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
    [Route("api/rateupdate")]
    [ApiController]
    public class RateUpdate_Controller : ControllerBase
    {
        private static string EscapeSql(string? value) =>
            (value ?? "").Trim().Replace("'", "''");

        [HttpGet]
        [Route("GetRateList")]
        public IActionResult GetRateList([FromQuery] string branchCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(branchCode))
                    return Ok(new { status = false, message = "Branch Code is required." });

                DataTable dt = SQLService.GetDataTable($@"
                    EXEC SP_GetRateUpdateList
                        @BranchCode = '{EscapeSql(branchCode)}'");

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
        public IActionResult BulkUpdate([FromBody] BulkRateUpdateRequest request)
        {
            try
            {
                if (request?.Rates == null || request.Rates.Count == 0)
                    return Ok(new { status = false, message = "No rate items provided." });

                if (string.IsNullOrWhiteSpace(request.UpdatedBy))
                    return Ok(new { status = false, message = "UpdatedBy is required." });

                var validAreas = new[] { "ALL", "DINE_IN", "TAKEAWAY", "DELIVERY" };
                foreach (var r in request.Rates)
                {
                    if (string.IsNullOrWhiteSpace(r.Branch_Code) || string.IsNullOrWhiteSpace(r.Product_Code))
                        return Ok(new { status = false, message = "Branch_Code or Product_Code missing in one or more items." });
                    if (r.Selling_Price < 0)
                        return Ok(new { status = false, message = $"Selling_Price cannot be negative for {r.Product_Code}." });
                    var aType = (r.Area_Type ?? "ALL").ToUpper().Trim();
                    if (!Array.Exists(validAreas, a => a == aType))
                        return Ok(new { status = false, message = $"Invalid Area_Type '{r.Area_Type}' for {r.Product_Code}." });
                    r.Area_Type = aType;
                }

                string ratesJson = JsonConvert.SerializeObject(request.Rates);
                string safeJson = ratesJson.Replace("'", "''");

                DataTable dt = SQLService.GetDataTable($@"
                    EXEC SP_BulkUpsertBranchRates
                        @RatesJson = N'{safeJson}',
                        @UpdatedBy = '{EscapeSql(request.UpdatedBy)}'");

                int rowsAffected = dt?.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Rows_Affected"]) : 0;
                return Ok(new
                {
                    status = true,
                    message = $"{rowsAffected} rate(s) saved successfully.",
                    rows_affected = rowsAffected
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("ResetToBase")]
        public IActionResult ResetToBase([FromBody] ResetRateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Branch_Code))
                    return Ok(new { status = false, message = "Branch_Code is required." });

                if (request.Product_Codes == null || request.Product_Codes.Count == 0)
                    return Ok(new { status = false, message = "No product codes provided." });

                string codesJson = JsonConvert.SerializeObject(request.Product_Codes);
                string safeJson = codesJson.Replace("'", "''");
                string areaType = (request.Area_Type ?? "ALL_AREAS").ToUpper().Trim();

                DataTable dt = SQLService.GetDataTable($@"
                    EXEC SP_ResetBranchRatesToBase
                        @BranchCode   = '{EscapeSql(request.Branch_Code)}',
                        @ProductCodes = N'{safeJson}',
                        @AreaType     = '{EscapeSql(areaType)}',
                        @UpdatedBy    = '{EscapeSql(request.Updated_By)}'");

                int resetCount = dt?.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Reset_Count"]) : 0;
                return Ok(new { status = true, message = $"{resetCount} rate(s) reset to base price." });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
    }
}
