﻿using System.Reflection;

namespace Common.DataUtils;


public class ProjectResourceReader
{
    private readonly Assembly _assembly;

    public ProjectResourceReader(Assembly assembly)
    {
        _assembly = assembly;
    }

    public string ReadResourceString(string resourcePath)
    {
        using (var stream = GetAssemblyManifest(resourcePath))

        using (var reader = new StreamReader(stream))
            return reader.ReadToEnd();
    }

    public byte[] ReadResourceBytes(string resourcePath)
    {
        using (var stream = GetAssemblyManifest(resourcePath))
        using (var reader = new BinaryReader(stream))
            return reader.ReadBytes((int)stream.Length);

    }

    public Stream GetAssemblyManifest(string resourcePath)
    {
        var stream = _assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            throw new ArgumentOutOfRangeException(nameof(resourcePath), $"No resource found by name '{resourcePath}'");
        }
        return stream;
    }

    public List<string> GetResourceNamesMatchingPathRoot(string resourcePathRoot)
    {
        return _assembly.GetManifestResourceNames().Where(x => x.StartsWith(resourcePathRoot)).ToList();
    }
}