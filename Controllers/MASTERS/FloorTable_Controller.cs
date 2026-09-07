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
    [Route("api/floortable")]
    [ApiController]
    public class Floor_Table_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddFloorTable")]
        public async Task<IActionResult> AddFloorTable(
            FLOOR_TABLE_MASTER table)
        {
            try
            {
                JObject jobject = new JObject();

                table.Branch_Code = table.Branch_Code?.Trim();
                table.Floor_Name = table.Floor_Name?.Trim();
                table.Table_Name = table.Table_Name?.Trim();

                if (string.IsNullOrWhiteSpace(table.Branch_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Branch Code is required.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(table.Floor_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Floor Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(table.Table_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Table Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                if (table.Chair <= 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Chair count must be greater than zero."
                    );
                    return Ok(jobject.ToString());
                }

                string branchCheckQuery = $@"
                SELECT COUNT(*)
                FROM Branch_Master
                WHERE Branch_Code = '{table.Branch_Code}'
                AND Is_Active = 'A'";

                int branchCount =
                    SQLService.ExecuteScalarQuery(branchCheckQuery);

                if (branchCount == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Invalid Branch Code.");
                    return Ok(jobject.ToString());
                }

                string duplicateCheckQuery = $@"
                SELECT COUNT(*)
                FROM Floor_Table_Master
                WHERE Branch_Code = '{table.Branch_Code}'
                AND UPPER(Floor_Name) = UPPER('{table.Floor_Name}')
                AND UPPER(Table_Name) = UPPER('{table.Table_Name}')
                AND Is_Active = 'A'";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(duplicateCheckQuery);

                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Table Name already exists for this branch and floor."
                    );
                    return Ok(jobject.ToString());
                }

                string deleteInactiveQuery = $@"
                DELETE FROM Floor_Table_Master
                WHERE Branch_Code = '{table.Branch_Code}'
                AND UPPER(Floor_Name) = UPPER('{table.Floor_Name}')
                AND UPPER(Table_Name) = UPPER('{table.Table_Name}')
                AND Is_Active = 'D'";

                SQLService.ExecuteNonQuery(deleteInactiveQuery);

                table = await LoadTableMaxCode(table);

                var data = new FLOOR_TABLE_MASTER
                {
                    Table_Code = table.Table_Code,
                    Branch_Code = table.Branch_Code,
                    Floor_Name = table.Floor_Name.ToUpper(),
                    Table_Name = table.Table_Name.ToUpper(),
                    Chair = table.Chair,
                    Is_Active = table.Is_Active ?? "A",
                    Created_By = table.Created_By,
                    Created_On =
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>
                {
                    "Branch_Name",
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    data,
                    "Floor_Table_Master",
                    ignoredColumns
                );

                int result = SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Floor Table Saved Successfully"
                    );
                    jobject.Add("table_code", table.Table_Code);
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Save Floor Table");
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
        [Route("UpdateFloorTable/{Table_Code}")]
        public IActionResult UpdateFloorTable(
            string Table_Code,
            FLOOR_TABLE_MASTER table)
        {
            try
            {
                JObject jobject = new JObject();

                table.Branch_Code = table.Branch_Code?.Trim();
                table.Floor_Name = table.Floor_Name?.Trim();
                table.Table_Name = table.Table_Name?.Trim();

                if (string.IsNullOrWhiteSpace(Table_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Table Code is required.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(table.Branch_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Branch Code is required.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(table.Floor_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Floor Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(table.Table_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Table Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                if (table.Chair <= 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Chair count must be greater than zero."
                    );
                    return Ok(jobject.ToString());
                }

                string recordCheckQuery = $@"
                SELECT COUNT(*)
                FROM Floor_Table_Master
                WHERE Table_Code = '{Table_Code}'
                AND Is_Active = 'A'";

                int recordCount =
                    SQLService.ExecuteScalarQuery(recordCheckQuery);

                if (recordCount == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Floor Table record not found."
                    );
                    return Ok(jobject.ToString());
                }

                string branchCheckQuery = $@"
                SELECT COUNT(*)
                FROM Branch_Master
                WHERE Branch_Code = '{table.Branch_Code}'
                AND Is_Active = 'A'";

                int branchCount =
                    SQLService.ExecuteScalarQuery(branchCheckQuery);

                if (branchCount == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Invalid Branch Code.");
                    return Ok(jobject.ToString());
                }

                string duplicateCheckQuery = $@"
                SELECT COUNT(*)
                FROM Floor_Table_Master
                WHERE Branch_Code = '{table.Branch_Code}'
                AND UPPER(Floor_Name) = UPPER('{table.Floor_Name}')
                AND UPPER(Table_Name) = UPPER('{table.Table_Name}')
                AND Table_Code <> '{Table_Code}'
                AND Is_Active = 'A'";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(duplicateCheckQuery);

                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Table Name already exists for this branch and floor."
                    );
                    return Ok(jobject.ToString());
                }

                var data = new FLOOR_TABLE_MASTER
                {
                    Table_Code = Table_Code,
                    Branch_Code = table.Branch_Code,
                    Floor_Name = table.Floor_Name.ToUpper(),
                    Table_Name = table.Table_Name.ToUpper(),
                    Chair = table.Chair,
                    Is_Active = table.Is_Active ?? "A",
                    Updated_By = table.Updated_By,
                    Updated_On =
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>
                {
                    "Branch_Name",
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Floor_Table_Master",
                    "Table_Code",
                    Table_Code,
                    ignoredColumns
                );

                int result = SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Floor Table Updated Successfully"
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Update Floor Table");
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
        [Route("DeleteFloorTable/{Table_Code}")]
        public IActionResult DeleteFloorTable(string Table_Code)
        {
            try
            {
                JObject jobject = new JObject();

                string query = $@"
                UPDATE Floor_Table_Master
                SET
                    Is_Active = 'D',
                    Updated_On = GETDATE()
                WHERE Table_Code = '{Table_Code}'
                AND Is_Active = 'A'";

                int result = SQLService.ExecuteNonQuery(query);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Floor Table Deleted Successfully"
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Floor Table not found or already deleted."
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
        [Route("FloorTableList")]
        public IActionResult FloorTableList()
        {
            try
            {
                ApiResponse response = new ApiResponse();

                DataTable dt = SQLService.GetDataTable(
                    "EXEC SP_FloorTableList"
                );

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
        [Route("GetFloorTableByCode/{Table_Code}")]
        public IActionResult GetFloorTableByCode(string Table_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                string query = $@"
                SELECT
                    F.*,
                    B.Branch_Name
                FROM Floor_Table_Master F
                INNER JOIN Branch_Master B
                    ON F.Branch_Code = B.Branch_Code
                WHERE F.Table_Code = '{Table_Code}'
                AND F.Is_Active = 'A'";

                DataTable dt = SQLService.GetDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Floor Table not found."
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

        [HttpGet]
        [Route("FloorTableListByBranch/{Branch_Code}")]
        public IActionResult FloorTableListByBranch(string Branch_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                string query = $@"
                SELECT
                    ROW_NUMBER() OVER(
                        ORDER BY F.Floor_Name, F.Table_Name
                    ) AS Sno,
                    F.*,
                    B.Branch_Name
                FROM Floor_Table_Master F
                INNER JOIN Branch_Master B
                    ON F.Branch_Code = B.Branch_Code
                WHERE F.Branch_Code = '{Branch_Code}'
                AND F.Is_Active = 'A'
                ORDER BY F.Floor_Name, F.Table_Name";

                DataTable dt = SQLService.GetDataTable(query);

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

        private async Task<FLOOR_TABLE_MASTER> LoadTableMaxCode(
            FLOOR_TABLE_MASTER table)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
                ISNULL(
                    MAX(
                        TRY_CAST(
                            REPLACE(Table_Code, 'T', '') AS INT
                        )
                    ),
                    0
                ) + 1
            FROM Floor_Table_Master
            WHERE Table_Code LIKE 'T%'");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);

                table.Table_Code =
                    "T" + code.ToString().PadLeft(5, '0');
            }

            return await Task.FromResult(table);
        }
    }
}