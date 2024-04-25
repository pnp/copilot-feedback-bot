using Azure.Core;
using Common.Engine.Config;
using Entities.DB;
using Entities.DB.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace ActivityImporter.Engine.Graph.GraphUser;

/// <summary>
/// Ensures user table info is upto-date from Graph
/// </summary>
public class UserMetadataUpdater : AbstractApiLoader
{
    #region Constructor & Privates

    private readonly AppConfig _appConfig;
    private readonly ManualGraphCallClient _httpClient;
    private readonly GraphServiceClient _graphServiceClient;
    private UserMetadataCache? _userMetaCache;
    private readonly GraphUserLoader _userLoader;

    public UserMetadataUpdater(AppConfig appConfig, ILogger telemetry, TokenCredential creds, ManualGraphCallClient manualGraphCallClient)
        : base(telemetry)
    {
        _graphServiceClient = new GraphServiceClient(creds);
        _appConfig = appConfig;

        // Override default
        _httpClient = manualGraphCallClient;
        _userLoader = new GraphUserLoader(_appConfig.ConnectionStrings.Redis, _appConfig.AuthConfig.TenantId, _httpClient, _telemetry);
    }

    public GraphUserLoader GraphUserLoader => _userLoader;

    #endregion

    /// <summary>
    /// Main method
    /// </summary>
    public async Task InsertAndUpdateDatabaseUsersFromGraph()
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlServer(_appConfig.ConnectionStrings.SQL);

        using (var db = new DataContext(optionsBuilder.Options))
        {
            _userMetaCache = new UserMetadataCache(db);

            _telemetry.LogInformation($"{DateTime.Now.ToShortTimeString()} User import - start");


            // If we have no active users, assume new install so clear delta key
            var activeUserCount = await db.Users.Where(u => u.AccountEnabled.HasValue && u.AccountEnabled.Value == true).CountAsync();
            if (activeUserCount == 0)
            {
                await _userLoader.ClearUserQueryDeltaCode();
            }

            // Load from Graph & update delta code once done
            var allActiveGraphUsers = await _userLoader.LoadAllActiveUsers();

            var allDbUsers = await db.Users.ToListAsync();
            var graphMentionedExistingDbUsers = GetDbUsersFromGraphUsers(allActiveGraphUsers, allDbUsers);

            // Insert any user we've not seen so far
            var insertedDbUsers = await InsertMissingUsers(db, allActiveGraphUsers, graphMentionedExistingDbUsers);
            var notInserted = allActiveGraphUsers.Where(
                u => !string.IsNullOrEmpty(u.UserPrincipalName) &&
                    !insertedDbUsers.Where(i => i.UserPrincipalName.ToLower() == u.UserPrincipalName.ToLower()).Any()).ToList();


            // Check existing users again Graph updates
            _telemetry.LogInformation($"User import - updating {notInserted.Count.ToString("N0")} existing users...");
            foreach (var existingGraphUser in notInserted)
            {
                var dbUser = graphMentionedExistingDbUsers.Where(u => u.UserPrincipalName.ToLower() == existingGraphUser.UserPrincipalName.ToLower()).SingleOrDefault();
                if (dbUser != null)
                {
                    await UpdateDbUserWithGraphData(existingGraphUser, allActiveGraphUsers, allDbUsers, dbUser);
                }
                else
                {
                    _telemetry.LogWarning($"User import - couldn't find existing user {existingGraphUser.UserPrincipalName} in DB");
                }
            }

            // Combine inserted & modified db users
            var allProcessedDbUsers = new List<User>(insertedDbUsers);
            var notInsertDbUsers = GetDbUsersFromGraphUsers(notInserted, graphMentionedExistingDbUsers);
            allProcessedDbUsers.AddRange(notInsertDbUsers);

            db.ChangeTracker.DetectChanges();
            await db.SaveChangesAsync();
            _telemetry.LogInformation($"{DateTime.Now.ToShortTimeString()} User import - inserted {insertedDbUsers.Count.ToString("N0")} new users and updated {notInserted.Count.ToString("N0")} from Graph API");
        }
    }


    private async Task UpdateDbUserWithGraphData(GraphUser graphUser, List<GraphUser> allGraphUsers, List<User> allDbUsers, User dbUser)
    {
        UpdateDbUserFromGraphUser(dbUser, graphUser);
        if (_userMetaCache == null)
        {
            throw new InvalidOperationException("UserMetadataCache not set");
        }

        var nameMaxLengthDepartment = Common.DataUtils.CommonStringUtils.EnsureMaxLength(graphUser.Department?.Trim(), 100);
        dbUser.Department = !string.IsNullOrEmpty(nameMaxLengthDepartment) ?
            await _userMetaCache.DepartmentCache.GetOrCreateNewResource(nameMaxLengthDepartment,
                new UserDepartment { Name = nameMaxLengthDepartment }) : null;

        var nameMaxLengthJobTitle = Common.DataUtils.CommonStringUtils.EnsureMaxLength(graphUser.JobTitle?.Trim(), 100);
        dbUser.JobTitle = !string.IsNullOrEmpty(nameMaxLengthJobTitle) ?
            await _userMetaCache.JobTitleCache.GetOrCreateNewResource(nameMaxLengthJobTitle,
                new UserJobTitle { Name = nameMaxLengthJobTitle }) : null;

        var nameMaxLengthOfficeLocation = Common.DataUtils.CommonStringUtils.EnsureMaxLength(graphUser.OfficeLocation?.Trim(), 100);
        dbUser.OfficeLocation = !string.IsNullOrEmpty(nameMaxLengthOfficeLocation) ?
            await _userMetaCache.OfficeLocationCache.GetOrCreateNewResource(nameMaxLengthOfficeLocation,
                new UserOfficeLocation { Name = nameMaxLengthOfficeLocation }) : null;

        var nameMaxLengthUsageLocation = Common.DataUtils.CommonStringUtils.EnsureMaxLength(graphUser.UsageLocation?.Trim(), 100);
        dbUser.UsageLocation = !string.IsNullOrEmpty(nameMaxLengthUsageLocation) ?
            await _userMetaCache.UseageLocationCache.GetOrCreateNewResource(nameMaxLengthUsageLocation,
                new UserUsageLocation { Name = nameMaxLengthUsageLocation }) : null;

        var nameMaxLengthCountry = Common.DataUtils.CommonStringUtils.EnsureMaxLength(graphUser.Country?.Trim(), 100);
        dbUser.UserCountry = !string.IsNullOrEmpty(nameMaxLengthCountry) ?
            await _userMetaCache.CountryOrRegionCache.GetOrCreateNewResource(nameMaxLengthCountry,
                new CountryOrRegion { Name = nameMaxLengthCountry }) : null;

        var nameMaxLengthCompany = Common.DataUtils.CommonStringUtils.EnsureMaxLength(graphUser.CompanyName?.Trim(), 100);
        dbUser.CompanyName = !string.IsNullOrEmpty(nameMaxLengthCompany) ?
            await _userMetaCache.CompanyNameCache.GetOrCreateNewResource(nameMaxLengthCompany,
                new CompanyName { Name = nameMaxLengthCompany }) : null;

        if (graphUser.DefaultManagerInfo?.Id != null)
        {
            // Try getting manager from DB 1st
            var dbManager = allDbUsers.Where(u => !string.IsNullOrEmpty(u.AzureAdId) && new Guid(u.AzureAdId).Equals(new Guid(graphUser.DefaultManagerInfo.Id))).FirstOrDefault();
            if (dbManager == null)
            {
                var graphManagerUser = allGraphUsers.FirstOrDefault(u => u.Id == graphUser.DefaultManagerInfo?.Id);

                if (graphManagerUser != null)
                {
                    // Got user from Graph cache; get DB user by UPN
                    var managerUpn = graphManagerUser.UserPrincipalName?.ToLower();

                    dbUser.Manager = !string.IsNullOrEmpty(managerUpn) ?
                        await _userMetaCache.UserCache.GetOrCreateNewResource(managerUpn,
                            new User { UserPrincipalName = managerUpn }, true) : null;

                }

                else
                {
                    _telemetry.LogInformation($"Couldn't find manager with AAD ID {graphUser.DefaultManagerInfo?.Id} in Graph cache or DB");
                }
            }
            else
            {
                dbUser.Manager = dbManager;
            }
        }
        dbUser.LastUpdated = DateTime.Now;
    }

    private List<User> GetDbUsersFromGraphUsers(List<GraphUser> allGraphUsers, List<User> allDbUsers)
    {
        var users = new List<User>();

        foreach (var graphUser in allGraphUsers)
        {
            // Do we have this graph user?
            var upn = graphUser.UserPrincipalName?.ToLower();
            if (!string.IsNullOrEmpty(upn))
            {
                var dbUser = allDbUsers.Where(u => u.UserPrincipalName.ToLower() == upn).SingleOrDefault();
                if (dbUser != null)
                {
                    users.Add(dbUser);
                }
            }
        }

        return users;
    }

    /// <summary>
    /// Inserts missing users into DB & calls UpdateDbUserWithGraphData
    /// </summary>
    private async Task<List<User>> InsertMissingUsers(DataContext db, List<GraphUser> allGraphUsers, List<User> graphMentionedDbUsers)
    {
        if (_userMetaCache == null)
        {
            throw new InvalidOperationException("UserMetadataCache not set");
        }

        _telemetry.LogInformation($"User import - Inserting missing users...");
        var usersInserted = new List<User>();

        // Build list of users to insert
        foreach (var graphUser in allGraphUsers)
        {
            // Do we have this graph user?
            var upn = graphUser.UserPrincipalName?.ToLower();
            if (!string.IsNullOrEmpty(upn) && !graphMentionedDbUsers.Where(u => u.UserPrincipalName.ToLower() == upn).Any())
            {
                // Lookup manager will just add to cache but not to context
                var dbUser = await _userMetaCache.UserCache.GetOrCreateNewResource(upn, UpdateDbUserFromGraphUser(new User { UserPrincipalName = upn }, graphUser));
                usersInserted.Add(dbUser);
            }
        }

        // Update too each user. 
        int i = 0;
        _telemetry.LogInformation($"User import - Loading metadata for {usersInserted.Count.ToString("N0")} new users...");

        foreach (var newDbUser in usersInserted)
        {
            var graphUser = allGraphUsers.Where(u => u.UserPrincipalName.ToLower() == newDbUser.UserPrincipalName).First();
            await UpdateDbUserWithGraphData(graphUser, allGraphUsers, graphMentionedDbUsers, newDbUser);

            if (i > 0 && i % 1000 == 0)
            {
                Console.WriteLine($"New user {i}/{usersInserted.Count.ToString("N0")} processed for lookups.");
            }
            i++;
        }

        db.Users.AddRange(usersInserted);

        Console.WriteLine($"User import - Saving {usersInserted.Count.ToString("N0")} new users to SQL...");
        await db.SaveChangesAsync();

        return usersInserted;
    }

    private User UpdateDbUserFromGraphUser(User dbUser, GraphUser graphUser)
    {
        dbUser.AccountEnabled = graphUser.AccountEnabled;
        dbUser.PostalCode = graphUser.PostalCode;
        dbUser.AzureAdId = graphUser.Id ?? throw new ArgumentNullException(nameof(graphUser.Id));

        return dbUser;
    }
}
