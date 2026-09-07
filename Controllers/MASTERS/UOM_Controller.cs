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
    [Route("api/uom")]
    [ApiController]
    public class UOM_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddUOM")]
        public async Task<IActionResult> AddUOM(UOM_MASTER uom)
        {
            try
            {
                JObject jobject = new JObject();

                uom.UOM_Name = uom.UOM_Name?.Trim();

                if (string.IsNullOrWhiteSpace(uom.UOM_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "UOM Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM UOM_Master
                WHERE UPPER(UOM_Name)=UPPER('{uom.UOM_Name}')
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "UOM Name already exists.");
                    return Ok(jobject.ToString());
                }

                uom = await LoadUOMMaxCode(uom);

                var data = new UOM_MASTER
                {
                    UOM_Code = uom.UOM_Code,
                    UOM_Name = uom.UOM_Name.ToUpper(),
                    Is_Active = uom.Is_Active ?? "A",
                    Created_By = uom.Created_By,
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
                    "UOM_Master",
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
        [Route("UpdateUOM/{UOM_Code}")]
        public IActionResult UpdateUOM(string UOM_Code, UOM_MASTER uom)
        {
            try
            {
                JObject jobject = new JObject();

                uom.UOM_Name = uom.UOM_Name?.Trim();

                if (string.IsNullOrWhiteSpace(uom.UOM_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "UOM Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM UOM_Master
                WHERE UPPER(UOM_Name)=UPPER('{uom.UOM_Name}')
                AND UOM_Code<>'{UOM_Code}'
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "UOM Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new UOM_MASTER
                {
                    UOM_Code = UOM_Code,
                    UOM_Name = uom.UOM_Name.ToUpper(),
                    Is_Active = uom.Is_Active,
                    Updated_By = uom.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "UOM_Master",
                    "UOM_Code",
                    UOM_Code,
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
        [Route("DeleteUOM/{UOM_Code}")]
        public IActionResult DeleteUOM(string UOM_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE UOM_Master
            SET Is_Active='D'
            WHERE UOM_Code='{UOM_Code}'";

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
        [Route("UOMList")]
        public IActionResult UOMList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_UOMList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        private async Task<UOM_MASTER> LoadUOMMaxCode(UOM_MASTER uom)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
            ISNULL(MAX(CAST(REPLACE(UOM_Code,'U','') AS INT)),0)+1
            FROM UOM_Master");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                uom.UOM_Code = "U" + code.ToString().PadLeft(5, '0');
            }

            return uom;
        }
    }
}