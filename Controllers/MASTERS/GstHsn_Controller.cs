using CommonModels;
using CommonServices;
using laptop_service.Models.MASTERS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using WIN_BOT;

namespace laptop_service.Controllers.MASTERS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/hsngst")]
    [ApiController]
    public class HSN_GST_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddHSNGST")]
        public async Task<IActionResult> AddHSNGST(HSN_GST_MASTER hsn)
        {
            try
            {
                JObject jobject = new JObject();

                hsn.Hsn_Name = hsn.Hsn_Name?.Trim();
                hsn.GST_Code = hsn.GST_Code?.Trim();
                hsn.GST_Name = hsn.GST_Name?.Trim();

                if (string.IsNullOrWhiteSpace(hsn.Hsn_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "HSN Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
SELECT COUNT(*)
FROM HSN_GST_Master
WHERE Is_Active='A'
AND (
    UPPER(Hsn_Name)=UPPER('{hsn.Hsn_Name}')
    OR UPPER(GST_Name)=UPPER('{hsn.GST_Name}')
)";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "HSN Name or GST Name already exists.");
                    return Ok(jobject.ToString());
                }
                if (string.IsNullOrWhiteSpace(hsn.GST_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "GST Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                hsn = await LoadHSNAndGSTMaxCode(hsn);

                var data = new HSN_GST_MASTER
                {
                    Hsn_Code = hsn.Hsn_Code,
                    Hsn_Name = hsn.Hsn_Name.ToUpper(),
                    GST_Code = hsn.GST_Code,
                    GST_Name = hsn.GST_Name,
                    Is_Active = hsn.Is_Active ?? "A",
                    Created_By = hsn.Created_By,
                    Created_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>()
                {
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    data,
                    "HSN_GST_Master",
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Saved Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Save");
                }

                return Ok(jobject.ToString());
            }
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        [HttpPost]
        [Route("UpdateHSNGST/{Hsn_Code}")]
        public IActionResult UpdateHSNGST(string Hsn_Code, HSN_GST_MASTER hsn)
        {
            try
            {
                JObject jobject = new JObject();

                hsn.Hsn_Name = hsn.Hsn_Name?.Trim();
                hsn.GST_Code = hsn.GST_Code?.Trim();
                hsn.GST_Name = hsn.GST_Name?.Trim();

                if (string.IsNullOrWhiteSpace(hsn.Hsn_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "HSN Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
SELECT COUNT(*)
FROM HSN_GST_Master
WHERE Hsn_Code<>'{Hsn_Code}'
AND Is_Active='A'
AND (
    UPPER(Hsn_Name)=UPPER('{hsn.Hsn_Name}')
    OR UPPER(GST_Name)=UPPER('{hsn.GST_Name}')
)";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "HSN Name or GST Name already exists.");
                    return Ok(jobject.ToString());
                }
                if (string.IsNullOrWhiteSpace(hsn.GST_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "GST Name cannot be empty.");
                    return Ok(jobject.ToString());
                }
                var data = new HSN_GST_MASTER
                {
                    Hsn_Code = Hsn_Code,
                    Hsn_Name = hsn.Hsn_Name.ToUpper(),
                    GST_Code = hsn.GST_Code,
                    GST_Name = hsn.GST_Name,
                    Is_Active = hsn.Is_Active,
                    Updated_By = hsn.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "HSN_GST_Master",
                    "Hsn_Code",
                    Hsn_Code,
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Updated Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Update");
                }

                return Ok(jobject.ToString());
            }
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        [HttpGet]
        [Route("DeleteHSNGST/{Hsn_Code}")]
        public IActionResult DeleteHSNGST(string Hsn_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE HSN_GST_Master
            SET Is_Active='D'
            WHERE Hsn_Code='{Hsn_Code}'";

            int result = SQLService.ExecuteNonQuery(query);

            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Deleted Successfully");
            }
            else
            {
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Delete");
            }

            return Ok(jobject.ToString());
        }

        [HttpGet]
        [Route("HSNGSTList")]
        public IActionResult HSNGSTList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_HSNGSTList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }
        [HttpGet]
        [Route("HSNList")]
        public IActionResult HSNList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_HSNList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        [HttpGet]
        [Route("GSTList")]
        public IActionResult GSTList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_GSTList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }
        private async Task<HSN_GST_MASTER> LoadHSNAndGSTMaxCode(HSN_GST_MASTER hsn)
        {
            DataTable hsnDt = SQLService.GetDataTable(@"
    SELECT
    ISNULL(MAX(CAST(REPLACE(Hsn_Code,'H','') AS INT)),0)+1
    FROM HSN_GST_Master
    WHERE Hsn_Code LIKE 'H%'");

            if (hsnDt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(hsnDt.Rows[0][0]);
                hsn.Hsn_Code = "H" + code.ToString().PadLeft(5, '0');
            }

            DataTable gstDt = SQLService.GetDataTable(@"
    SELECT
    ISNULL(MAX(CAST(REPLACE(GST_Code,'G','') AS INT)),0)+1
    FROM HSN_GST_Master
    WHERE GST_Code LIKE 'G%'");

            if (gstDt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(gstDt.Rows[0][0]);
                hsn.GST_Code = "G" + code.ToString().PadLeft(5, '0');
            }

            return hsn;
        }
    }
}