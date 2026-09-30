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

        public ChatController(
            ChatAgentService agentService,
            ILogger<ChatController> logger)
        {
            _agentService = agentService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AskAgent(
            [FromBody] ChatRequestDto request)
        {
            // Validate request
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                _logger.LogWarning(
                    "Chat request rejected because the prompt was empty.");

                return BadRequest(new
                {
                    response = "Please provide a valid prompt."
                });
            }

            // Temporary fallback until authentication is connected.
            // This ensures the AI always receives a valid student ID.
            int studentId = request.StudentId <= 0
                ? 1
                : request.StudentId;

            _logger.LogInformation(
                "Received chat request for StudentId {StudentId}. Prompt length: {PromptLength}",
                studentId,
                request.Prompt.Length);

            try
            {
                // IMPORTANT:
                // ChatAgentService expects:
                // ProcessUserMessageAsync(string userMessage, int studentId)
                var reply = await _agentService.ProcessUserMessageAsync(
                    request.Prompt,
                    studentId);

                _logger.LogInformation(
                    "Chat request completed successfully for StudentId {StudentId}. Response length: {ResponseLength}",
                    studentId,
                    reply?.Length ?? 0);

                return Ok(new
                {
                    response = reply
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing AI chat request for StudentId {StudentId}",
                    studentId);

                // Do NOT expose the exception details to the client.
                // The full exception is already available in the server logs.
                return StatusCode(500, new
                {
                    response =
                        "EduBuddy is currently having trouble processing your request. " +
                        "Please try again in a moment."
                });
            }
        }
    }

    public record ChatRequestDto(
        int StudentId,
        string Prompt);
}