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
    [Route("api/productgroup")]
    [ApiController]
    public class ProductGroup_Controller : ControllerBase
    {
        // =========================================================
        // ADD PRODUCT GROUP
        // =========================================================
        [HttpPost]
        [Route("AddProductGroup")]
        public async Task<IActionResult> AddProductGroup([FromBody] M_PRODUCT_GROUP_MASTER group)
        {
            try
            {
                JObject jobject = new JObject();

                group.Group_Name = group.Group_Name?.Trim();
                group.Category_Code = group.Category_Code?.Trim();
                group.Created_By = group.Created_By?.Trim();

                if (string.IsNullOrWhiteSpace(group.Group_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Group Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(group.Category_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Please select a Category.");
                    return Ok(jobject.ToString());
                }

                string safeGroupName = group.Group_Name.Replace("'", "''");
                string duplicateQuery = $@"
                    SELECT COUNT(*)
                    FROM dbo.Product_Group_Master
                    WHERE UPPER(LTRIM(RTRIM(Group_Name))) = UPPER(LTRIM(RTRIM('{safeGroupName}')))
                      AND Category_Code = '{group.Category_Code}'
                      AND Is_Active <> 'D'";

                int duplicateCount = SQLService.ExecuteScalarQuery(duplicateQuery);
                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Group Name already exists for this category.");
                    return Ok(jobject.ToString());
                }

                group = await LoadGroupMaxCode(group);

                var data = new M_PRODUCT_GROUP_MASTER
                {
                    Group_Code = group.Group_Code,
                    Group_Name = group.Group_Name.ToUpper(),
                    Category_Code = group.Category_Code,
                    Display_Order = group.Display_Order ?? 0,
                    Is_Active = group?.Is_Active ?? "A",
                    Created_By = group.Created_By ?? "admin",
                    Created_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>()
                {
                    "Category_Name",
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    data,
                    "Product_Group_Master",
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

        // =========================================================
        // UPDATE PRODUCT GROUP
        // =========================================================
        [HttpPost]
        [Route("UpdateProductGroup/{Group_Code}")]
        public IActionResult UpdateProductGroup(string Group_Code, [FromBody] M_PRODUCT_GROUP_MASTER group)
        {
            try
            {
                JObject jobject = new JObject();

                group.Group_Name = group.Group_Name?.Trim();
                group.Category_Code = group.Category_Code?.Trim();
                group.Updated_By = group.Updated_By?.Trim();

                if (string.IsNullOrWhiteSpace(group.Group_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Group Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string safeGroupName = group.Group_Name.Replace("'", "''");
                string duplicateQuery = $@"
                    SELECT COUNT(*)
                    FROM dbo.Product_Group_Master
                    WHERE UPPER(LTRIM(RTRIM(Group_Name))) = UPPER(LTRIM(RTRIM('{safeGroupName}')))
                      AND Category_Code = '{group.Category_Code}'
                      AND Group_Code <> '{Group_Code}'
                      AND Is_Active <> 'D'";

                int duplicateCount = SQLService.ExecuteScalarQuery(duplicateQuery);
                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Group Name already exists for this category.");
                    return Ok(jobject.ToString());
                }

                var data = new M_PRODUCT_GROUP_MASTER
                {
                    Group_Code = Group_Code,
                    Group_Name = group.Group_Name.ToUpper(),
                    Category_Code = group.Category_Code,
                    Display_Order = group.Display_Order ?? 0,
                    Is_Active = group.Is_Active ?? "A",
                    Updated_By = group.Updated_By ?? "admin",
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Category_Name",
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Product_Group_Master",
                    "Group_Code",
                    Group_Code,
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

        // =========================================================
        // DELETE PRODUCT GROUP (SOFT DELETE)
        // =========================================================
        [HttpGet]
        [Route("DeleteProductGroup/{Group_Code}")]
        public IActionResult DeleteProductGroup(string Group_Code)
        {
            try
            {
                JObject jobject = new JObject();

                string query = $@"
                    UPDATE dbo.Product_Group_Master
                    SET Is_Active = 'D', Updated_On = GETDATE()
                    WHERE Group_Code = '{Group_Code}'";

                int rowsAffected = SQLService.ExecuteNonQuery(query);

                if (rowsAffected > 0)
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
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        // =========================================================
        // GET ALL GROUPS
        // =========================================================
        [HttpGet]
        [Route("GroupList")]
        public IActionResult GroupList()
        {
            try
            {
                ApiResponse response = new ApiResponse();
                DataTable dt = SQLService.GetDataTable("EXEC SP_ProductGroupMasterList");

                response.status = true;
                response.data = UtilityService.DataTableToJArray(dt);
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // GET GROUPS BY CATEGORY CODE (FOR DEPENDENT DROPDOWNS)
        // =========================================================
        [HttpGet]
        [Route("GroupByCategoryId/{Category_Code}")]
        public IActionResult GroupByCategoryId(string Category_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();
                string safeCatCode = Category_Code.Replace("'", "''");
                DataTable dt = SQLService.GetDataTable($"EXEC SP_ProductGroupByCategoryId @Category_Code = '{safeCatCode}'");

                response.status = true;
                response.data = UtilityService.DataTableToJArray(dt);
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // AUTO GENERATE GROUP CODE (G00001, G00002, ...)
        // =========================================================
        private async Task<M_PRODUCT_GROUP_MASTER> LoadGroupMaxCode(M_PRODUCT_GROUP_MASTER group)
        {
            DataTable dt = SQLService.GetDataTable(@"
                SELECT ISNULL(MAX(CAST(SUBSTRING(Group_Code, 2, LEN(Group_Code)) AS INT)), 0) AS MaxId
                FROM dbo.Product_Group_Master");

            int maxId = (dt.Rows.Count > 0 && dt.Rows[0]["MaxId"] != DBNull.Value)
                ? Convert.ToInt32(dt.Rows[0]["MaxId"])
                : 0;

            group.Group_Code = "G" + (maxId + 1).ToString("D5");
            return group;
        }
    }
}
