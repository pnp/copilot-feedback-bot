using Common.Engine.Config;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;

namespace Common.Engine;

public static class BotUserUtils
{
    public static BotUser ParseBotUserInfo(this ChannelAccount user)
    {
        return string.IsNullOrEmpty(user.AadObjectId) ? new BotUser { IsAzureAdUserId = false, UserId = user.Id } : new BotUser { IsAzureAdUserId = true, UserId = user.AadObjectId };
    }

    public static async Task<BotUser> GetBotUserAsync(ITurnContext context, BotConfig _botConfig, GraphServiceClient _graphServiceClient)
    {
        return await GetBotUserAsync(context.Activity.From, _botConfig, _graphServiceClient);
    }
    public static async Task<BotUser> GetBotUserAsync(ChannelAccount channelUser, BotConfig botConfig, GraphServiceClient graphServiceClient)
    {
        // Testing hack
        BotUser botUser;
        if (!string.IsNullOrEmpty(botConfig.TestUPN))
        {
            var user = await graphServiceClient.Users[botConfig.TestUPN].GetAsync(op => op.QueryParameters.Select = ["Id"]);
            botUser = new BotUser { UserId = user!.Id!, IsAzureAdUserId = true };
        }
        else
            botUser = ParseBotUserInfo(channelUser);

        return botUser;
    }
}

public class BotUser
{
    public string UserId { get; set; } = string.Empty;
    public bool IsAzureAdUserId { get; set; } = false;
}