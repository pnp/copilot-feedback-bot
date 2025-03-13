using System.Reflection;
using System.Runtime.CompilerServices;

namespace Common.DataUtils.Config;

public abstract class PropertyBoundConfig
{
    /// <summary>
    /// Load automatically config properties
    /// </summary>
    /// <param name="config">Config to read</param>
    /// <exception cref="ArgumentNullException">If config to read is null</exception>
    /// <exception cref="ConfigurationMissingException">If config has missing required properties</exception>
    public PropertyBoundConfig(Microsoft.Extensions.Configuration.IConfiguration config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        // Set config props
        var allProps = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in allProps)
        {
            // Set values
            var configValAtt = prop.GetCustomAttribute<ConfigValueAttribute>();
            if (configValAtt != null)
            {
                var configVal = config[configValAtt.BackingPropertyName ?? prop.Name];
                if (!configValAtt.Optional && string.IsNullOrEmpty(configVal))
                {
                    throw new ConfigurationMissingException(prop.Name);
                }
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(this, configVal);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(configVal, out var boolVal))
                    {
                        prop.SetValue(this, boolVal);
                    }
                    else
                    {
                        if (!configValAtt.Optional)
                        {
                            throw new ConfigurationMissingException(prop.Name);
                        }
                    }
                }
                else if (prop.PropertyType == typeof(int))
                {
                    if (int.TryParse(configVal, out var intVal))
                    {
                        prop.SetValue(this, intVal);
                    }
                    else
                    {
                        if (!configValAtt.Optional)
                        {
                            throw new ConfigurationMissingException(prop.Name);
                        }
                    }
                }
                else if (prop.PropertyType == typeof(long))
                {
                    if (long.TryParse(configVal, out var longVal))
                    {
                        prop.SetValue(this, longVal);
                    }
                    else
                    {
                        if (!configValAtt.Optional)
                        {
                            throw new ConfigurationMissingException(prop.Name);
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException($"Property type {prop.PropertyType} is not supported");
                }
            }

            // Set config sub-sections
            var configSectionAtt = prop.GetCustomAttribute<ConfigSectionAttribute>();
            if (configSectionAtt != null)
            {
                var configSection = config.GetSection(configSectionAtt.SectionName);

                if (!configSection.GetChildren().Any() && configSectionAtt.Optional)
                    continue;

                object? instance = null;
                try
                {
                    instance = Activator.CreateInstance(prop.PropertyType, configSection);
                }
                catch (TargetInvocationException ex)
                {
                    // Throw a more useful exception if sub-section is missing configuration
                    if (ex.InnerException is ConfigurationMissingException)
                    {
                        throw ex.InnerException;
                    }
                    throw;
                }

                prop.SetValue(this, instance);
            }
        }
    }

    public PropertyBoundConfig()
    {
        Console.WriteLine("WARNING: Using default constructor for PropertyBoundConfig - this is only for testing!");
    }
}

public class ConfigurationMissingException : Exception
{
    public ConfigurationMissingException(string propertyName) : base($"Missing required configuration value '{propertyName}'")
    {
    }
}

/// <summary>
/// Property comes from supplied config section
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ConfigValueAttribute : Attribute
{
    public ConfigValueAttribute(bool optional = false, string? backingPropertyName = null)
    {
        Optional = optional;
        BackingPropertyName = backingPropertyName;
    }

    public bool Optional { get; set; }
    public string? BackingPropertyName { get; set; }
}

/// <summary>
/// Property has a sub-section
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ConfigSectionAttribute : Attribute
{
    public ConfigSectionAttribute([CallerMemberName] string? sectionName = null!)
    {
        SectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
    }

    public bool Optional { get; set; }
    public string SectionName { get; set; }
}
