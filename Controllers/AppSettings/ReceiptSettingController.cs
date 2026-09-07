using Microsoft.AspNetCore.Mvc;
using laptop_service.Models.AppSettings;
using laptop_service.Services.AppSettings;

namespace laptop_service.Controllers.AppSettings
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptSettingController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ReceiptSettingController> _logger;

        public ReceiptSettingController(DatabaseService databaseService, ILogger<ReceiptSettingController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        [HttpGet("{branchCode}/{tillCode}")]
        public async Task<IActionResult> Get(string branchCode, string tillCode)
        {
            try
            {
                var settings = await _databaseService.GetReceiptSettingByTillAsync(branchCode, "DEFAULT");
                return Ok(new { status = true, data = settings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt settings for branch {BranchCode} till {TillCode}", branchCode, tillCode);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost("{branchCode}/{tillCode}")]
        public async Task<IActionResult> Post(string branchCode, string tillCode, [FromBody] ReceiptSetting settings)
        {
            try
            {
                settings.Branch_Code = branchCode;
                settings.Till_Code = "DEFAULT";
                await _databaseService.SaveReceiptSettingAsync(settings, "Admin");
                return Ok(new { status = true, message = "Receipt settings saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving receipt settings for branch {BranchCode} till {TillCode}", branchCode, tillCode);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}
