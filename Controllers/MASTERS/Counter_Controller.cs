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
    [Route("api/counter")]
    [ApiController]
    public class Counter_Master_Controller : Controller
    {
        // ============================================================
        // ADD COUNTER
        // ============================================================

        [HttpPost]
        [Route("AddCounter")]
        public async Task<IActionResult> AddCounter(
            [FromBody] COUNTER_MASTER counter)
        {
            try
            {
                JObject response = new JObject();

                if (counter == null)
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter request cannot be empty."
                    );

                    return Ok(response.ToString());
                }

                NormalizeCounter(counter);

                string validationMessage =
                    ValidateCounter(counter, true);

                if (!string.IsNullOrWhiteSpace(validationMessage))
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        validationMessage
                    );

                    return Ok(response.ToString());
                }

                string counterName =
                    EscapeSql(counter.Counter_Name);

                string branchCode =
                    EscapeSql(counter.Branch_Code);

                // Duplicate counter name check inside same branch.
                string duplicateQuery = $@"
SELECT COUNT(*)
FROM dbo.M_Counter_Master
WHERE UPPER(Counter_Name) =
    UPPER('{counterName}')
AND Branch_Code = '{branchCode}'
AND Is_Active = 'A';";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(
                        duplicateQuery
                    );

                if (duplicateCount > 0)
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter Name already exists for this branch."
                    );

                    return Ok(response.ToString());
                }

                /*
                 * Delete an old inactive record having the same
                 * counter name in the same branch.
                 */
                string deleteInactiveQuery = $@"
DELETE FROM dbo.M_Counter_Master
WHERE UPPER(Counter_Name) =
    UPPER('{counterName}')
AND Branch_Code = '{branchCode}'
AND Is_Active = 'D';";

                SQLService.ExecuteNonQuery(
                    deleteInactiveQuery
                );

                counter =
                    await LoadCounterMaxCode(counter);

                var data = new COUNTER_MASTER
                {
                    Counter_Code =
                        counter.Counter_Code,

                    Counter_Name =
                        counter.Counter_Name?.ToUpper(),

                    Branch_Code =
                        counter.Branch_Code,

                    Counter_Type =
                        counter.Counter_Type,

                    Location_Name =
                        EmptyToNull(
                            counter.Location_Name
                        ),

                    Printer_Name =
                        EmptyToNull(
                            counter.Printer_Name
                        ),

                    Printer_IP =
                        EmptyToNull(
                            counter.Printer_IP
                        ),

                    Device_Name =
                        EmptyToNull(
                            counter.Device_Name
                        ),

                    Device_Id =
                        EmptyToNull(
                            counter.Device_Id
                        ),

                    Is_Default =
                        counter.Is_Default ?? "N",

                    Is_Active =
                        counter.Is_Active ?? "A",

                    Remarks =
                        EmptyToNull(
                            counter.Remarks
                        ),

                    Created_By =
                        counter.Created_By,

                    Created_On =
                        DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm:ss"
                        ),

                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns =
                    new List<string>
                    {
                        "Counter_Id",
                        "Updated_By",
                        "Updated_On"
                    };

                /*
                 * Make sure SQLHelper supports null properties.
                 * Counter_Id is ignored because it is IDENTITY.
                 */
                string insertQuery =
                    SQLHelper.BuildInsertQuery(
                        data,
                        "M_Counter_Master",
                        ignoredColumns
                    );

                int result =
                    SQLService.ExecuteNonQuery(
                        insertQuery
                    );

                if (result > 0)
                {
                    response.Add("status", true);
                    response.Add(
                        "message",
                        "Counter saved successfully."
                    );

                    response.Add(
                        "counter_code",
                        counter.Counter_Code
                    );
                }
                else
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Cannot save counter."
                    );
                }

                return Ok(response.ToString());
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

        // ============================================================
        // UPDATE COUNTER
        // ============================================================

        [HttpPost]
        [Route("UpdateCounter/{counterCode}")]
        public IActionResult UpdateCounter(
            string counterCode,
            [FromBody] COUNTER_MASTER counter)
        {
            try
            {
                JObject response = new JObject();

                counterCode =
                    counterCode?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(counterCode))
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter Code cannot be empty."
                    );

                    return Ok(response.ToString());
                }

                if (counter == null)
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter request cannot be empty."
                    );

                    return Ok(response.ToString());
                }

                NormalizeCounter(counter);

                string validationMessage =
                    ValidateCounter(counter, false);

                if (!string.IsNullOrWhiteSpace(validationMessage))
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        validationMessage
                    );

                    return Ok(response.ToString());
                }

                string escapedCounterCode =
                    EscapeSql(counterCode);

                string counterName =
                    EscapeSql(counter.Counter_Name);

                string branchCode =
                    EscapeSql(counter.Branch_Code);

                // Check whether the counter exists.
                string counterExistsQuery = $@"
SELECT COUNT(*)
FROM dbo.M_Counter_Master
WHERE Counter_Code =
    '{escapedCounterCode}';";

                int counterExists =
                    SQLService.ExecuteScalarQuery(
                        counterExistsQuery
                    );

                if (counterExists == 0)
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter not found."
                    );

                    return Ok(response.ToString());
                }

                /*
                 * Duplicate name check:
                 * Same counter name is not allowed within the same branch,
                 * except for the current counter.
                 */
                string duplicateQuery = $@"
SELECT COUNT(*)
FROM dbo.M_Counter_Master
WHERE UPPER(Counter_Name) =
    UPPER('{counterName}')
AND Branch_Code = '{branchCode}'
AND Counter_Code <> '{escapedCounterCode}'
AND Is_Active = 'A';";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(
                        duplicateQuery
                    );

                if (duplicateCount > 0)
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter Name already exists for this branch."
                    );

                    return Ok(response.ToString());
                }

                /*
                 * Only one default counter per branch.
                 * If current counter is selected as default,
                 * reset all other counters in the branch.
                 */
                if (counter.Is_Default == "Y")
                {
                    string resetDefaultQuery = $@"
UPDATE dbo.M_Counter_Master
SET
    Is_Default = 'N',
    Updated_By =
        '{EscapeSql(counter.Updated_By)}',
    Updated_On = GETDATE()
WHERE Branch_Code = '{branchCode}'
AND Counter_Code <> '{escapedCounterCode}'
AND Is_Active = 'A';";

                    SQLService.ExecuteNonQuery(
                        resetDefaultQuery
                    );
                }

                var data = new COUNTER_MASTER
                {
                    Counter_Code =
                        counterCode,

                    Counter_Name =
                        counter.Counter_Name?.ToUpper(),

                    Branch_Code =
                        counter.Branch_Code,

                    Counter_Type =
                        counter.Counter_Type,

                    Location_Name =
                        EmptyToNull(
                            counter.Location_Name
                        ),

                    Printer_Name =
                        EmptyToNull(
                            counter.Printer_Name
                        ),

                    Printer_IP =
                        EmptyToNull(
                            counter.Printer_IP
                        ),

                    Device_Name =
                        EmptyToNull(
                            counter.Device_Name
                        ),

                    Device_Id =
                        EmptyToNull(
                            counter.Device_Id
                        ),

                    Is_Default =
                        counter.Is_Default,

                    Is_Active =
                        counter.Is_Active,

                    Remarks =
                        EmptyToNull(
                            counter.Remarks
                        ),

                    Updated_By =
                        counter.Updated_By,

                    Updated_On =
                        DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm:ss"
                        )
                };

                var ignoredColumns =
                    new List<string>
                    {
                        "Counter_Id",
                        "Created_By",
                        "Created_On"
                    };

                string updateQuery =
                    SQLHelper.BuildUpdateQuery(
                        data,
                        "M_Counter_Master",
                        "Counter_Code",
                        counterCode,
                        ignoredColumns
                    );

                int result =
                    SQLService.ExecuteNonQuery(
                        updateQuery
                    );

                if (result > 0)
                {
                    response.Add("status", true);
                    response.Add(
                        "message",
                        "Counter updated successfully."
                    );
                }
                else
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Cannot update counter."
                    );
                }

                return Ok(response.ToString());
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

        // ============================================================
        // DELETE COUNTER - SOFT DELETE
        // ============================================================

        [HttpGet]
        [Route("DeleteCounter/{counterCode}")]
        public IActionResult DeleteCounter(
            string counterCode,
            string? updatedBy = null)
        {
            try
            {
                JObject response = new JObject();

                counterCode =
                    counterCode?.Trim() ?? string.Empty;

                updatedBy =
                    updatedBy?.Trim();

                if (string.IsNullOrWhiteSpace(counterCode))
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter Code cannot be empty."
                    );

                    return Ok(response.ToString());
                }

                string query = $@"
UPDATE dbo.M_Counter_Master
SET
    Is_Active = 'D',
    Is_Default = 'N',
    Updated_By =
        {(string.IsNullOrWhiteSpace(updatedBy)
            ? "NULL"
            : $"'{EscapeSql(updatedBy)}'")},
    Updated_On = GETDATE()
WHERE Counter_Code =
    '{EscapeSql(counterCode)}'
AND Is_Active = 'A';";

                int result =
                    SQLService.ExecuteNonQuery(
                        query
                    );

                if (result > 0)
                {
                    response.Add("status", true);
                    response.Add(
                        "message",
                        "Counter deleted successfully."
                    );
                }
                else
                {
                    response.Add("status", false);
                    response.Add(
                        "message",
                        "Counter not found or already deleted."
                    );
                }

                return Ok(response.ToString());
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

        // ============================================================
        // COUNTER LIST
        // Optional branch filter.
        // ============================================================

        [HttpGet]
        [Route("CounterList")]
        public IActionResult CounterList(
            string? branchCode = null,
            string? counterType = null)
        {
            try
            {
                ApiResponse response =
                    new ApiResponse();

                branchCode =
                    string.IsNullOrWhiteSpace(branchCode)
                        ? null
                        : branchCode.Trim();

                counterType =
                    string.IsNullOrWhiteSpace(counterType)
                        ? null
                        : counterType
                            .Trim()
                            .ToUpper();

                if (counterType != null &&
                    counterType != "ALL" &&
                    counterType != "DINE_IN" &&
                    counterType != "TAKEAWAY")
                {
                    response.status = false;
                    response.message =
                        "Counter Type must be ALL, DINE_IN or TAKEAWAY.";

                    return new JsonResult(response);
                }

                string query = $@"
EXEC dbo.SP_CounterList
    @BranchCode =
        {SqlNullable(branchCode)},
    @CounterType =
        {SqlNullable(counterType)};";

                DataTable dt =
                    SQLService.GetDataTable(query);

                response.status = true;

                response.message =
                    dt.Rows.Count > 0
                        ? "Counter list loaded successfully."
                        : "No counter records found.";

                response.data =
                    UtilityService.DataTableToJArray(
                        dt
                    );

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

        // ============================================================
        // GET COUNTER BY CODE
        // ============================================================

        [HttpGet]
        [Route("GetCounterByCode/{counterCode}")]
        public IActionResult GetCounterByCode(
            string counterCode)
        {
            try
            {
                counterCode =
                    counterCode?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(counterCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message =
                            "Counter Code cannot be empty."
                    });
                }

                string query = $@"
SELECT
    Counter_Id,
    Counter_Code,
    Counter_Name,
    Branch_Code,
    Counter_Type,
    Location_Name,
    Printer_Name,
    Printer_IP,
    Device_Name,
    Device_Id,
    Is_Default,
    Is_Active,
    Remarks,
    Created_By,
    Created_On,
    Updated_By,
    Updated_On
FROM dbo.M_Counter_Master
WHERE Counter_Code =
    '{EscapeSql(counterCode)}';";

                DataTable dt =
                    SQLService.GetDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message =
                            "Counter not found."
                    });
                }

                JArray data =
                    UtilityService.DataTableToJArray(
                        dt
                    );

                return new JsonResult(new
                {
                    status = true,
                    message =
                        "Counter details loaded successfully.",
                    data = data.FirstOrDefault()
                });
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

        // ============================================================
        // GENERATE COUNTER CODE
        // Example: C00001
        // ============================================================

        private Task<COUNTER_MASTER> LoadCounterMaxCode(
            COUNTER_MASTER counter)
        {
            DataTable dt =
                SQLService.GetDataTable(@"
SELECT
    ISNULL
    (
        MAX
        (
            TRY_CAST
            (
                REPLACE(Counter_Code, 'C', '')
                AS INT
            )
        ),
        0
    ) + 1
FROM dbo.M_Counter_Master
WHERE Counter_Code LIKE 'C%';");

            if (dt.Rows.Count > 0)
            {
                int code =
                    Convert.ToInt32(
                        dt.Rows[0][0]
                    );

                counter.Counter_Code =
                    "C" +
                    code.ToString()
                        .PadLeft(5, '0');
            }

            return Task.FromResult(counter);
        }

        // ============================================================
        // NORMALIZATION
        // ============================================================

        private static void NormalizeCounter(
            COUNTER_MASTER counter)
        {
            counter.Counter_Name =
                counter.Counter_Name?.Trim();

            counter.Branch_Code =
                counter.Branch_Code?.Trim();

            counter.Counter_Type =
                counter.Counter_Type?
                    .Trim()
                    .ToUpper();

            counter.Location_Name =
                counter.Location_Name?.Trim();

            counter.Printer_Name =
                counter.Printer_Name?.Trim();

            counter.Printer_IP =
                counter.Printer_IP?.Trim();

            counter.Device_Name =
                counter.Device_Name?.Trim();

            counter.Device_Id =
                counter.Device_Id?.Trim();

            counter.Is_Default =
                string.IsNullOrWhiteSpace(
                    counter.Is_Default
                )
                    ? "N"
                    : counter.Is_Default
                        .Trim()
                        .ToUpper();

            counter.Is_Active =
                string.IsNullOrWhiteSpace(
                    counter.Is_Active
                )
                    ? "A"
                    : counter.Is_Active
                        .Trim()
                        .ToUpper();

            counter.Remarks =
                counter.Remarks?.Trim();

            counter.Created_By =
                counter.Created_By?.Trim();

            counter.Updated_By =
                counter.Updated_By?.Trim();
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        private static string ValidateCounter(
            COUNTER_MASTER counter,
            bool isNewRecord)
        {
            if (string.IsNullOrWhiteSpace(
                counter.Counter_Name))
            {
                return "Counter Name cannot be empty.";
            }

            if (string.IsNullOrWhiteSpace(
                counter.Branch_Code))
            {
                return "Branch Code cannot be empty.";
            }

            if (string.IsNullOrWhiteSpace(
                counter.Counter_Type))
            {
                return "Counter Type cannot be empty.";
            }

            string[] validCounterTypes =
            {
                "ALL",
                "DINE_IN",
                "TAKEAWAY"
            };

            if (!validCounterTypes.Contains(
                counter.Counter_Type))
            {
                return
                    "Counter Type must be ALL, DINE_IN or TAKEAWAY.";
            }

            if (counter.Is_Default != "Y" &&
                counter.Is_Default != "N")
            {
                return
                    "Is Default must be Y or N.";
            }

            if (counter.Is_Active != "A" &&
                counter.Is_Active != "D")
            {
                return
                    "Is Active must be A or D.";
            }

            if (isNewRecord &&
                string.IsNullOrWhiteSpace(
                    counter.Created_By))
            {
                return "Created By cannot be empty.";
            }

            if (!isNewRecord &&
                string.IsNullOrWhiteSpace(
                    counter.Updated_By))
            {
                return "Updated By cannot be empty.";
            }

            return string.Empty;
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private static string EscapeSql(
            string? value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Replace("'", "''");
        }

        private static string SqlNullable(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "NULL";
            }

            return $"'{EscapeSql(value)}'";
        }

        private static string? EmptyToNull(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}