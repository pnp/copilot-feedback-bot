using Azure;
using Common.Engine.Config;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System.Collections.Concurrent;

namespace Common.Engine;


public class BotConversationCache : TableStorageManager
{
    const string TABLE_NAME = "ConversationCache";
    private readonly GraphServiceClient _graphServiceClient;
    private ConcurrentDictionary<string, CachedUserAndConversationData> _userIdConversationCache = new();

    public BotConversationCache(GraphServiceClient graphServiceClient, AppConfig appConfig) : base(appConfig.ConnectionStrings.Storage)
    {
        _graphServiceClient = graphServiceClient;
        // Dev only: make sure the Azure Storage emulator is running or this will fail
    }

    public async Task PopulateMemCacheIfEmpty()
    {
        if (_userIdConversationCache.Count > 0) return;

        var client = await base.GetTableClient(TABLE_NAME);
        var queryResultsFilter = client.Query<CachedUserAndConversationData>(filter: $"PartitionKey eq '{CachedUserAndConversationData.PartitionKeyVal}'");
        foreach (var qEntity in queryResultsFilter)
        {
            _userIdConversationCache.AddOrUpdate(qEntity.RowKey, qEntity, (key, newValue) => qEntity);
            Console.WriteLine($"{qEntity.RowKey}: {qEntity}");
        }
    }

    public async Task RemoveFromCache(string aadObjectId)
    {
        CachedUserAndConversationData? u = null;
        if (_userIdConversationCache.TryGetValue(aadObjectId, out u))
        {
            _userIdConversationCache.TryRemove(aadObjectId, out u);
        }
        var client = await base.GetTableClient(TABLE_NAME);

        await client.DeleteEntityAsync(CachedUserAndConversationData.PartitionKeyVal, aadObjectId);
    }

    /// <summary>
    /// App installed for user & now we have a conversation reference to cache for future chat threads.
    /// </summary>
    public async Task AddConversationReferenceToCache(Activity activity, BotUser botUser)
    {
        var conversationReference = activity.GetConversationReference();
        await AddOrUpdateUserAndConversationId(conversationReference, botUser, activity.ServiceUrl, _graphServiceClient);
    }

    internal async Task AddOrUpdateUserAndConversationId(ConversationReference conversationReference, BotUser botUser, string serviceUrl, GraphServiceClient graphClient)
    {
        CachedUserAndConversationData? u = null;
        var client = await base.GetTableClient(TABLE_NAME);

        if (!_userIdConversationCache.TryGetValue(botUser.UserId, out u))
        {
            // Have not got in memory cache
            Response<CachedUserAndConversationData>? entityResponse = null;
            try
            {
                entityResponse = client.GetEntity<CachedUserAndConversationData>(CachedUserAndConversationData.PartitionKeyVal, botUser.UserId);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // No worries
            }

            if (entityResponse == null)
            {
                string? upn = null;
                if (botUser.IsAzureAdUserId)
                {
                    // Get UPN from Graph
                    var user = await graphClient.Users[botUser.UserId].GetAsync(op => op.QueryParameters.Select = ["userPrincipalName"]);
                    upn = user?.UserPrincipalName ?? throw new ArgumentNullException($"No userPrincipalName for {nameof(conversationReference.User.AadObjectId)} '{conversationReference.User.AadObjectId}'");
                }

                // Not in storage account either. Add there
                u = new CachedUserAndConversationData()
                {
                    RowKey = botUser.UserId,
                    ServiceUrl = serviceUrl,
                    UserPrincipalName = upn,
                };
                u.ConversationId = conversationReference.Conversation.Id;
                client.AddEntity(u);
            }
            else
            {
                u = entityResponse.Value;
            }
        }

        // Update memory cache
        _userIdConversationCache.AddOrUpdate(botUser.UserId, u, (key, newValue) => u);
    }


    public async Task<List<CachedUserAndConversationData>> GetCachedUsers()
    {
        await PopulateMemCacheIfEmpty();
        return _userIdConversationCache.Values.ToList();
    }

    public CachedUserAndConversationData? GetCachedUser(string aadObjectId)
    {
        return _userIdConversationCache.Values.Where(u => u.RowKey == aadObjectId).SingleOrDefault();
    }

    public bool ContainsUserId(string aadId)
    {
        return _userIdConversationCache.ContainsKey(aadId);
    }
}
