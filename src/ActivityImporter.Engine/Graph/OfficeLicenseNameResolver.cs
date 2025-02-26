using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Reflection;

namespace ActivityImporter.Engine.Graph;

/// <summary>
/// CSV wrapper for license names
/// </summary>
public class OfficeLicenseNameResolver
{
    private List<OfficeNamesCsvImportLine> _records = new List<OfficeNamesCsvImportLine>();
    public OfficeLicenseNameResolver()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        const string RESOURCE_NAME = "ActivityImporter.Engine.Graph.O365ProductIdentifiers.csv";
        using (var stream = assembly.GetManifestResourceStream(RESOURCE_NAME))
            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    using (var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        _records = csv.GetRecords<OfficeNamesCsvImportLine>().ToList();
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(RESOURCE_NAME), $"No license info resource found by name '{RESOURCE_NAME}'");
            }

    }

    public string? GetDisplayNameFor(string id)
    {
        var result = _records.Where(r => r.IdString.ToLower() == id.ToLower()).FirstOrDefault();
        if (result == null)
            return null;

        return result.DisplayName;
    }
}

public class OfficeNamesCsvImportLine
{
    [Name("Product_Display_Name")]
    public required string DisplayName { get; set; }

    [Name("String_Id")]
    public required string IdString { get; set; }

    public override string ToString()
    {
        return $"{this.IdString} ({this.DisplayName})";
    }
}