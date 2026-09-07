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
    [Route("api/tender")]
    [ApiController]
    public class Tender_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddTender")]
        public async Task<IActionResult> AddTender(TENDER_MASTER tender)
        {
            try
            {
                JObject jobject = new JObject();

                tender.Tender_Name = tender.Tender_Name?.Trim();

                if (string.IsNullOrWhiteSpace(tender.Tender_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Tender Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Tender_Master
                WHERE UPPER(Tender_Name)=UPPER('{tender.Tender_Name}')
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Tender Name already exists.");
                    return Ok(jobject.ToString());
                }

                tender = await LoadTenderMaxCode(tender);

                var data = new TENDER_MASTER
                {
                    Tender_Code = tender.Tender_Code,
                    Tender_Name = tender.Tender_Name.ToUpper(),
                    Display_Order = tender.Display_Order, 
                    Is_Active = tender.Is_Active ?? "A",
                    Created_By = tender.Created_By,
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
                    "Tender_Master",
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
        [Route("UpdateTender/{Tender_Code}")]
        public IActionResult UpdateTender(string Tender_Code, TENDER_MASTER tender)
        {
            try
            {
                JObject jobject = new JObject();

                tender.Tender_Name = tender.Tender_Name?.Trim();

                if (string.IsNullOrWhiteSpace(tender.Tender_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Tender Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Tender_Master
                WHERE UPPER(Tender_Name)=UPPER('{tender.Tender_Name}')
                AND Tender_Code<>'{Tender_Code}'
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Tender Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new TENDER_MASTER
                {
                    Tender_Code = Tender_Code,
                    Tender_Name = tender.Tender_Name.ToUpper(),
                    Display_Order = tender.Display_Order,
                    Is_Active = tender.Is_Active,
                    Updated_By = tender.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Tender_Master",
                    "Tender_Code",
                    Tender_Code,
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
        [Route("DeleteTender/{Tender_Code}")]
        public IActionResult DeleteTender(string Tender_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE Tender_Master
            SET Is_Active='D'
            WHERE Tender_Code='{Tender_Code}'";

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
        [Route("TenderList")]
        public IActionResult TenderList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_TenderList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        private async Task<TENDER_MASTER> LoadTenderMaxCode(TENDER_MASTER tender)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
            ISNULL(MAX(CAST(REPLACE(Tender_Code,'T','') AS INT)),0)+1
            FROM Tender_Master
            WHERE Tender_Code LIKE 'T%'");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                tender.Tender_Code = "T" + code.ToString().PadLeft(5, '0');
            }

            return tender;
        }
    }
}