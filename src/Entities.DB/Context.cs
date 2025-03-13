using Entities.DB.Entities;
using Entities.DB.Entities.AuditLog;
using Entities.DB.Entities.Profiling;
using Entities.DB.Entities.SP;
using Entities.DB.Entities.UsageReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Entities.DB;

public abstract class CommonContext : DbContext
{
    public CommonContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<UserDepartment> UserDepartments { get; set; }

    public DbSet<StateOrProvince> StateOrProvinces { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CountryOrRegion> CountryOrRegions { get; set; }
    public DbSet<CompanyName> CompanyNames { get; set; }
    public DbSet<UserOfficeLocation> UserOfficeLocations { get; set; }
    public DbSet<UserJobTitle> UserJobTitles { get; set; }
    public DbSet<LicenseType> LicenseTypes { get; set; }
    public DbSet<UserLicenseTypeLookup> UserLicenseTypeLookups { get; set; }
}

public class DataContext : CommonContext
{
    #region Props

    public DbSet<IgnoredEvent> IgnoredAuditEvents { get; set; }

    public DbSet<SharePointEventMetadata> SharePointEvents { get; set; }

    public DbSet<ImportSiteFilter> ImportSiteFilters { get; set; }
    public DbSet<CommonAuditEvent> AuditEventsCommon { get; set; }

    public DbSet<UserUsageLocation> UserUsageLocations { get; set; }

    public DbSet<SPEventFileExtension> SharePointFileExtensions { get; set; }
    public DbSet<SPEventFileName> SharePointFileNames { get; set; }
    public DbSet<EventOperation> EventOperations { get; set; }
    public DbSet<SPEventType> SharePointEventType { get; set; }

    public DbSet<Site> Sites { get; set; }
    public DbSet<Url> Urls { get; set; }
    public DbSet<OnlineMeeting> OnlineMeetings { get; set; }

    public DbSet<GlobalTeamsUserUsageLog> TeamUserActivityLogs { get; set; }
    public DbSet<GlobalTeamsUserDeviceUsageLog> TeamsUserDeviceUsageLog { get; set; }
    public DbSet<YammerUserActivityLog> YammerUserActivityLogs { get; set; }
    public DbSet<YammerDeviceActivityLog> YammerDeviceActivityLogs { get; set; }

    public DbSet<AppPlatformUserActivityLog> AppPlatformUserUsageLog { get; set; }

    public DbSet<OutlookUsageActivityLog> OutlookUsageActivityLogs { get; set; }
    public DbSet<OneDriveUserActivityLog> OneDriveUserActivityLogs { get; set; }
    public DbSet<SharePointUserActivityLog> SharePointUserActivityLogs { get; set; }

    public DbSet<CopilotChat> CopilotChats { get; set; }
    public DbSet<CopilotEventMetadataFile> CopilotEventMetadataFiles { get; set; }
    public DbSet<CopilotEventMetadataMeeting> CopilotEventMetadataMeetings { get; set; }



    public DbSet<UserSurveyResponseDB> SurveyResponses { get; set; }
    public DbSet<UserSurveyResponseActivityType> SurveyResponseActivities { get; set; }

    public DbSet<UserSurveyResponseActivityType> SurveyResponseActivityTypes { get; set; }
    public DbSet<CopilotActivity> CopilotActivities { get; set; }
    public DbSet<CopilotActivityType> CopilotActivityTypes { get; set; }


    public DbSet<SurveyPageDB> SurveyPages { get; set; }
    public DbSet<SurveyQuestionDB> SurveyQuestions { get; set; }
    public DbSet<SurveyAnswerDB> SurveyAnswers { get; set; }

    #endregion

    /// <summary>
    /// Define model schema
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add unique indexes
        modelBuilder.Entity<Site>().HasIndex(b => b.UrlBase).IsUnique();

        modelBuilder.Entity<Url>()
         .HasIndex(t => new { t.FullUrl })
         .IsUnique();

        modelBuilder.Entity<SPEventFileName>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<SPEventType>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<SPEventFileExtension>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<EventOperation>()
         .HasIndex(t => new { t.Name })
         .IsUnique();

        modelBuilder.Entity<UserDepartment>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<UserJobTitle>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<UserOfficeLocation>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<StateOrProvince>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<CountryOrRegion>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<CompanyName>()
         .HasIndex(t => new { t.Name })
         .IsUnique();


        modelBuilder.Entity<User>()
         .HasIndex(t => new { t.UserPrincipalName })
         .IsUnique();


        modelBuilder.Entity<UserLicenseTypeLookup>()
         .HasIndex(t => new { t.LicenseTypeId, t.UserId })
         .IsUnique();

        modelBuilder.Entity<SurveyQuestionDB>()
         .HasIndex(t => new { t.QuestionId })
         .IsUnique();


        modelBuilder.Entity<CopilotActivityType>()
         .HasIndex(t => new { t.Name })
         .IsUnique();
        modelBuilder.Entity<CopilotActivity>()
         .HasIndex(t => new { t.Name })
         .IsUnique();

        modelBuilder.Entity<ImportSiteFilter>()
         .HasIndex(t => t.UrlBase)
         .IsUnique();

        modelBuilder.Entity<UserSurveyResponseDB>()
         .HasOne(f => f.User)
         .WithMany()
         .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }


    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    public async Task<bool> EnsureCreated()
    {
        return await Database.EnsureCreatedAsync();
    }
}


public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CopilotFeedbackDev;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new DataContext(optionsBuilder.Options);
    }
}

public class ProfilingContext : CommonContext
{
    public ProfilingContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<ActivitiesWeekly> ActivitiesWeekly { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivitiesWeekly>()
            .ToTable("ActivitiesWeekly", "profiling")
            .HasKey(a => new { a.Metric, a.MetricDate, a.UserID });

        modelBuilder.Entity<User>().ToTable("users", "dbo");
        modelBuilder.Entity<UserDepartment>().ToTable("user_departments", "dbo");
        modelBuilder.Entity<StateOrProvince>().ToTable("state_or_provinces", "dbo");
        modelBuilder.Entity<CountryOrRegion>().ToTable("user_country_or_region", "dbo");
        modelBuilder.Entity<CompanyName>().ToTable("company_names", "dbo");
        modelBuilder.Entity<UserOfficeLocation>().ToTable("user_office_locations", "dbo");
        modelBuilder.Entity<UserJobTitle>().ToTable("user_job_titles", "dbo");
        modelBuilder.Entity<UserLicenseTypeLookup>().ToTable("user_license_type_lookups", "dbo");
        modelBuilder.Entity<LicenseType>().ToTable("license_types", "dbo");

        modelBuilder.HasDefaultSchema("profiling");
    }
}
