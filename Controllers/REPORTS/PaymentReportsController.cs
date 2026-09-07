using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using laptop_service.Services.REPORTS;
using CommonModels;

namespace laptop_service.Controllers.REPORTS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/reports/payment")]
    [ApiController]
    public class PaymentReportsController : Controller
    {
        private readonly PaymentReportsService _paymentReportsService;

        public PaymentReportsController()
        {
            _paymentReportsService = new PaymentReportsService();
        }

        // ─────────────────────────────────────────────────────────────────────
        // 1. GET PAYMENT COLLECTION REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("Collection")]
        public IActionResult GetCollection(string branchCode, string fromDate, string toDate)
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

                var report = _paymentReportsService.GetPaymentCollection(branchCode, fromDate, toDate);
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
        // 2. GET PAYMENT MODE BREAKUP REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("ModeBreakup")]
        public IActionResult GetModeBreakup(string branchCode, string fromDate, string toDate)
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

                var report = _paymentReportsService.GetPaymentModeBreakup(branchCode, fromDate, toDate);
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
