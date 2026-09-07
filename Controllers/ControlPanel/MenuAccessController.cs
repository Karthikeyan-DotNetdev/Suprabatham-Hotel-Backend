using CommonModels;
using CommonServices;
using laptop_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using WIN_BOT;

namespace laptop_service.Controllers.ControlPanel
{
    [Authorize]
    [Route("api/MenuAccess")]
    [ApiController]
    public class MenuAccessController : ControllerBase
    {

        [HttpPost]
        [Route("save/{id}")]
        public async Task<IActionResult> SaveUser(string id ,List<MenuAccess> menuAccess)
        {
            var result = 0;
            JObject jobject = new JObject();
            SqlConnection con = new SqlConnection(SQLService.connectionString);
            con.Open();
            SqlCommand cmdDel = new SqlCommand("DELETE from CONTROLPANEL where RoleID='" + id + "'", con);
            var delete = cmdDel.ExecuteNonQuery();
            foreach (var data in menuAccess)
            {
                string query = "insertControlPanel";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@roleid", id);
                cmd.Parameters.AddWithValue("@module", data.module);
                cmd.Parameters.AddWithValue("@formview", data.opFormView);
                cmd.Parameters.AddWithValue("@fullcontrol", data.opFullControl);
                cmd.Parameters.AddWithValue("@insert", data.opInsert);
                cmd.Parameters.AddWithValue("@edit", data.opEdit);
                cmd.Parameters.AddWithValue("@view", data.opView);
                cmd.Parameters.AddWithValue("@delete", data.opDelete);
                cmd.Parameters.AddWithValue("@export", data.opExport);
                cmd.Parameters.AddWithValue("@print", data.opPrint);
                result = cmd.ExecuteNonQuery();
            }
            con.Close();
            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Saved Successfully");

            }
            else
            {
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Saved!");
            }
            return Ok(jobject.ToString());
        }

        [HttpGet]
        [Route("{roleid}")]
        public IActionResult Get(string roleid)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Identify the tenant & form the SQL connection string
            string ConStr = SqlService.GetTenantDatabaseConnectionStirng(HttpContext);

            // Fetch the data from SQL database
            DataTable dataTable = SQLService.GetDataTable("getControlPanelData '" + roleid + "' ");

            // Form the API return object
            JArray jArray = UtilityService.DataTableToJArray(dataTable);

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jArray;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

        [HttpGet]
        [Route("accessMenu/{roleid}")]
        public IActionResult GetAccessMenu(string roleid)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Identify the tenant & form the SQL connection string
            string ConStr = SqlService.GetTenantDatabaseConnectionStirng(HttpContext);

            // Fetch the data from SQL database
            DataTable dataTable = SQLService.GetDataTable("get_AccessMenu  '" + roleid + "' ");

            // Form the API return object
            JArray jArray = UtilityService.DataTableToJArray(dataTable);

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jArray;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }
    }
}
