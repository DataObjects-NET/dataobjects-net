// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System.Collections.Generic;
using System.Configuration;
using Xtensive.Core;
using ConfigurationSection = Xtensive.Orm.Configuration.Elements.ConfigurationSection;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Configuration of logging
  /// </summary>
  public sealed class LoggingConfiguration
  {
    /// <summary>
    /// Gets or sets external provider. Provider's name specified as assembly qualified name.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets list of <see cref="LogConfiguration"/>
    /// </summary>
    public IList<LogConfiguration> Logs { get; set; }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    public LoggingConfiguration()
    {
      Logs = new List<LogConfiguration>();
    }

    /// <summary>
    /// Loads logging configuration from standart configuration section.
    /// </summary>
    /// <returns>Loaded configuration</returns>
    public static LoggingConfiguration Load()
    {
      var section = (ConfigurationSection)ConfigurationManager.GetSection("Xtensive.Orm");
      var configuration = section != null ? section.Logging.ToNative() : new LoggingConfiguration();
      return configuration;
    }

    /// <summary>
    /// Loads logging configuration from custom configuration section.
    /// </summary>
    /// <param name="sectionName">Name of custom section.</param>
    /// <returns>Loaded configuration</returns>
    public static LoggingConfiguration Load(string sectionName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sectionName, "sectionName");
      var section = (ConfigurationSection)ConfigurationManager.GetSection(sectionName);
      var configuration = section != null ? section.Logging.ToNative() : new LoggingConfiguration();
      return configuration;
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    /// <param name="provider">External provider for logging. Provider's name specified as assembly qualified name.</param>
    public LoggingConfiguration(string provider)
    {
      Provider = provider;
      Logs = new List<LogConfiguration>();
    }
  }
}