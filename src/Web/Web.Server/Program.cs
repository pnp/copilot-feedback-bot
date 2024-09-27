using Common.Engine;
using Common.Engine.Notifications;
using Entities.DB;
using Microsoft.Bot.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Web.Bots;
using Web.Bots.Dialogues;

namespace Web;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var config = DependencyInjection.AddBotServices(builder.Services, builder.Configuration);

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
