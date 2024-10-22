using System.Reflection;

namespace Web;

internal class Utils
{
    public static string ReadResource(string resourcePath)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        var manifests = assembly.GetManifestResourceNames();


        using (var stream = assembly.GetManifestResourceStream(resourcePath))
            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(resourcePath), $"No resource found by name '{resourcePath}'");
            }
    }
}
