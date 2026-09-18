using CommonModels;
using CommonServices;
using laptop_service.Models.MASTERS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;

namespace laptop_service.Controllers.MASTERS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/roleaccess")]
    [ApiController]
    public class RoleAccessController : ControllerBase
    {
        private static string EscapeSql(string? value) =>
            (value ?? "").Trim().Replace("'", "''");

        // =========================================================================
        // 1. GET: /api/roleaccess/GetRoles (Returns all distinct roles in system)
        // =========================================================================
        [HttpGet]
        [Route("GetRoles")]
        public IActionResult GetRoles()
        {
            try
            {
                string query = @"
                    SELECT DISTINCT UPPER(TRIM(Role)) AS Role_Name 
                    FROM dbo.Employee_Master 
                    WHERE Role IS NOT NULL AND TRIM(Role) <> '' AND Is_Active = 'A'
                    UNION
                    SELECT 'ADMIN' UNION SELECT 'MANAGER' UNION SELECT 'CASHIER' UNION SELECT 'WAITER';
                ";
                DataTable dt = SQLService.GetDataTable(query);

                var rolesList = new List<string>();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string r = row["Role_Name"]?.ToString() ?? "";
                        if (!string.IsNullOrWhiteSpace(r) && !rolesList.Contains(r))
                            rolesList.Add(r);
                    }
                }

                return Ok(new { status = true, data = rolesList });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        // =========================================================================
        // 2. GET: /api/roleaccess/GetPermissions?role=CASHIER
        // =========================================================================
        [HttpGet]
        [Route("GetPermissions")]
        public IActionResult GetPermissions([FromQuery] string role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role))
                    role = "CASHIER";

                string safeRole = EscapeSql(role.ToUpper().Trim());

                // Auto seed if role permissions don't exist yet
                SQLService.ExecuteNonQuery($"EXEC dbo.SP_Seed_Role_Permissions @RoleName = '{safeRole}';");

                string query = $@"
                    SELECT 
                        Permission_Id,
                        Role_Name,
                        Category_Key,
                        Module_Key,
                        Field_Key,
                        Display_Name,
                        ISNULL(Description, '') AS Description,
                        CAST(Is_Allowed AS BIT) AS Is_Allowed,
                        Display_Order
                    FROM dbo.Role_Permission_Master
                    WHERE UPPER(TRIM(Role_Name)) = '{safeRole}'
                    ORDER BY 
                        CASE Category_Key 
                            WHEN 'DASHBOARD' THEN 1 
                            WHEN 'POS' THEN 2 
                            WHEN 'MASTERS' THEN 3 
                            WHEN 'REPORTS' THEN 4 
                            WHEN 'SETTINGS' THEN 5 
                            ELSE 6 
                        END,
                        Display_Order,
                        Display_Name;
                ";

                DataTable dt = SQLService.GetDataTable(query);
                var items = new List<RolePermissionItem>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        items.Add(new RolePermissionItem
                        {
                            Permission_Id = Convert.ToInt64(r["Permission_Id"]),
                            Role_Name = r["Role_Name"].ToString() ?? "",
                            Category_Key = r["Category_Key"].ToString() ?? "",
                            Module_Key = r["Module_Key"].ToString() ?? "",
                            Field_Key = r["Field_Key"].ToString() ?? "ACCESS",
                            Display_Name = r["Display_Name"].ToString() ?? "",
                            Description = r["Description"]?.ToString(),
                            Is_Allowed = Convert.ToBoolean(r["Is_Allowed"]),
                            Display_Order = Convert.ToInt32(r["Display_Order"])
                        });
                    }
                }

                return Ok(new { status = true, role = safeRole, data = items });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        // =========================================================================
        // 3. POST: /api/roleaccess/SavePermissions (Bulk Save by Admin)
        // =========================================================================
        [HttpPost]
        [Route("SavePermissions")]
        public IActionResult SavePermissions([FromBody] SaveRolePermissionsRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Role_Name) || request.Permissions == null || request.Permissions.Count == 0)
                {
                    return Ok(new { status = false, message = "Role Name and Permissions list are required." });
                }

                string safeRole = EscapeSql(request.Role_Name.ToUpper().Trim());
                string safeUser = EscapeSql(request.Updated_By);

                var sb = new StringBuilder();
                foreach (var p in request.Permissions)
                {
                    int isAllowedInt = p.Is_Allowed ? 1 : 0;
                    string safeCat = EscapeSql(p.Category_Key);
                    string safeMod = EscapeSql(p.Module_Key);
                    string safeFld = EscapeSql(p.Field_Key);

                    sb.AppendLine($@"
                        UPDATE dbo.Role_Permission_Master
                        SET Is_Allowed = {isAllowedInt},
                            Updated_By = '{safeUser}',
                            Updated_On = GETDATE()
                        WHERE UPPER(TRIM(Role_Name)) = '{safeRole}'
                          AND Category_Key = '{safeCat}'
                          AND Module_Key   = '{safeMod}'
                          AND Field_Key    = '{safeFld}';
                    ");
                }

                SQLService.ExecuteNonQuery(sb.ToString());

                return Ok(new
                {
                    status = true,
                    message = $"Permissions for role '{safeRole}' updated successfully.",
                    total_updated = request.Permissions.Count
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }

        // =========================================================================
        // 4. GET: /api/roleaccess/GetUserActivePermissions?roleName=CASHIER
        // =========================================================================
        [HttpGet]
        [Route("GetUserActivePermissions")]
        public IActionResult GetUserActivePermissions([FromQuery] string roleName)
        {
            try
            {
                string safeRole = EscapeSql(roleName?.ToUpper().Trim() ?? "CASHIER");
                string query = $@"
                    SELECT 
                        Category_Key,
                        Module_Key,
                        Field_Key,
                        CAST(Is_Allowed AS BIT) AS Is_Allowed
                    FROM dbo.Role_Permission_Master
                    WHERE UPPER(TRIM(Role_Name)) = '{safeRole}'
                      AND Is_Allowed = 1;
                ";

                DataTable dt = SQLService.GetDataTable(query);
                var activeKeys = new HashSet<string>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        string cat = r["Category_Key"].ToString() ?? "";
                        string mod = r["Module_Key"].ToString() ?? "";
                        string fld = r["Field_Key"].ToString() ?? "";
                        activeKeys.Add($"{cat}.{mod}.{fld}");
                    }
                }

                return Ok(new { status = true, role = safeRole, activePermissions = activeKeys });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
    }
}
