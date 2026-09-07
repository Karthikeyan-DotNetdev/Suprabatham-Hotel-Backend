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
    [Route("api/category")]
    [ApiController]
    public class Category_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddCategory")]
        public async Task<IActionResult> AddCategory(M_CATEGORY_MASTER category)
        {
            try
            {
                JObject jobject = new JObject();

                category.Category_Name = category.Category_Name?.Trim();

                if (string.IsNullOrWhiteSpace(category.Category_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Category Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Category_Master
                WHERE UPPER(Category_Name)=UPPER('{category.Category_Name}')
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Category Name already exists.");
                    return Ok(jobject.ToString());
                }

                category = await LoadCategoryMaxCode(category);

                var data = new M_CATEGORY_MASTER
                {
                    Category_Code = category.Category_Code,
                    Category_Name = category.Category_Name.ToUpper(),
                    Is_Active = category?.Is_Active ?? "A",
                    Created_By = category.Created_By,
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
                    "Category_Master",
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
        [Route("UpdateCategory/{Category_Code}")]
        public IActionResult UpdateCategory(string Category_Code, M_CATEGORY_MASTER category)
        {
            try
            {
                JObject jobject = new JObject();

                category.Category_Name = category.Category_Name?.Trim();

                if (string.IsNullOrWhiteSpace(category.Category_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Category Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Category_Master
                WHERE UPPER(Category_Name)=UPPER('{category.Category_Name}')
                AND Category_Code<>'{Category_Code}'
                AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Category Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new M_CATEGORY_MASTER
                {
                    Category_Code = Category_Code,
                    Category_Name = category.Category_Name.ToUpper(),
                    Is_Active = category.Is_Active,
                    Updated_By = category.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Category_Master",
                    "Category_Code",
                    Category_Code,
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
        [Route("DeleteCategory/{Category_Code}")]
        public IActionResult DeleteCategory(string Category_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE Category_Master
            SET Is_Active='D'
            WHERE Category_Code='{Category_Code}'";

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
        [Route("CategoryList")]
        public IActionResult CategoryList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_CategoryList");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        private async Task<M_CATEGORY_MASTER> LoadCategoryMaxCode(M_CATEGORY_MASTER category)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
            ISNULL(MAX(CAST(REPLACE(Category_Code,'C','') AS INT)),0)+1
            FROM Category_Master");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                category.Category_Code = "C" + code.ToString().PadLeft(5, '0');
            }

            return category;
        }
    }
}