using CommonModels;
using CommonServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using WIN_BOT;

namespace laptop_service.Controllers.MASTERS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/state")]
    [ApiController]
    public class StateController : ControllerBase
    {
        // GET:
        // api/state/code/Tamil%20Nadu

        [HttpGet]
        [Route("code/{stateName}")]
        public IActionResult GetStateCode(string stateName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stateName))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "State name is required."
                    });
                }

                stateName = stateName.Trim();

                // Prevent SQL error when state name contains apostrophe.
                string safeStateName = stateName.Replace("'", "''");

                string query = $@"
                SELECT TOP 1
                    StateName,
                    StateCode
                FROM
                (
                    SELECT
                        StateName,
                        StateCode,
                        1 AS MatchPriority
                    FROM StateMaster
                    WHERE
                        REPLACE(
                            LOWER(LTRIM(RTRIM(StateName))),
                            ' ',
                            ''
                        ) =
                        REPLACE(
                            LOWER(LTRIM(RTRIM('{safeStateName}'))),
                            ' ',
                            ''
                        )

                    UNION ALL

                    SELECT
                        SA.AliasName AS StateName,
                        SA.StateCode,
                        2 AS MatchPriority
                    FROM StateAlias SA
                    WHERE
                        REPLACE(
                            LOWER(LTRIM(RTRIM(SA.AliasName))),
                            ' ',
                            ''
                        ) =
                        REPLACE(
                            LOWER(LTRIM(RTRIM('{safeStateName}'))),
                            ' ',
                            ''
                        )
                ) AS StateResult
                ORDER BY MatchPriority";

                DataTable dt = SQLService.GetDataTable(query);

                if (dt.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "State not found.",
                        state_name_received = stateName
                    });
                }

                return new JsonResult(new
                {
                    status = true,
                    message = "State code fetched successfully.",
                    data = new
                    {
                        state_name = stateName,
                        state_code = dt.Rows[0]["StateCode"]?.ToString()
                    }
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
    }
}