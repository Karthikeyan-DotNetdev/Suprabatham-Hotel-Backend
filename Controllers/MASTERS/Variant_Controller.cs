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
    [Route("api/variant")]
    [ApiController]
    public class Variant_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddVariant")]
        public async Task<IActionResult> AddVariant(VARIANT_MASTER variant)
        {
            try
            {
                JObject jobject = new JObject();

                variant.Variant_Name = variant.Variant_Name?.Trim();

                if (string.IsNullOrWhiteSpace(variant.Variant_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Variant Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Variant_Master
                WHERE UPPER(Variant_Name) = UPPER('{variant.Variant_Name}')
                AND Is_Active = 'A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Variant Name already exists.");
                    return Ok(jobject.ToString());
                }

                string deleteInactiveQuery = $@"
                DELETE FROM Variant_Master
                WHERE UPPER(Variant_Name) = UPPER('{variant.Variant_Name}')
                AND Is_Active = 'D'";

                SQLService.ExecuteNonQuery(deleteInactiveQuery);

                variant = await LoadVariantMaxCode(variant);

                var data = new VARIANT_MASTER
                {
                    Variant_Code = variant.Variant_Code,
                    Variant_Name = variant.Variant_Name.ToUpper(),
                    Is_Active = variant.Is_Active ?? "A",
                    Created_By = variant.Created_By,
                    Created_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>
                {
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    data,
                    "Variant_Master",
                    ignoredColumns
                );

                int result = SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Variant Saved Successfully");
                    jobject.Add("variant_code", variant.Variant_Code);
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Save Variant");
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
        [Route("UpdateVariant/{Variant_Code}")]
        public IActionResult UpdateVariant(
            string Variant_Code,
            VARIANT_MASTER variant)
        {
            try
            {
                JObject jobject = new JObject();

                variant.Variant_Name = variant.Variant_Name?.Trim();

                if (string.IsNullOrWhiteSpace(variant.Variant_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Variant Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string recordCheckQuery = $@"
                SELECT COUNT(*)
                FROM Variant_Master
                WHERE Variant_Code = '{Variant_Code}'
                AND Is_Active = 'A'";

                int recordCount =
                    SQLService.ExecuteScalarQuery(recordCheckQuery);

                if (recordCount == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Variant record not found.");
                    return Ok(jobject.ToString());
                }

                string duplicateCheckQuery = $@"
                SELECT COUNT(*)
                FROM Variant_Master
                WHERE UPPER(Variant_Name) = UPPER('{variant.Variant_Name}')
                AND Variant_Code <> '{Variant_Code}'
                AND Is_Active = 'A'";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(duplicateCheckQuery);

                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Variant Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new VARIANT_MASTER
                {
                    Variant_Code = Variant_Code,
                    Variant_Name = variant.Variant_Name.ToUpper(),
                    Is_Active = variant.Is_Active ?? "A",
                    Updated_By = variant.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Variant_Master",
                    "Variant_Code",
                    Variant_Code,
                    ignoredColumns
                );

                int result = SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Variant Updated Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Update Variant");
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
        [Route("DeleteVariant/{Variant_Code}")]
        public IActionResult DeleteVariant(string Variant_Code)
        {
            try
            {
                JObject jobject = new JObject();

                string query = $@"
                UPDATE Variant_Master
                SET
                    Is_Active = 'D',
                    Updated_On = GETDATE()
                WHERE Variant_Code = '{Variant_Code}'
                AND Is_Active = 'A'";

                int result = SQLService.ExecuteNonQuery(query);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Variant Deleted Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Variant not found or already deleted."
                    );
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
        [Route("VariantList")]
        public IActionResult VariantList()
        {
            try
            {
                ApiResponse response = new ApiResponse();

                DataTable dt =
                    SQLService.GetDataTable("EXEC SP_VariantList");

                response.status = true;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetVariantByCode/{Variant_Code}")]
        public IActionResult GetVariantByCode(string Variant_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                string query = $@"
                SELECT *
                FROM Variant_Master
                WHERE Variant_Code = '{Variant_Code}'
                AND Is_Active = 'A'";

                DataTable dt = SQLService.GetDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Variant not found."
                    });
                }

                response.status = true;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        private async Task<VARIANT_MASTER> LoadVariantMaxCode(
            VARIANT_MASTER variant)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
                ISNULL(
                    MAX(
                        TRY_CAST(
                            REPLACE(Variant_Code, 'V', '') AS INT
                        )
                    ),
                    0
                ) + 1
            FROM Variant_Master
            WHERE Variant_Code LIKE 'V%'");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);

                variant.Variant_Code =
                    "V" + code.ToString().PadLeft(5, '0');
            }

            return await Task.FromResult(variant);
        }
    }
}