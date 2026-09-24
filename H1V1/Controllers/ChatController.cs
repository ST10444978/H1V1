using H1V1.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace H1V1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatAgentService _agentService;

        public ChatController(ChatAgentService agentService) => _agentService = agentService;

        [HttpPost]
        public async Task<IActionResult> AskAgent([FromBody] ChatRequestDto request)
        {
            var reply = await _agentService.ProcessUserMessageAsync(request.StudentId, request.Prompt);
            return Ok(new { response = reply });
        }
    }

    public record ChatRequestDto(int StudentId, string Prompt);
}
