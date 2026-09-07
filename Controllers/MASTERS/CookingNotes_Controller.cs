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
    [Route("api/cookingnotes")]
    [ApiController]
    public class Cooking_Notes_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddCookingNotes")]
        public async Task<IActionResult> AddCookingNotes(
            COOKING_NOTES_MASTER cooking)
        {
            try
            {
                JObject jobject = new JObject();

                cooking.Cooking_Notes = cooking.Cooking_Notes?.Trim();

                if (string.IsNullOrWhiteSpace(cooking.Cooking_Notes))
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cooking Notes cannot be empty."
                    );

                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Cooking_Notes_Master
                WHERE UPPER(Cooking_Notes) =
                      UPPER('{cooking.Cooking_Notes}')
                AND Is_Active = 'A'";

                int count =
                    SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cooking Notes already exists."
                    );

                    return Ok(jobject.ToString());
                }

                string deleteInactiveQuery = $@"
                DELETE FROM Cooking_Notes_Master
                WHERE UPPER(Cooking_Notes) =
                      UPPER('{cooking.Cooking_Notes}')
                AND Is_Active = 'D'";

                SQLService.ExecuteNonQuery(deleteInactiveQuery);

                cooking = await LoadCookingMaxCode(cooking);

                var data = new COOKING_NOTES_MASTER
                {
                    Cooking_Code = cooking.Cooking_Code,
                    Cooking_Notes =
                        cooking.Cooking_Notes.ToUpper(),
                    Is_Active = cooking.Is_Active ?? "A",
                    Created_By = cooking.Created_By,
                    Created_On =
                        DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm:ss"
                        ),
                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>
                {
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery =
                    SQLHelper.BuildInsertQuery(
                        data,
                        "Cooking_Notes_Master",
                        ignoredColumns
                    );

                int result =
                    SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Cooking Notes Saved Successfully"
                    );
                    jobject.Add(
                        "cooking_code",
                        cooking.Cooking_Code
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cannot Save Cooking Notes"
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

        [HttpPost]
        [Route("UpdateCookingNotes/{Cooking_Code}")]
        public IActionResult UpdateCookingNotes(
            string Cooking_Code,
            COOKING_NOTES_MASTER cooking)
        {
            try
            {
                JObject jobject = new JObject();

                cooking.Cooking_Notes =
                    cooking.Cooking_Notes?.Trim();

                if (string.IsNullOrWhiteSpace(Cooking_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cooking Code is required."
                    );

                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(
                    cooking.Cooking_Notes))
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cooking Notes cannot be empty."
                    );

                    return Ok(jobject.ToString());
                }

                string recordCheckQuery = $@"
                SELECT COUNT(*)
                FROM Cooking_Notes_Master
                WHERE Cooking_Code = '{Cooking_Code}'
                AND Is_Active = 'A'";

                int recordCount =
                    SQLService.ExecuteScalarQuery(
                        recordCheckQuery
                    );

                if (recordCount == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cooking Notes record not found."
                    );

                    return Ok(jobject.ToString());
                }

                string duplicateCheckQuery = $@"
                SELECT COUNT(*)
                FROM Cooking_Notes_Master
                WHERE UPPER(Cooking_Notes) =
                      UPPER('{cooking.Cooking_Notes}')
                AND Cooking_Code <> '{Cooking_Code}'
                AND Is_Active = 'A'";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(
                        duplicateCheckQuery
                    );

                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cooking Notes already exists."
                    );

                    return Ok(jobject.ToString());
                }

                var data = new COOKING_NOTES_MASTER
                {
                    Cooking_Code = Cooking_Code,
                    Cooking_Notes =
                        cooking.Cooking_Notes.ToUpper(),
                    Is_Active = cooking.Is_Active ?? "A",
                    Updated_By = cooking.Updated_By,
                    Updated_On =
                        DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm:ss"
                        )
                };

                var ignoredColumns = new List<string>
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery =
                    SQLHelper.BuildUpdateQuery(
                        data,
                        "Cooking_Notes_Master",
                        "Cooking_Code",
                        Cooking_Code,
                        ignoredColumns
                    );

                int result =
                    SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Cooking Notes Updated Successfully"
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cannot Update Cooking Notes"
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
        [Route("DeleteCookingNotes/{Cooking_Code}")]
        public IActionResult DeleteCookingNotes(
            string Cooking_Code)
        {
            try
            {
                JObject jobject = new JObject();

                string query = $@"
                UPDATE Cooking_Notes_Master
                SET
                    Is_Active = 'D',
                    Updated_On = GETDATE()
                WHERE Cooking_Code = '{Cooking_Code}'
                AND Is_Active = 'A'";

                int result =
                    SQLService.ExecuteNonQuery(query);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Cooking Notes Deleted Successfully"
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Cooking Notes not found or already deleted."
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
        [Route("CookingNotesList")]
        public IActionResult CookingNotesList()
        {
            try
            {
                ApiResponse response = new ApiResponse();

                DataTable dt =
                    SQLService.GetDataTable(
                        "EXEC SP_CookingNotesList"
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
        [Route("GetCookingNotesByCode/{Cooking_Code}")]
        public IActionResult GetCookingNotesByCode(
            string Cooking_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                string query = $@"
                SELECT *
                FROM Cooking_Notes_Master
                WHERE Cooking_Code = '{Cooking_Code}'
                AND Is_Active = 'A'";

                DataTable dt =
                    SQLService.GetDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Cooking Notes not found."
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

        private async Task<COOKING_NOTES_MASTER>
            LoadCookingMaxCode(
                COOKING_NOTES_MASTER cooking)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
                ISNULL(
                    MAX(
                        TRY_CAST(
                            REPLACE(
                                Cooking_Code,
                                'CN',
                                ''
                            ) AS INT
                        )
                    ),
                    0
                ) + 1
            FROM Cooking_Notes_Master
            WHERE Cooking_Code LIKE 'CN%'");

            if (dt.Rows.Count > 0)
            {
                int code =
                    Convert.ToInt32(dt.Rows[0][0]);

                cooking.Cooking_Code =
                    "CN" +
                    code.ToString().PadLeft(5, '0');
            }

            return await Task.FromResult(cooking);
        }
    }
}