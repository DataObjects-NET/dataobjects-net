// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.14

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Diagnostics.Configuration
{
  /// <summary>
  /// A root element of diagnostics configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    /// <summary>
    /// Gets default section name for diagnostics configuration.
    /// Value is "Xtensive.Diagnostics".
    /// </summary>
    public static readonly string DefaultSectionName = "Xtensive.Diagnostics";
    
    private const string LogCollectionElementName = "logs";

    /// <summary>
    /// Gets the collection of domain configurations.
    /// </summary>
    [ConfigurationProperty(LogCollectionElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<LogElement>), AddItemName = "log")]
    public ConfigurationCollection<LogElement> Logs {
      get {
        return (ConfigurationCollection<LogElement>)base[LogCollectionElementName];
      }
    }
  }
}