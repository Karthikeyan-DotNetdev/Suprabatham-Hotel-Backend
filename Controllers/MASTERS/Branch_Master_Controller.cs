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
    [Route("api/branch")]
    [ApiController]
    public class Branch_Master_Controller : Controller
    {
        [HttpPost]
        [Route("AddBranch")]
        public async Task<IActionResult> AddBranch(M_BRANCH_MASTER branch)
        {
            try
            {
                JObject jobject = new JObject();

                branch.Branch_Name = branch.Branch_Name?.Trim();

                if (string.IsNullOrWhiteSpace(branch.Branch_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Branch Name cannot be empty.");
                    return Ok(jobject.ToString());
                }
                string checkQuery = $@"
SELECT COUNT(*)
FROM Branch_Master
WHERE UPPER(Branch_Name)=UPPER('{branch.Branch_Name}')
AND Is_Active='A'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Branch Name already exists.");
                    return Ok(jobject.ToString());
                }

                branch = await LoadBranchMaxCode(branch);

                var data = new M_BRANCH_MASTER
                {
                    Branch_Code = branch.Branch_Code,
                    Branch_Name = branch.Branch_Name.ToUpper(),

                    Phone_No = branch.Phone_No,
                    Mobile_No = branch.Mobile_No,

                    Email = branch.Email,
                    Website = branch.Website,

                    Address = branch.Address,
                    Pin_Code = branch.Pin_Code,

                    Country_Name = branch.Country_Name,
                    State_Name = branch.State_Name,
                    State_Code = branch.State_Code,
                    City = branch.City,

                    GST_UIN = branch.GST_UIN,
                    Service_Tax_No = branch.Service_Tax_No,
                    PAN_No = branch.PAN_No,
                    CIN_No = branch.CIN_No,
                    FSSAI_No = branch.FSSAI_No,

                    Is_Active = "A",

                    Created_By = branch.Created_By,
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
                    "Branch_Master",
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
        [Route("UpdateBranch/{Branch_Code}")]
        public IActionResult UpdateBranch(string Branch_Code, M_BRANCH_MASTER branch)
        {
            try
            {
                JObject jobject = new JObject();

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM Branch_Master
                WHERE Branch_Name='{branch.Branch_Name}'
                AND Branch_Code<>'{Branch_Code}'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Branch Name already exists.");
                    return Ok(jobject.ToString());
                }

                var data = new M_BRANCH_MASTER
                {
                    Branch_Code = Branch_Code,

                    Branch_Name = branch.Branch_Name.ToUpper(),

                    Phone_No = branch.Phone_No,
                    Mobile_No = branch.Mobile_No,

                    Email = branch.Email,
                    Website = branch.Website,

                    Address = branch.Address,
                    Pin_Code = branch.Pin_Code,

                    Country_Name = branch.Country_Name,
                    State_Name = branch.State_Name,
                    State_Code = branch.State_Code,
                    City = branch.City,

                    GST_UIN = branch.GST_UIN,
                    Service_Tax_No = branch.Service_Tax_No,
                    PAN_No = branch.PAN_No,
                    CIN_No = branch.CIN_No,
                    FSSAI_No = branch.FSSAI_No,

                    Is_Active = branch.Is_Active,

                    Updated_By = branch.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var ignoredColumns = new List<string>()
                {
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "Branch_Master",
                    "Branch_Code",
                    Branch_Code,
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
        [Route("DeleteBranch/{Branch_Code}")]
        public IActionResult DeleteBranch(string Branch_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE Branch_Master
            SET Is_Active = 'D'
            WHERE Branch_Code = '{Branch_Code}'";

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
        [Route("BranchList")]
        public IActionResult BranchList()
        {
            ApiResponse response = new ApiResponse();

            DataTable dt = SQLService.GetDataTable(@"
        EXEC SP_BranchList
    ");

            response.status = true;
            response.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(response);
        }

        [HttpGet]
        [Route("GetBranch/{Branch_Code}")]
        public IActionResult GetBranch(string Branch_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();
                DataTable dt = SQLService.GetDataTable($@"
    SELECT 
        Branch_Code,
        Branch_Name,
        Phone_No,
        Mobile_No,
        Email,
        Website,
        Address,
        Pin_Code,
        Country_Name,
        State_Name,
        State_Code,
        City, 
        GST_UIN,
        Service_Tax_No,
        PAN_No,
        CIN_No,
        FSSAI_No,
        Is_Active
    FROM Branch_Master 
    WHERE Branch_Code = '{Branch_Code}' 
    AND Is_Active = 'A'");
                response.status = true;
                response.data = UtilityService.DataTableToJArray(dt);
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse { status = false, message = ex.Message });
            }
        }

        private async Task<M_BRANCH_MASTER> LoadBranchMaxCode(M_BRANCH_MASTER branch)
        {
            DataTable dt = SQLService.GetDataTable(@"
            SELECT
            ISNULL(MAX(CAST(REPLACE(Branch_Code,'B','') AS INT)),0)+1
            FROM Branch_Master");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);
                branch.Branch_Code = "B" + code.ToString().PadLeft(5, '0');
            }

            return branch;
        }
    }
}