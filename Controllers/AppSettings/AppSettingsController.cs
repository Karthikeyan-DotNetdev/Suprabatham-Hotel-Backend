using Microsoft.AspNetCore.Mvc;
using laptop_service.Services.AppSettings;

namespace laptop_service.Controllers.AppSettings
{
    public class BranchSettingsRequest
    {
        public Dictionary<string, string?> Settings { get; set; } = new();
    }

    [ApiController]
    [Route("api/branchsettings")]
    public class AppSettingsController : ControllerBase
    {
        private readonly AppSettingsService _service;
        private readonly ILogger<AppSettingsController> _logger;

        private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
        {
            "BILLING", "ORDER", "OPERATIONS", "KITCHEN", "PAYMENT", "DISPLAY", "SECURITY", "INTEGRATION", "REPORTS", "NOTIFICATION"
        };

        public AppSettingsController(AppSettingsService service, ILogger<AppSettingsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{branchCode}/{category}")]
        public async Task<IActionResult> Get(string branchCode, string category)
        {
            if (!AllowedCategories.Contains(category))
                return BadRequest(new { status = false, message = $"Unknown category '{category}'" });

            try
            {

                var settings = await _service.GetSettingsAsync(branchCode, category.ToUpper());
                return Ok(new { status = true, data = settings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting settings for branch {BranchCode} category {Category}", branchCode, category);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }

        [HttpPost("{branchCode}/{category}")]
        public async Task<IActionResult> Post(
            string branchCode,
            string category,
            [FromBody] BranchSettingsRequest request)
        {
            if (!AllowedCategories.Contains(category))
                return BadRequest(new { status = false, message = $"Unknown category '{category}'" });

            if (request == null || request.Settings == null || request.Settings.Count == 0)
                return BadRequest(new { status = false, message = "Settings payload cannot be empty." });

            try
            {

                await _service.SaveSettingsAsync(branchCode, category.ToUpper(), request.Settings, "Admin");
                return Ok(new { status = true, message = $"{category} settings saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings for branch {BranchCode} category {Category}", branchCode, category);
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}
