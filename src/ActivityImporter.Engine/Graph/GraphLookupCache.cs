using Common.DataUtils;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;

namespace ActivityImporter.Engine.Graph
{
    /// <summary>
    /// GraphServiceClient + caches
    /// </summary>
    public class TeamsLoadContext
    {
        public TeamsLoadContext(GraphServiceClient graphClient)
        {
            UserCache = new UserLookupCache(graphClient);
            GraphClient = graphClient;
        }
        public GraphServiceClient GraphClient { get; set; }
        public UserLookupCache UserCache { get; set; }
    }

    /// <summary>
    /// A Graph cache for an abstract resource.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GraphLookupCache<T> : ObjectByIdCache<T> where T : class
    {
        protected readonly GraphServiceClient graphClient;
        public GraphLookupCache(GraphServiceClient graphClient)
        {
            this.graphClient = graphClient;
        }
    }

    /// <summary>
    /// Graph cache for users
    /// </summary>
    public class UserLookupCache : GraphLookupCache<Microsoft.Graph.Models.User>
    {
        public UserLookupCache(GraphServiceClient graphClient) : base(graphClient) { }

        public override async Task<Microsoft.Graph.Models.User?> Load(string? id)
        {
            var drive = await graphClient.Me.Drive.GetAsync();
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            try
            {
                var user = await graphClient.Users[id].GetAsync();
                return user;
            }
            catch (ODataError ex)
            {
                if (ex.ResponseStatusCode == 404)
                {
                    Console.WriteLine($"Got {ex.ResponseStatusCode} finding user by ID '{id}'");
                    return null;
                }
                else
                {
                    Console.WriteLine($"Got unexepected exception {ex.Message} finding user by ID '{id}'");
                    throw;
                }
            }
        }
    }
}
