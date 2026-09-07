using Microsoft.AspNetCore.Mvc;
using laptop_service.Models.AppSettings;
using laptop_service.Services.AppSettings;

namespace laptop_service.Controllers.AppSettings
{
    [ApiController]
    [Route("api/[controller]")]
    public class KOTPrinterSettingController : ControllerBase
    {
        private readonly KOTPrinterSettingService _printerService;
        private readonly ILogger<KOTPrinterSettingController> _logger;

        public KOTPrinterSettingController(KOTPrinterSettingService printerService, ILogger<KOTPrinterSettingController> logger)
        {
            _printerService = printerService;
            _logger = logger;
        }

        [HttpGet("list/{branchCode}")]
        public async Task<IActionResult> GetList(string branchCode)
        {
            try
            {
                var settingsList = await _printerService.GetKOTPrinterSettingsByBranchAsync(branchCode);
                return Ok(new { status = true, data = settingsList });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting KOT printer settings list for branch {BranchCode}", branchCode);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost("{branchCode}")]
        public async Task<IActionResult> Post(string branchCode, [FromBody] KOTPrinterSetting settings)
        {
            try
            {
                settings.Branch_Code = branchCode;
                await _printerService.SaveKOTPrinterSettingAsync(settings, "Admin");
                return Ok(new { status = true, message = "KOT printer setting saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving KOT printer settings for branch {BranchCode}", branchCode);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}
