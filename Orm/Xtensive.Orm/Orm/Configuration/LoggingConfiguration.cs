// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
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
    /// Loads logging configuration from the default configuration section.
    /// </summary>
    /// <returns>Loaded configuration.</returns>
    public static LoggingConfiguration Load(System.Configuration.Configuration configuration)
    {
      return Load(configuration, WellKnown.DefaultConfigurationSection);
    }

    /// <summary>
    /// Loads logging configuration from the specified configuration section.
    /// </summary>
    /// <param name="sectionName">Name of configuration section.</param>
    /// <returns>Loaded configuration.</returns>
    public static LoggingConfiguration Load(System.Configuration.Configuration configuration, string sectionName)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sectionName, "sectionName");

      var section = (ConfigurationSection) configuration.GetSection(sectionName);
      if (section==null)
        throw new InvalidOperationException(string.Format(Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      var loggingConfiguration = section.Logging.ToNative();
      return loggingConfiguration;
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    public LoggingConfiguration()
    {
      Logs = new List<LogConfiguration>();
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