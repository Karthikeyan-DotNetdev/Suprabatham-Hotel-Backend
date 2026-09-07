using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using laptop_service.Services.REPORTS;
using CommonModels;

namespace laptop_service.Controllers.REPORTS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/reports/dashboard")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;

        public DashboardController()
        {
            _dashboardService = new DashboardService();
        }

        [HttpGet]
        [Route("summary")]
        public IActionResult GetDashboardSummary(string branchCode, string fromDate, string toDate)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                if (string.IsNullOrWhiteSpace(branchCode))
                {
                    response.status = false;
                    response.message = "Branch Code is required.";
                    return new JsonResult(response);
                }

                if (string.IsNullOrWhiteSpace(fromDate) || string.IsNullOrWhiteSpace(toDate))
                {
                    response.status = false;
                    response.message = "Date range is required.";
                    return new JsonResult(response);
                }

                var summary = _dashboardService.GetDashboardSummary(branchCode, fromDate, toDate);
                response.status = true;
                response.data = summary;

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = false, message = ex.Message });
            }
        }
    }
}
