using Entities.DB;
using Entities.DB.LookupCaches.Discrete;

namespace ActivityImporter.Engine.Graph.GraphUser;

/// <summary>
/// Metadata cache for user records
/// </summary>
internal class UserMetadataCache
{
    public UserDepartmentCache DepartmentCache { get; set; }
    public UserJobTitleCache JobTitleCache { get; set; }
    public OfficeLocationCache OfficeLocationCache { get; set; }
    public UsageLocationCache UseageLocationCache { get; set; }
    public StateOrProvinceCache StateOrProvinceCache { get; set; }

    public CountryOrRegionCache CountryOrRegionCache { get; set; }
    public CompanyNameCache CompanyNameCache { get; set; }
    public UserCache UserCache { get; set; }
    public LicenseTypeCache LicenseTypeCache { get; set; }

    public UserMetadataCache(DataContext context)
    {
        DepartmentCache = new UserDepartmentCache(context);
        JobTitleCache = new UserJobTitleCache(context);
        OfficeLocationCache = new OfficeLocationCache(context);
        UseageLocationCache = new UsageLocationCache(context);
        CountryOrRegionCache = new CountryOrRegionCache(context);
        CompanyNameCache = new CompanyNameCache(context);
        UserCache = new UserCache(context);
        LicenseTypeCache = new LicenseTypeCache(context);
        StateOrProvinceCache = new StateOrProvinceCache(context);
    }
}
