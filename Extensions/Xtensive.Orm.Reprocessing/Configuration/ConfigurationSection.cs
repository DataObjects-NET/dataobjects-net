using System;
using System.ComponentModel;
using System.Configuration;

namespace Xtensive.Orm.Reprocessing.Configuration
{
  /// <summary>
  /// A root element of reprocessing configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    /// <summary>
    /// Gets default section name for reprocessing configuration.
    /// Value is "Xtensive.Orm.Reprocessing".
    /// </summary>
    [Obsolete("Use ReprocessingConfiguration.DefaultSectionName instead")]
    public static readonly string DefaultSectionName = "Xtensive.Orm.Reprocessing";

    /// <summary>
    /// Gets or sets default transaction open mode.
    /// </summary>
    [ConfigurationProperty("defaultTransactionOpenMode", DefaultValue = TransactionOpenMode.New)]
    public TransactionOpenMode DefaultTransactionOpenMode
    {
      get { return (TransactionOpenMode) this["defaultTransactionOpenMode"]; }
    }

    /// <summary>
    /// Gets or sets default execute strategy
    /// </summary>
    [ConfigurationProperty("defaultExecuteStrategy", DefaultValue = typeof (HandleReprocessableExceptionStrategy))]
    [TypeConverter(typeof (TypeNameConverter))]
    public Type DefaultExecuteStrategy
    {
      get { return (Type) this["defaultExecuteStrategy"]; }
    }
  }
}