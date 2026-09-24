using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace H1V1.Services.AI
{
    public class ChatAgentService
    {
        private readonly IChatClient _chatClient;
        private readonly AgentToolService _toolService;
        private readonly ILogger<ChatAgentService> _logger;

        public ChatAgentService(IChatClient chatClient, AgentToolService toolService, ILogger<ChatAgentService> logger)
        {
            _chatClient = chatClient;
            _toolService = toolService;
            _logger = logger;
        }

        public async Task<string> ProcessUserMessageAsync(int studentId, string userPrompt)
        {
            _logger.LogInformation("Processing prompt for Student {StudentId}: {Prompt}", studentId, userPrompt);

            // Using List<AITool> prevents type conversion errors with AIFunction
            var tools = new List<AITool>
            {
                AIFunctionFactory.Create(_toolService.GetPendingAssessments),
                AIFunctionFactory.Create(_toolService.TrackAssessment),
                AIFunctionFactory.Create(_toolService.SaveStudyPlan),
                AIFunctionFactory.Create(_toolService.SearchResources)
            };

            var systemPrompt = $@"
                You are an Academic Success Agent helping student ID {studentId}.
                Your goal is to assist students with schedule planning, resource discovery, and assessment tracking.
                Always use tools to interact with the student's database.
                If asked to generate a study plan, first fetch their pending assessments, design a detailed plan, and save it using the SaveStudyPlan tool.
                Maintain a supportive and structured tone.";

            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, userPrompt)
            };

            var options = new ChatOptions
            {
                Tools = tools
            };

            var response = await _chatClient.GetResponseAsync(messages, options);
            _logger.LogInformation("Agent successfully completed execution.");

            // In Microsoft.Extensions.AI, response.Text is the top-level response string
            return response.Text ?? "Operation completed.";
        }
    }
}