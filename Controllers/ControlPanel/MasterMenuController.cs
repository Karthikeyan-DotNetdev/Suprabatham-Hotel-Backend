using CommonModels;
using CommonServices;
using laptop_service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using WIN_BOT;

namespace laptop_service.Controllers.ControlPanel
{
    [Authorize]
    [Route("api/ControlPanelMenu")]
    [ApiController]
    public class MasterMenuController : ControllerBase
    {   

        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> Save(ControlPanelMenu controlPanelMenu)
        {
            JObject jobject = new JObject();

            if (string.IsNullOrWhiteSpace(controlPanelMenu.Display_Order))
            {
                jobject.Add("status", false);
                jobject.Add("message", "Display Order cannot be empty or whitespace only.");
                return Ok(jobject.ToString());
            }

            string checkDisplayOrderQuery = $"SELECT COUNT(*) FROM M_CONTROLPANEL_MENUGROUP WHERE Display_Order = '{controlPanelMenu.Display_Order}'";
            string checkQuery = "SELECT COUNT(*) FROM M_CONTROLPANEL_MENUGROUP WHERE HomeMenu='" + controlPanelMenu.HomeMenu + "'";
            int count = SQLService.ExecuteScalarQuery(checkQuery);
            int DisplayOrderCount = SQLService.ExecuteScalarQuery(checkDisplayOrderQuery);

            if (count > 0)
            {
                TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
                jobject.Add("status", false);
                jobject.Add("message", "" + textInfo.ToTitleCase(controlPanelMenu.HomeMenu?.ToLower() + " Menu Already Exist"));
            }

            else if (DisplayOrderCount > 0)
            {
                jobject.Add("status", false);
                jobject.Add("message", "Display Order already exists.");
                return Ok(jobject.ToString());
            }
            else
            {


                controlPanelMenu.HomeMenu = controlPanelMenu.HomeMenu?.Trim();
                controlPanelMenu.Display_Order = controlPanelMenu.Display_Order?.Trim();


                controlPanelMenu.groupIndex = await LoadGroupIndexMaxCode(controlPanelMenu);
                var controlPanel = new ControlPanelMenu
                {
                    HomeMenu = controlPanelMenu.HomeMenu?.ToUpper(),
                    groupIndex = controlPanelMenu.groupIndex?.ToUpper(),
                    Display_Order = controlPanelMenu.Display_Order,
                    Is_Active = controlPanelMenu?.Is_Active?.ToString() ?? "0",
                };
                //*** Important *** //
                var ignoredColumns = new List<string> { };
                var InsertQuery = SQLHelper.BuildInsertQuery(controlPanel, "M_CONTROLPANEL_MENUGROUP", ignoredColumns);
                int result = SQLService.ExecuteNonQuery(InsertQuery);
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
            }
            return Ok(jobject.ToString());
        }

        [HttpPost]
        [Route("update/{groupIndex}")]
        public async Task<IActionResult> UpdateUser(string groupIndex, ControlPanelMenu controlPanelMenu)
        {



            JObject jobject = new JObject();

            if (string.IsNullOrWhiteSpace(controlPanelMenu.Display_Order))
            {
                jobject.Add("status", false);
                jobject.Add("message", "Display Order cannot be empty or whitespace only.");
                return Ok(jobject.ToString());
            }


            string checkDisplayOrderQuery = $"SELECT COUNT(*) FROM M_CONTROLPANEL_MENUGROUP WHERE Display_Order = '{controlPanelMenu.Display_Order}'AND Display_Order != '{controlPanelMenu}'";
            int DisplayOrderCount = SQLService.ExecuteScalarQuery(checkDisplayOrderQuery);


            if (DisplayOrderCount > 0)
            {
                jobject.Add("status", false);
                jobject.Add("message", "Display Order already exists.");
                return Ok(jobject.ToString());
            }

            controlPanelMenu.HomeMenu = controlPanelMenu.HomeMenu?.Trim();
            controlPanelMenu.Display_Order = controlPanelMenu.Display_Order?.Trim();
            controlPanelMenu.groupIndex = groupIndex;
            var controlPanel = new ControlPanelMenu
            {
                HomeMenu = controlPanelMenu.HomeMenu?.ToUpper(),
                groupIndex = controlPanelMenu.groupIndex,
                Display_Order = controlPanelMenu.Display_Order,
                Is_Active = controlPanelMenu?.Is_Active?.ToString() ?? "0",
            };

            //*** Important *** //
            var ignoredColumns = new List<string> { };
            var UpdateQuery = SQLHelper.BuildUpdateQuery(controlPanel, "M_CONTROLPANEL_MENUGROUP", "groupIndex", controlPanelMenu.groupIndex, ignoredColumns);
            int result = SQLService.ExecuteNonQuery(UpdateQuery);
            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Updated Successfully");

            }
            else
            {
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Updated!");
            }
            return Ok(jobject.ToString());
        }

        private async Task<string> LoadGroupIndexMaxCode(ControlPanelMenu controlPanelMenu)
        {
            DataTable dtCode = SQLService.GetDataTable("select max(groupIndex) from M_CONTROLPANEL_MENUGROUP");
            if (dtCode.Rows.Count > 0)
            {
                string code = dtCode.Rows[0][0].ToString();
                if (code == "0")
                {
                    controlPanelMenu.groupIndex = "1";
                }
                else
                {
                    int maxcode = int.Parse(code) + 1;
                    code = maxcode.ToString();
                    controlPanelMenu.groupIndex = code;
                }
                return controlPanelMenu.groupIndex;
            }
            else
            {
                return controlPanelMenu.groupIndex = "1";
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("list")]
        public IActionResult List()
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            DataTable dtCode = SQLService.GetDataTable("get_ControlPanelMasterMenu");

            JArray jArray = UtilityService.DataTableToJArray(dtCode); ;

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jArray;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

        [HttpGet]
        [Route("{groupIndex}")]
        public IActionResult Get(string groupIndex)
        {
            // Define API response object
            ApiResponse apiResponse = new ApiResponse();

            // Identify the tenant & form the SQL connection string
            string ConStr = SqlService.GetTenantDatabaseConnectionStirng(HttpContext);

            // Fetch the data from SQL database
            DataTable dataTable = SQLService.GetDataTable("SELECT * FROM M_CONTROLPANEL_MENUGROUP WHERE groupIndex='" + groupIndex + "'");

            // Form the API return object
            JObject jObject = UtilityService.DataTableToJObject(dataTable);

            // Add the return data in response object
            apiResponse.status = true;
            apiResponse.data = jObject;

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

        [HttpGet]
        [Route("delete/{groupIndex}")]
        public IActionResult Delete(string groupIndex)
        {
            // Define API response object
            JObject jobject = new JObject();
            // Fetch the data from SQL database
            string query = "update M_CONTROLPANEL_MENUGROUP set Is_Active='D' where groupIndex='" + groupIndex + "'";
            int result = SQLService.ExecuteNonQuery(query);

            // Form the API return object
            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Deleted Successfully");
            }
            else
            {
                // Add the return data in response object
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Delete!");
            }

            // Return Action Result
            return Ok(UtilityService.ModelToJsonString(jobject));
        }
    }
}
