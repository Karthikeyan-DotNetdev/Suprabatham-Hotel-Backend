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
    [Route("api/role")]
    [ApiController]
    public class Role_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddRole")]
        public async Task<IActionResult> AddRole(M_ROLE_MASTER role)
        {
            try
            {
                JObject jobject = new JObject();

                role.Role_Name = role.Role_Name?.Trim();

                if (string.IsNullOrWhiteSpace(role.Role_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Role Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Role_Master
                WHERE UPPER(Role_Name)=UPPER('{role.Role_Name}')
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Role Name already exists.");
                    return Ok(jobject.ToString());
                }

                role = await LoadRoleMaxCode(role);

                var data = new M_ROLE_MASTER
                {
                    Role_Code = role.Role_Code,
                    Role_Name = role.Role_Name.ToUpper(),
                    Is_Active = role?.Is_Active ?? "A",
                    Created_By = role.Created_By,
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
                    "Role_Master",
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
        [Route("UpdateRole/{Role_Code}")]
        public IActionResult UpdateRole(string Role_Code, M_ROLE_MASTER role)
        {
            try
            {
                JObject jobject = new JObject();

                role.Role_Name = role.Role_Name?.Trim();

                if (string.IsNullOrWhiteSpace(role.Role_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Role Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Role_Master
                WHERE UPPER(Role_Name)=UPPER('{role.Role_Name}')
                AND Role_Code<>'{Role_Code}'
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Role Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new M_ROLE_MASTER
                {
                    Role_Code = Role_Code,
                    Role_Name = role.Role_Name.ToUpper(),
                    Is_Active = role.Is_Active,
                    Updated_By = role.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Role_Master",
                    "Role_Code",
                    Role_Code,
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
        [Route("DeleteRole/{Role_Code}")]
        public IActionResult DeleteRole(string Role_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE Role_Master
            SET Is_Active='D'
            WHERE Role_Code='{Role_Code}'";

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
        [Route("RoleList")]
        public IActionResult RoleList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable(@"
            EXEC SP_RoleList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        private async Task<M_ROLE_MASTER> LoadRoleMaxCode(M_ROLE_MASTER role)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
            ISNULL(MAX(CAST(REPLACE(Role_Code,'R','') AS INT)),0)+1
            FROM Role_Master");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                role.Role_Code = "R" + code.ToString().PadLeft(5, '0');
            }

            return role;
        }
    }
}