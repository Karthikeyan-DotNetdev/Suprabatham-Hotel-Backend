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
    [Route("api/kitchen")]
    [ApiController]
    public class Kitchen_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddKitchen")]
        public async Task<IActionResult> AddKitchen(KITCHEN_MASTER kitchen)
        {
            try
            {
                JObject jobject = new JObject();

                kitchen.Kitchen_Name = kitchen.Kitchen_Name?.Trim();

                if (string.IsNullOrWhiteSpace(kitchen.Kitchen_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Kitchen Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Kitchen_Master
                WHERE UPPER(Kitchen_Name)=UPPER('{kitchen.Kitchen_Name}')
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Kitchen Name already exists.");
                    return Ok(jobject.ToString());
                }

                string deleteInactiveQuery = $@"
                DELETE FROM Kitchen_Master
                WHERE UPPER(Kitchen_Name)=UPPER('{kitchen.Kitchen_Name}')
                AND Is_Active='D'";

                SQLService.ExecuteNonQuery(deleteInactiveQuery);

                kitchen = await LoadKitchenMaxCode(kitchen);

                var data = new KITCHEN_MASTER
                {
                    Kitchen_Code = kitchen.Kitchen_Code,
                    Kitchen_Name = kitchen.Kitchen_Name.ToUpper(),
                    Is_Active = kitchen.Is_Active ?? "A",
                    Created_By = kitchen.Created_By,
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
                    "Kitchen_Master",
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
        [Route("UpdateKitchen/{Kitchen_Code}")]
        public IActionResult UpdateKitchen(string Kitchen_Code, KITCHEN_MASTER kitchen)
        {
            try
            {
                JObject jobject = new JObject();

                kitchen.Kitchen_Name = kitchen.Kitchen_Name?.Trim();

                if (string.IsNullOrWhiteSpace(kitchen.Kitchen_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Kitchen Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Kitchen_Master
                WHERE UPPER(Kitchen_Name)=UPPER('{kitchen.Kitchen_Name}')
                AND Kitchen_Code<>'{Kitchen_Code}'
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Kitchen Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new KITCHEN_MASTER
                {
                    Kitchen_Code = Kitchen_Code,
                    Kitchen_Name = kitchen.Kitchen_Name.ToUpper(),
                    Is_Active = kitchen.Is_Active,
                    Updated_By = kitchen.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Kitchen_Master",
                    "Kitchen_Code",
                    Kitchen_Code,
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
        [Route("DeleteKitchen/{Kitchen_Code}")]
        public IActionResult DeleteKitchen(string Kitchen_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE Kitchen_Master
            SET Is_Active='D'
            WHERE Kitchen_Code='{Kitchen_Code}'";

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
        [Route("KitchenList")]
        public IActionResult KitchenList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_KitchenList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        private async Task<KITCHEN_MASTER> LoadKitchenMaxCode(KITCHEN_MASTER kitchen)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
            ISNULL(MAX(CAST(REPLACE(Kitchen_Code,'K','') AS INT)),0)+1
            FROM Kitchen_Master
            WHERE Kitchen_Code LIKE 'K%'");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                kitchen.Kitchen_Code = "K" + code.ToString().PadLeft(5, '0');
            }

            return kitchen;
        }
    }
}