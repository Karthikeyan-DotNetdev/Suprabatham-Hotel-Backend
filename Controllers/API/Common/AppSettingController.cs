using AppSettings;
using CommonModels;
using CommonServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace laptop_service.Controllers.API
{
    [Route("api/app-settings")]
    [ApiController]
    public class AppSettingController : ControllerBase
    {

        [HttpGet]
        [Route("IsDataLogEnabled")]
        public IActionResult GetIsDataLogEnabled()
        {
            ApiResponse apiResponse = new ApiResponse();

            JObject jObject = new JObject();
            jObject.Add("IsDataLogEnabled", AppSetting.IsDataLogEnabled);

            apiResponse.status = true;
            apiResponse.data = jObject;

            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

        [HttpPut]
        [Route("IsDataLogEnabled/{IsDataLogEnabled}")]
        public IActionResult SetIsRequestDataLogEnabled(string IsDataLogEnabled = "0")
        {
            ApiResponse apiResponse = new ApiResponse();

            AppSetting.IsDataLogEnabled = IsDataLogEnabled;

            JObject jObject = new JObject();
            jObject.Add("IsDataLogEnabled", AppSetting.IsDataLogEnabled);

            apiResponse.status = true;
            apiResponse.data = jObject;

            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

    }
}
