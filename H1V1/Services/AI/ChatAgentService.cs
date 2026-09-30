using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace H1V1.Services.AI
{
    public class ChatAgentService
    {
        private readonly AgentToolService _toolService;
        private readonly ILogger<ChatAgentService> _logger;
        private readonly Client _geminiClient;

        private const string ModelName = "gemini-3.8-flash";

        public ChatAgentService(
            AgentToolService toolService,
            ILogger<ChatAgentService> logger,
            IConfiguration configuration)
        {
            _toolService = toolService;
            _logger = logger;

            var apiKey = configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key was not found. Configure it using User Secrets with the key 'Gemini:ApiKey'.");
            }

            _geminiClient = new Client(apiKey: apiKey);
        }

        public async Task<string> ProcessUserMessageAsync(
            int studentId,
            string userPrompt)
        {
            _logger.LogInformation(
                "Processing prompt for Student {StudentId}: {Prompt}",
                studentId,
                userPrompt);

            try
            {
                var tools = CreateTools();

                var systemPrompt = $@"
                                    You are an Academic Success Agent helping student ID {studentId}.

                                    Your responsibilities are:
                                    - Help the student plan their academic work.
                                    - Track assessments and exams.
                                    - Generate personalised study plans.
                                    - Find relevant educational resources.
                                    - Use the student's database information whenever appropriate.

                                    IMPORTANT RULES:
                                    1. If the student asks for a study plan, first retrieve their pending assessments using GetPendingAssessments.
                                    2. Use the returned assessment information to create a realistic and structured study plan.
                                    3. After generating the study plan, save it using SaveStudyPlan.
                                    4. If the student asks to add or track an assessment, use TrackAssessment.
                                    5. If the student asks for resources, use SearchResources.
                                    6. Do not claim that an action was completed unless the corresponding tool was actually executed.
                                    7. Give the student a clear and friendly final response.

                                    The current student ID is {studentId}.
";

                var contents = new List<Content>
                {
                    new Content
                    {
                        Role = "user",
                        Parts =
                        [
                            new Part
                            {
                                Text = systemPrompt + "\n\nStudent request:\n" + userPrompt
                            }
                        ]
                    }
                };

                var config = new GenerateContentConfig
                {
                    Tools = tools
                };

                // First Gemini request
                var response = await _geminiClient.Models.GenerateContentAsync(
                    model: ModelName,
                    contents: contents,
                    config: config);

                // Process tool calls if Gemini requested them
                while (response.FunctionCalls != null &&
                       response.FunctionCalls.Count > 0)
                {
                    _logger.LogInformation(
                        "Gemini requested {Count} tool call(s).",
                        response.FunctionCalls.Count);

                    // IMPORTANT:
                    // Add Gemini's complete response to the conversation.
                    // This preserves the information Gemini needs for
                    // Gemini 3 function calling.
                    contents.Add(response.Candidates[0].Content);

                    var functionResponseParts = new List<Part>();

                    foreach (var functionCall in response.FunctionCalls)
                    {
                        var functionName = functionCall.Name;
                        var args = functionCall.Args;

                        _logger.LogInformation(
                            "Executing Gemini tool: {FunctionName}",
                            functionName);

                        var result = await ExecuteToolAsync(
                            functionName,
                            args,
                            studentId);

                        functionResponseParts.Add(
                            new Part
                            {
                                FunctionResponse = new FunctionResponse
                                {
                                    Name = functionName,
                                    Response = new Dictionary<string, object>
                                    {
                                        ["result"] = result
                                    }
                                }
                            });
                    }

                    // Send tool results back to Gemini
                    contents.Add(
                        new Content
                        {
                            Role = "user",
                            Parts = functionResponseParts
                        });

                    response = await _geminiClient.Models.GenerateContentAsync(
                        model: ModelName,
                        contents: contents,
                        config: config);
                }

                _logger.LogInformation(
                    "Gemini successfully completed execution for Student {StudentId}.",
                    studentId);

                return response.Text ??
                       "I completed the request, but I couldn't generate a final response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing Gemini request for Student {StudentId}.",
                    studentId);

                Console.WriteLine("========== GEMINI ERROR ==========");
                Console.WriteLine($"Type: {ex.GetType().FullName}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                Console.WriteLine("==================================");

                throw;
            }
        }

        private List<Tool> CreateTools()
        {
            return
            [
                new Tool
                {
                    FunctionDeclarations =
                    [
                        new FunctionDeclaration
                        {
                            Name = "GetPendingAssessments",
                            Description =
                                "Retrieves all upcoming, incomplete assessments and exams for a student.",
                            Parameters = new Schema
                            {
                                Type = "OBJECT",
                                Properties = new Dictionary<string, Schema>
                                {
                                    ["studentId"] = new Schema
                                    {
                                        Type = "INTEGER",
                                        Description = "The ID of the student."
                                    }
                                },
                                Required = ["studentId"]
                            }
                        },

                        new FunctionDeclaration
                        {
                            Name = "TrackAssessment",
                            Description =
                                "Adds a new exam, assignment, or project to track for a student.",
                            Parameters = new Schema
                            {
                                Type = "OBJECT",
                                Properties = new Dictionary<string, Schema>
                                {
                                    ["studentId"] = new Schema
                                    {
                                        Type = "INTEGER",
                                        Description = "The ID of the student."
                                    },
                                    ["title"] = new Schema
                                    {
                                        Type = "STRING",
                                        Description = "The assessment title."
                                    },
                                    ["courseCode"] = new Schema
                                    {
                                        Type = "STRING",
                                        Description = "The course code."
                                    },
                                    ["dueDate"] = new Schema
                                    {
                                        Type = "STRING",
                                        Description = "The assessment due date in ISO format."
                                    }
                                },
                                Required =
                                [
                                    "studentId",
                                    "title",
                                    "courseCode",
                                    "dueDate"
                                ]
                            }
                        },

                        new FunctionDeclaration
                        {
                            Name = "SaveStudyPlan",
                            Description =
                                "Saves a generated study plan to the database for future reference.",
                            Parameters = new Schema
                            {
                                Type = "OBJECT",
                                Properties = new Dictionary<string, Schema>
                                {
                                    ["studentId"] = new Schema
                                    {
                                        Type = "INTEGER",
                                        Description = "The ID of the student."
                                    },
                                    ["title"] = new Schema
                                    {
                                        Type = "STRING",
                                        Description = "The study plan title."
                                    },
                                    ["planMarkdown"] = new Schema
                                    {
                                        Type = "STRING",
                                        Description = "The complete study plan in Markdown."
                                    }
                                },
                                Required =
                                [
                                    "studentId",
                                    "title",
                                    "planMarkdown"
                                ]
                            }
                        },

                        new FunctionDeclaration
                        {
                            Name = "SearchResources",
                            Description =
                                "Finds educational materials, articles, and video links for a subject.",
                            Parameters = new Schema
                            {
                                Type = "OBJECT",
                                Properties = new Dictionary<string, Schema>
                                {
                                    ["topic"] = new Schema
                                    {
                                        Type = "STRING",
                                        Description = "The academic topic to search for."
                                    }
                                },
                                Required = ["topic"]
                            }
                        }
                    ]
                }
            ];
        }

        private async Task<string> ExecuteToolAsync(
            string functionName,
            Dictionary<string, object> args,
            int studentId)
        {
            switch (functionName)
            {
                case "GetPendingAssessments":
                    {
                        return await _toolService.GetPendingAssessments(studentId);
                    }

                case "TrackAssessment":
                    {
                        var title = GetStringArgument(args, "title");
                        var courseCode = GetStringArgument(args, "courseCode");
                        var dueDateString = GetStringArgument(args, "dueDate");

                        if (!DateTime.TryParse(dueDateString, out var dueDate))
                        {
                            return "The supplied due date was invalid.";
                        }

                        return await _toolService.TrackAssessment(
                            studentId,
                            title,
                            courseCode,
                            dueDate);
                    }

                case "SaveStudyPlan":
                    {
                        var title = GetStringArgument(args, "title");
                        var planMarkdown = GetStringArgument(args, "planMarkdown");

                        return await _toolService.SaveStudyPlan(
                            studentId,
                            title,
                            planMarkdown);
                    }

                case "SearchResources":
                    {
                        var topic = GetStringArgument(args, "topic");

                        return await _toolService.SearchResources(topic);
                    }

                default:
                    return $"Unknown tool requested: {functionName}";
            }
        }

        private static string GetStringArgument(
            Dictionary<string, object> args,
            string key)
        {
            if (!args.TryGetValue(key, out var value))
            {
                return string.Empty;
            }

            return value?.ToString() ?? string.Empty;
        }
    }
}

//using Microsoft.Extensions.AI;
//using Microsoft.Extensions.Logging;

//namespace H1V1.Services.AI
//{
//    public class ChatAgentService
//    {
//        private readonly IChatClient _chatClient;
//        private readonly AgentToolService _toolService;
//        private readonly ILogger<ChatAgentService> _logger;

//        public ChatAgentService(IChatClient chatClient, AgentToolService toolService, ILogger<ChatAgentService> logger)
//        {
//            _chatClient = chatClient;
//            _toolService = toolService;
//            _logger = logger;
//        }

//        public async Task<string> ProcessUserMessageAsync(int studentId, string userPrompt)
//        {
//            _logger.LogInformation("Processing prompt for Student {StudentId}: {Prompt}", studentId, userPrompt);

//            try
//            {
//                // Using List<AITool> prevents type conversion errors with AIFunction
//                var tools = new List<AITool>
//                {
//                    AIFunctionFactory.Create(_toolService.GetPendingAssessments),
//                    AIFunctionFactory.Create(_toolService.TrackAssessment),
//                    AIFunctionFactory.Create(_toolService.SaveStudyPlan),
//                    AIFunctionFactory.Create(_toolService.SearchResources)
//                };

//                var systemPrompt = $@"
//                    You are an Academic Success Agent helping student ID {studentId}.
//                    Your goal is to assist students with schedule planning, resource discovery, and assessment tracking.
//                    Always use tools to interact with the student's database.
//                    If asked to generate a study plan, first fetch their pending assessments, design a detailed plan, and save it using the SaveStudyPlan tool.
//                    Maintain a supportive and structured tone.";

//                var messages = new List<ChatMessage>
//                {
//                    new ChatMessage(ChatRole.System, systemPrompt),
//                    new ChatMessage(ChatRole.User, userPrompt)
//                };

//                var options = new ChatOptions
//                {
//                    Tools = tools
//                };

//                var response = await _chatClient.GetResponseAsync(messages, options);
//                _logger.LogInformation("Agent successfully completed execution.");

//                return response.Text ?? "Operation completed.";
//            }
//            catch (System.ClientModel.ClientResultException ex)
//            {
//                _logger.LogError(
//                    ex,
//                    "Gemini API request failed. Status: {Status}",
//                    ex.Status);

//                Console.WriteLine("========== GEMINI API ERROR ==========");
//                Console.WriteLine($"Status: {ex.Status}");
//                Console.WriteLine($"Message: {ex.Message}");

//                if (ex.GetRawResponse() != null)
//                {
//                    Console.WriteLine("Response:");
//                    Console.WriteLine(ex.GetRawResponse().Content.ToString());
//                }

//                Console.WriteLine("======================================");

//                throw;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error processing AI request");

//                Console.WriteLine("========== UNEXPECTED ERROR ==========");
//                Console.WriteLine(ex);
//                Console.WriteLine("======================================");

//                throw;
//            }
//        }
//    }
//}
