using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using laptop_service.Services.REPORTS;
using CommonModels;

namespace laptop_service.Controllers.REPORTS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/reports/salessummary")]
    [ApiController]
    public class SalesSummaryController : Controller
    {
        private readonly SalesSummaryService _salesSummaryService;

        public SalesSummaryController()
        {
            _salesSummaryService = new SalesSummaryService();
        }

        // ─────────────────────────────────────────────────────────────────────
        // 1. GET DAY SUMMARY REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("DaySummary")]
        public IActionResult GetDaySummary(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetDaySummary(branchCode, fromDate, toDate);
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
        // 2. GET BILL-WISE REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("BillWise")]
        public IActionResult GetBillWise(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetBillWiseReport(branchCode, fromDate, toDate);
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
        // 3. GET ITEM-WISE REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("ItemWise")]
        public IActionResult GetItemWise(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetItemWiseReport(branchCode, fromDate, toDate);
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
        // 4. GET CATEGORY-WISE REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("CategoryWise")]
        public IActionResult GetCategoryWise(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetCategoryWiseReport(branchCode, fromDate, toDate);
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
        // 5. GET ORDER TYPE REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("OrderType")]
        public IActionResult GetOrderType(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetOrderTypeReport(branchCode, fromDate, toDate);
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
        // 6. GET TOP PRODUCTS REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("TopProducts")]
        public IActionResult GetTopProducts(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetTopProductsReport(branchCode, fromDate, toDate);
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
        // 7. GET SALES SUMMARY REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("SalesSummary")]
        public IActionResult GetSalesSummary(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetSalesSummary(branchCode, fromDate, toDate);
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
        // 8. GET EXECUTIVE SALES SUMMARY REPORT
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("ExecutiveSalesSummary")]
        public IActionResult GetExecutiveSalesSummary(string branchCode, string fromDate, string toDate)
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

                var report = _salesSummaryService.GetExecutiveSalesSummary(branchCode, fromDate, toDate);
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
