using H1V1.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace H1V1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatAgentService _agentService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatAgentService agentService, ILogger<ChatController> logger)
        {
            _agentService = agentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AskAgent([FromBody] ChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { response = "Please provide a valid prompt." });
            }

            int studentId = request.StudentId <= 0 ? 1 : request.StudentId;

            try
            {
                var reply = await _agentService.ProcessUserMessageAsync(studentId, request.Prompt);
                return Ok(new { response = reply });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat request");
                return Ok(new { response = $"I encountered an issue processing your request: {ex.Message}. Please try again later or check your API key configuration." });
            }
        }
    }

    public record ChatRequestDto(int StudentId, string Prompt);
}
