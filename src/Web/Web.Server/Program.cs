using Common.Engine;
using Common.Engine.Notifications;
using Entities.DB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Web.Bots;
using Web.Bots.Dialogues;
using Microsoft.Identity.Web;

namespace Web;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var config = DependencyInjection.AddBotServices(builder.Services, builder.Configuration);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(ops => 
            {
                ops.Audience = "api://localhost:5173/5023a8dc-8448-4f41-b34c-131ee03def2f";
            }, o => 
            { 
                o.ClientId = config.AuthConfig.ClientId; 
                o.TenantId = config.AuthConfig.TenantId;
                o.ClientSecret = config.AuthConfig.ClientSecret;
                o.Authority = $"https://login.microsoftonline.com/{config.AuthConfig.TenantId}";
                o.Instance = "https://login.microsoftonline.com/";
            });

        // Bot services --->
        // Create the Bot Framework Adapter with error handling enabled.


        // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
        builder.Services.AddSingleton<IStorage, MemoryStorage>();

        // Create the User state. (Used in this bot's Dialog implementation.)
        builder.Services.AddSingleton<UserState>();

        // Create the Conversation state. (Used by the Dialog system itself.)
        builder.Services.AddSingleton<ConversationState>();

        // Bot diags
        builder.Services.AddSingleton<SurveyDialogue>();
        builder.Services.AddSingleton<StopBotheringMeDialogue>();

        builder.Services.AddSingleton<BotActionsHelper>();
        builder.Services.AddSingleton<BotAppInstallHelper>();

        builder.Services.AddTransient<IBot, CopilotFeedbackBot<SurveyDialogue>>();
        builder.Services.AddSingleton<IConversationResumeHandler, SurveyConversationResumeHandler>();

#if !DEBUG
        builder.Services.AddApplicationInsightsTelemetry();
#endif
        var app = builder.Build();

#if DEBUG

        // Clear out the bot cache for dev testing. That way we get 1st time user experience every time.
        var graph = app.Services.GetRequiredService<GraphServiceClient>();
        var botCache = new BotConversationCache(graph, config);
        var allUsers = await botCache.GetCachedUsers();
        foreach (var user in allUsers)
        {
            await botCache.RemoveFromCache(user.RowKey);
        }
#endif

        // Ensure DB
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlServer(config.ConnectionStrings.SQL);
        using (var db = new DataContext(optionsBuilder.Options))
        {
            var logger = LoggerFactory.Create(config =>
            {
                config.AddConsole();
            }).CreateLogger("Import console");
            await DbInitialiser.EnsureInitialised(db, logger, config.TestUPN);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
