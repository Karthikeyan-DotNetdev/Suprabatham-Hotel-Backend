using Microsoft.AspNetCore.Mvc;
using laptop_service.Models.AppSettings;
using laptop_service.Services.AppSettings;

namespace laptop_service.Controllers.AppSettings
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptPrinterSettingController : ControllerBase
    {
        private readonly ReceiptPrinterSettingService _printerService;
        private readonly ILogger<ReceiptPrinterSettingController> _logger;

        public ReceiptPrinterSettingController(ReceiptPrinterSettingService printerService, ILogger<ReceiptPrinterSettingController> logger)
        {
            _printerService = printerService;
            _logger = logger;
        }

        [HttpGet("{branchCode}/{tillCode}")]
        public async Task<IActionResult> Get(string branchCode, string tillCode)
        {
            try
            {
                var settings = await _printerService.GetReceiptPrinterSettingByBranchAndTillAsync(branchCode, tillCode);
                if (settings == null)
                {
                    return Ok(new { status = true, data = new ReceiptPrinterSetting { Branch_Code = branchCode, Till_Code = tillCode } });
                }
                return Ok(new { status = true, data = settings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt printer settings for branch {BranchCode} and till {TillCode}", branchCode, tillCode);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost("{branchCode}/{tillCode}")]
        public async Task<IActionResult> Post(string branchCode, string tillCode, [FromBody] ReceiptPrinterSetting settings)
        {
            try
            {
                settings.Branch_Code = branchCode;
                settings.Till_Code = tillCode;
                await _printerService.SaveReceiptPrinterSettingAsync(settings, "Admin");
                return Ok(new { status = true, message = "Receipt printer setting saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving receipt printer settings for branch {BranchCode} and till {TillCode}", branchCode, tillCode);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}
