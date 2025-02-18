using Common.DataUtils;
using System.Reflection;

namespace Web.Server;

internal class Utils
{
    public static string ReadResource(string resourcePath)
    {
        var assembly = Assembly.GetExecutingAssembly();

        return ResourceUtils.ReadResource(assembly, resourcePath);
    }
}
