using Microsoft.AspNetCore.Mvc;
using laptop_service.Models;
using laptop_service.Services;

namespace laptop_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly PrintService _printService;
        private readonly ILogger<PrintController> _logger;

        public PrintController(PrintService printService, ILogger<PrintController> logger)
        {
            _printService = printService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<PrintRequest> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return BadRequest(new { status = false, message = "Request body is empty" });
            }

            var request = requests[0];
            _logger.LogInformation("Received print request in controller.");

            try
            {
                bool success = await _printService.ProcessPrintRequestAsync(request);
                if (success)
                {
                    return Ok(new { status = true, message = "Print job dispatched successfully" });
                }
                else
                {
                    return StatusCode(500, new { status = false, message = "Failed to dispatch print job. Check printer settings/network connection." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during print controller processing");
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }
    }
}
