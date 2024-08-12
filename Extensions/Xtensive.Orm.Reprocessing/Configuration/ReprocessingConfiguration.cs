using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Reprocessing.Configuration
{
  /// <summary>
  /// The configuration of the reprocessing system.
  /// </summary>
  public class ReprocessingConfiguration
  {
    // intermediate class for reading section
    private class ReprocessingOptions
    {
      public string DefaultTransactionOpenMode { get; set; }

      public string DefaultExecuteStrategy { get; set; }
    }

    /// <summary>
    /// Gets default value of the <see cref="DefaultTransactionOpenMode"/> property.
    /// </summary>
    public static readonly TransactionOpenMode DefaultDefaultTransactionOpenMode = TransactionOpenMode.New;

    /// <summary>
    /// Gets default value of the <see cref="DefaultExecuteStrategy"/> property.
    /// </summary>
    public static readonly Type DefaultDefaultExecuteStrategy = typeof (HandleReprocessableExceptionStrategy);

    /// <summary>
    /// Gets or sets default value of the <see cref="TransactionOpenMode"/> parameter.
    /// </summary>
    public TransactionOpenMode DefaultTransactionOpenMode { get; set; }

    /// <summary>
    /// Gets or sets default value of the <see cref="IExecuteActionStrategy"/> parameter.
    /// </summary>
    public Type DefaultExecuteStrategy { get; set; }

    /// <summary>
    /// Loads the reprocessing configuration from default section in application configuration file.
    /// </summary>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration Load()
    {
      return Load(ConfigurationSection.DefaultSectionName);
    }

    /// <summary>
    /// Loads the reprocessing configuration from sectionName section in application configuration file.
    /// </summary>
    /// <param name="sectionName">Name of the section.</param>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration Load(string sectionName)
    {
      var section = (ConfigurationSection) ConfigurationManager.GetSection(sectionName);
      return GetConfigurationFromSection(section);
    }

    /// <summary>
    /// /// Loads the reprocessing configuration from default sectionName section in given configuration.
    /// </summary>
    /// <param name="configuration">The configuration to load from.</param>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration Load(System.Configuration.Configuration configuration)
    {
      return Load(configuration, ConfigurationSection.DefaultSectionName);
    }

    /// <summary>
    /// /// Loads the reprocessing configuration from <paramref name="sectionName"/> section in given configuration file.
    /// </summary>
    /// <param name="configuration">The configuration to load from.</param>
    /// <param name="sectionName">The section to load from.</param>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration Load(System.Configuration.Configuration configuration, string sectionName)
    {
      var section = (ConfigurationSection) configuration.GetSection(sectionName);
      return GetConfigurationFromSection(section);
    }

    private static ReprocessingConfiguration GetConfigurationFromSection(ConfigurationSection section)
    {
      return section==null
        ? new ReprocessingConfiguration()
        : new ReprocessingConfiguration {
          DefaultExecuteStrategy = section.DefaultExecuteStrategy,
          DefaultTransactionOpenMode = section.DefaultTransactionOpenMode
        };
    }

    /// <summary>
    /// Loads the <see cref="ReprocessingConfiguration"/> from given configuration section.
    /// </summary>
    /// <param name="configurationSection"><see cref="IConfigurationSection"/> to load from.</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static ReprocessingConfiguration Load(IConfigurationSection configurationSection)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationSection, nameof(configurationSection));

      return TryReadAsOptions(configurationSection, out var reprocessingConfiguration)
         ? reprocessingConfiguration
         : new ReprocessingConfiguration();
    }

    private static bool TryReadAsOptions(IConfigurationSection configuration, out ReprocessingConfiguration reprocessingConfiguration)
    {
      var reprocessingOptions = configuration.Get<ReprocessingOptions>();

      if (reprocessingOptions == default) {
        reprocessingConfiguration = null;
        return false;
      }

      if (reprocessingOptions.DefaultTransactionOpenMode == default
        && reprocessingOptions.DefaultExecuteStrategy == default) {
        // that means instance is default. probably invalid
        reprocessingConfiguration = null;
        return false;
      }

      var result = new ReprocessingConfiguration();
      if (reprocessingOptions.DefaultTransactionOpenMode != default
        && Enum.TryParse<TransactionOpenMode>(reprocessingOptions.DefaultTransactionOpenMode, out var enumValue)) {
          result.DefaultTransactionOpenMode = enumValue;
      }
      if (!string.IsNullOrEmpty(reprocessingOptions.DefaultExecuteStrategy)) {
        var type = Type.GetType(reprocessingOptions.DefaultExecuteStrategy, false);
        if (type == null)
          throw new InvalidOperationException($"Can't resolve type '{reprocessingOptions.DefaultExecuteStrategy}'. Note that DefaultExecuteStrategy value should be in form of Assembly Qualified Name");
        result.DefaultExecuteStrategy = type;
      }
      reprocessingConfiguration = result;
      return true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReprocessingConfiguration"/> class.
    /// </summary>
    public ReprocessingConfiguration()
    {
      DefaultExecuteStrategy = DefaultDefaultExecuteStrategy;
      DefaultTransactionOpenMode = DefaultDefaultTransactionOpenMode;
    }
  }
}