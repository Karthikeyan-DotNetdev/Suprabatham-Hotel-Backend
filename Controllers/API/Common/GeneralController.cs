using CommonModels;
using CommonServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace laptop_service.Controllers.API
{
    [Authorize]
    [Route("api/general")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        [HttpGet]
        [Route("api-version")]
        public IActionResult ApiVersion()
        {
            ApiResponse apiResponse = new ApiResponse();

            JObject jObject = new JObject();
            jObject.Add("ApiVersion", GeneralService.ApiVersion());

            apiResponse.status = true;
            apiResponse.data = jObject;

            return Ok(UtilityService.ModelToJsonString(apiResponse));
        }

    }
}
