using EndAgent_API.data;
using EndAgent_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EndAgent_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ErrorLogService _logService;

        public LogController(ErrorLogService logService)
        {
            _logService = logService;
        }

        [HttpPost("addlog")]
        public async Task<IActionResult> SaveLog([FromBody] ErrorLogRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }

            await _logService.WriteLogAsync(request);

            return Ok(new
            {
                Message = "Log saved successfully"
            });
        }
    }
}
