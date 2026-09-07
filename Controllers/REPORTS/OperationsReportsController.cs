using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using laptop_service.Services.REPORTS;
using CommonModels;

namespace laptop_service.Controllers.REPORTS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/reports/operations")]
    [ApiController]
    public class OperationsReportsController : Controller
    {
        private readonly OperationsReportsService _operationsReportsService;

        public OperationsReportsController()
        {
            _operationsReportsService = new OperationsReportsService();
        }

        // ─────────────────────────────────────────────────────────────────────
        // 1. GET PENDING KOTs
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("KOTPending")]
        public IActionResult GetKOTPending(string branchCode, string fromDate, string toDate)
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

                var report = _operationsReportsService.GetKOTPending(branchCode, fromDate, toDate);
                response.status = true;
                response.data = report;

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. GET KOT STATUS AUDIT LOG
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("KOTStatus")]
        public IActionResult GetKOTStatus(string branchCode, string fromDate, string toDate)
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

                var report = _operationsReportsService.GetKOTStatus(branchCode, fromDate, toDate);
                response.status = true;
                response.data = report;

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3. GET HOURLY SALES TREND
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("HourlyTrend")]
        public IActionResult GetHourlyTrend(string branchCode, string fromDate, string toDate)
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

                var report = _operationsReportsService.GetHourlySalesTrend(branchCode, fromDate, toDate);
                response.status = true;
                response.data = report;

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. GET TABLE-WISE REVENUE
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("TableWiseRevenue")]
        public IActionResult GetTableWiseRevenue(string branchCode, string fromDate, string toDate)
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

                var report = _operationsReportsService.GetTableWiseRevenue(branchCode, fromDate, toDate);
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
