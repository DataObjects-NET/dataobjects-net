using System;
using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Reprocessing.Configuration
{
  /// <summary>
  /// The configuration of the reprocessing system.
  /// </summary>
  public class ReprocessingConfiguration
  {
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
    /// Initializes a new instance of the <see cref="ReprocessingConfiguration"/> class.
    /// </summary>
    public ReprocessingConfiguration()
    {
      DefaultExecuteStrategy = DefaultDefaultExecuteStrategy;
      DefaultTransactionOpenMode = DefaultDefaultTransactionOpenMode;
    }
  }
}