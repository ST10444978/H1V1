using H1V1.Data;
using H1V1.Repositories.Implementations;
using H1V1.Repositories.Interfaces;
using H1V1.Services.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace H1V1
{
    public class Program
    {
        public static void Main(string[] args) // Fixed 'striang' typo
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Database Configuration
            builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. Repositories
            builder.Services.AddScoped<IAcademicRepository, AcademicRepository>();
            builder.Services.AddScoped<IPomodoroRepository, PomodoroRepository>();

            // 3. AI Services & Tools
            builder.Services.AddScoped<AgentToolService>();

            // 4. Configure MEAI Chat Client
            builder.Services.AddSingleton<IChatClient>(sp =>
            {
                string apiKey = builder.Configuration["Gemini:ApiKey"] ?? "YOUR_API_KEY";

                // 1. Create standard OpenAI/Gemini-compatible ChatClient
                // 2. Convert it into a Microsoft.Extensions.AI IChatClient using .AsChatClient()
                IChatClient baseClient = new ChatClient("gpt-4o-mini", apiKey).AsIChatClient();

                // Wrap with automatic function/tool invocation logic
                return new ChatClientBuilder(baseClient)
                    .UseFunctionInvocation()
                    .Build();
            });

            builder.Services.AddScoped<ChatAgentService>();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}