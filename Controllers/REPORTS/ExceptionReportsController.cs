using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using laptop_service.Services.REPORTS;
using CommonModels;

namespace laptop_service.Controllers.REPORTS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/reports/exception")]
    [ApiController]
    public class ExceptionReportsController : Controller
    {
        private readonly ExceptionReportsService _exceptionReportsService;

        public ExceptionReportsController()
        {
            _exceptionReportsService = new ExceptionReportsService();
        }

        // ─────────────────────────────────────────────────────────────────────
        // 1. GET CANCELLED BILLS REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("CancelledBills")]
        public IActionResult GetCancelledBills(string branchCode, string fromDate, string toDate)
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

                var report = _exceptionReportsService.GetCancelledBillReport(branchCode, fromDate, toDate);
                response.status = true;
                response.data = report;

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = false, message = ex.Message });
            }
        }
    }
}
