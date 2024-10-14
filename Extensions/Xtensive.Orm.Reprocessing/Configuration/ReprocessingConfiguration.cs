using System;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Reprocessing.Configuration
{
  /// <summary>
  /// The configuration of the reprocessing system.
  /// </summary>
  public class ReprocessingConfiguration : ConfigurationBase
  {
    /// <summary>
    /// Gets default section name for reprocessing configuration.
    /// Value is "Xtensive.Orm.Reprocessing".
    /// </summary>
    public static readonly string DefaultSectionName = "Xtensive.Orm.Reprocessing";

    /// <summary>
    /// Gets default value of the <see cref="DefaultTransactionOpenMode"/> property.
    /// </summary>
    public static readonly TransactionOpenMode DefaultDefaultTransactionOpenMode = TransactionOpenMode.New;

    /// <summary>
    /// Gets default value of the <see cref="DefaultExecuteStrategy"/> property.
    /// </summary>
    public static readonly Type DefaultDefaultExecuteStrategy = typeof (HandleReprocessableExceptionStrategy);

    private TransactionOpenMode defaultTransactionOpenMode;
    private Type defaultExecuteStrategy;

    /// <summary>
    /// Gets or sets default value of the <see cref="TransactionOpenMode"/> parameter.
    /// </summary>
    public TransactionOpenMode DefaultTransactionOpenMode {
      get => defaultTransactionOpenMode;
      set {
        EnsureNotLocked();
        defaultTransactionOpenMode = value;
      }
    }

    /// <summary>
    /// Gets or sets default value of the <see cref="IExecuteActionStrategy"/> parameter.
    /// </summary>
    public Type DefaultExecuteStrategy {
      get => defaultExecuteStrategy;
      set {
        EnsureNotLocked();
        defaultExecuteStrategy = value;
      }
    }

    /// <inheritdoc />
    protected override ReprocessingConfiguration CreateClone() => new ReprocessingConfiguration();

    /// <inheritdoc />
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);

      var configuration = (ReprocessingConfiguration) source;
      configuration.DefaultTransactionOpenMode = configuration.DefaultTransactionOpenMode;
      configuration.DefaultExecuteStrategy = configuration.DefaultExecuteStrategy;
    }

    /// <inheritdoc />
    public override ReprocessingConfiguration Clone() =>  (ReprocessingConfiguration) base.Clone();

    /// <summary>
    /// Loads the reprocessing configuration from default section in application configuration file.
    /// </summary>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration Load()
    {
      return Load(DefaultSectionName);
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
      return Load(configuration, DefaultSectionName);
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
    /// Loads the <see cref="ReprocessingConfiguration"/> from specified section of configuration root.
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> to load section from.</param>
    /// <param name="sectionName">Name of the section where configuration is stored. Not applied if</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static ReprocessingConfiguration Load(IConfiguration configuration, string sectionName = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      if (configuration is IConfigurationRoot configurationRoot) {
        return new ReprocessingConfigurationReader().Read(configurationRoot, sectionName ?? DefaultSectionName);
      }
      else if (configuration is IConfigurationSection configurationSection) {
        return new ReprocessingConfigurationReader().Read(configurationSection);
      }

      throw new NotSupportedException("Type of configuration is not supported.");
    }


    /// <summary>
    /// Loads the <see cref="ReprocessingConfiguration"/> from specified section of configuration root.
    /// </summary>
    /// <param name="configurationRoot"><see cref="IConfigurationRoot"/> to load section from.</param>
    /// <param name="sectionName">Name of the section where configuration is stored.</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static ReprocessingConfiguration Load(IConfigurationRoot configurationRoot, string sectionName = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationRoot, nameof(configurationRoot));

      return new ReprocessingConfigurationReader().Read(configurationRoot, sectionName ?? DefaultSectionName);
    }

    /// <summary>
    /// Loads the <see cref="ReprocessingConfiguration"/> from given configuration section.
    /// </summary>
    /// <param name="configurationSection"><see cref="IConfigurationSection"/> to load from.</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static ReprocessingConfiguration Load(IConfigurationSection configurationSection)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationSection, nameof(configurationSection));

      return new ReprocessingConfigurationReader().Read(configurationSection);
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