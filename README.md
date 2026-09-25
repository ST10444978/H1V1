# H1V1 - Academic Success Agent & Student Productivity API

## Project Overview
H1V1 is an ASP.NET Core web application and RESTful API designed to serve as an AI-powered Academic Success Agent. It helps students track their academic assessments, manage study schedules, discover curated educational resources, and log Pomodoro study focus sessions.

The system integrates Entity Framework Core with SQL Server for database persistence and leverages `Microsoft.Extensions.AI` to enable LLM tool calling (function invocation).

---

## Repository & Project Structure

Below is the directory tree and description of key files and components in the solution:

```
H1V1/
├── Controllers/                  # API Controllers handling HTTP requests
│   ├── AssessmentController.cs   # Endpoints for pending assessments and marking assessments complete
│   ├── ChatController.cs         # AI agent chat endpoint (/api/chat)
│   ├── PomodoroController.cs     # Focus session logging and stats endpoints
│   ├── AiAgentController.cs      # Additional agent/UI controller
│   ├── HomeController.cs         # Web views controller
│   └── ...
├── Data/                         # Database context and seed data configuration
│   ├── AppDbContext.cs           # Entity Framework DbContext with DbSets and model seeding
│   ├── Seeder.cs                 # Supplemental seed utility
│   └── Migrations/               # EF Core database migration files
├── Models/                       # Data models, DTOs, and domain entities
│   └── Entities/                 # Core entity definitions
│       ├── Student.cs            # Student entity model
│       ├── Assessment.cs         # Assessment entity model
│       ├── StudyPlan.cs          # Generated study plan entity model
│       ├── Resource.cs           # Curated study resource model
│       └── PomodoroSession.cs    # Focus session tracking model
├── Repositories/                 # Data access layer (Repository pattern)
│   ├── Interfaces/               # Repository interfaces (IAcademicRepository, IPomodoroRepository)
│   └── Implementations/          # Async EF Core implementations (AcademicRepository, PomodoroRepository)
├── Services/                     # Business logic and AI orchestration
│   └── AI/                       # AI Agent services and tool definitions
│       ├── AgentToolService.cs   # Functions exposed to LLM with [Description] metadata
│       └── ChatAgentService.cs    # Orchestrates IChatClient and tool function invocation
├── Program.cs                    # App startup, DI configuration, and middleware setup
└── appsettings.json              # Application configuration and connection strings
```

---

## Detailed File Summaries

### Data & Persistence
- **`Data/AppDbContext.cs`**: Defines Entity Framework Core `DbSet` collections for `Students`, `Assessments`, `StudyPlans`, `Resources`, and `PomodoroSessions`. Configures model relationships and seeds initial sample data (Demo Student and initial study resources) in `OnModelCreating`.
- **`Data/Migrations/`**: Contains Entity Framework Core migration history (`InitialCreate`) for generating and maintaining the database schema.

### Domain Entities (`Models/Entities/`)
- **`Student.cs`**: Represents a student entity containing ID, Name, Major, and navigation properties for study plans and assessments.
- **`Assessment.cs`**: Represents an assignment or exam associated with a student, including title, course code, due date, and completion status.
- **`StudyPlan.cs`**: Stores AI-generated markdown study schedules linked to a student.
- **`Resource.cs`**: Stores curated educational links and study materials categorized by topic.
- **`PomodoroSession.cs`**: Records individual study focus sessions with duration in minutes and completion timestamp.

### Repositories (`Repositories/`)
- **`IAcademicRepository.cs` / `AcademicRepository.cs`**: Handles database operations for academic items: retrieving pending assessments sorted by due date, adding new assessments, marking assessments completed, saving generated study plans, and searching resources by topic.
- **`IPomodoroRepository.cs` / `PomodoroRepository.cs`**: Handles database operations for focus sessions: logging new Pomodoro focus sessions and computing total accumulated focus minutes for a student.

### AI Agent & Services (`Services/AI/`)
- **`AgentToolService.cs`**: Encapsulates tool functions exposed to the AI agent (`GetPendingAssessments`, `TrackAssessment`, `SaveStudyPlan`, `SearchResources`). Annotates methods with `[Description(...)]` attributes so the LLM correctly selects and invokes tools.
- **`ChatAgentService.cs`**: Manages the AI interaction using `Microsoft.Extensions.AI`. Sets the system prompt for the Academic Success Agent, registers tools via `AIFunctionFactory.Create()`, sends prompts to `IChatClient`, and returns generated agent responses.

### Controllers (`Controllers/`)
- **`ChatController.cs`**: Exposes `POST /api/chat` accepting `ChatRequestDto(StudentId, Prompt)` and returning the AI agent response JSON `{ response }`.
- **`AssessmentController.cs`**: Exposes `GET /api/assessments/{studentId}` to retrieve pending assessments and `POST /api/assessments/complete/{id}` to mark an assessment as complete.
- **`PomodoroController.cs`**: Exposes `POST /api/pomodoro/log` to record a focus session and `GET /api/pomodoro/stats/{studentId}` to query total focus minutes.

### Configuration
- **`Program.cs`**: Configures dependency injection for database contexts, repositories, AI services (`AgentToolService`, `ChatAgentService`), and configures `IChatClient` with automatic function invocation enabled (`UseFunctionInvocation()`).
