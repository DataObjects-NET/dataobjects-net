// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Logging configuration element within a configuration file.
  /// </summary>
  public class LoggingElement : ConfigurationElement
  {
    private const string ProviderElementName = "provider";
    private const string LogsCollectionElementName = "";
    
    /// <summary>
    /// Gets or sets external provider. Provider's name specified as assembly qualified name."
    /// </summary>
    [ConfigurationProperty(ProviderElementName, IsRequired = false)]
    public string Provider
    {
      get { return (string) this[ProviderElementName]; }
      set { this[ProviderElementName] = value; }
    }

    /// <summary>
    /// Gets or sets collection of <see cref="LogElement"/>
    /// </summary>
    [ConfigurationProperty(LogsCollectionElementName, IsRequired = false, IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(ConfigurationCollection<LogElement>), AddItemName = "log")]
    public ConfigurationCollection<LogElement> Logs
    {
      get { return (ConfigurationCollection<LogElement>) this[LogsCollectionElementName]; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="LoggingConfiguration"/>
    /// </summary>
    /// <returns></returns>
    public LoggingConfiguration ToNative()
    {
      var loggingConfiguration = new LoggingConfiguration(Provider);
      foreach (var logElement in Logs) {
        loggingConfiguration.Logs.Add(logElement.ToNative());
      }
      return loggingConfiguration;
    }
  }
}