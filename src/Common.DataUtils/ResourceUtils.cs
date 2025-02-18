using System.Reflection;

namespace Common.DataUtils;

public class ResourceUtils
{
    public static string ReadResource(Assembly assembly, string resourcePath)
    {

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
