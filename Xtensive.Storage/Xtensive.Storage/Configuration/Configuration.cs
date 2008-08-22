// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
using System.Collections.Generic;
using System.Configuration;
using Xtensive.Core;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// DataObjects.NET configuration section.
  /// </summary>
  [Serializable]
  public class Configuration
  {
    private string sectionName;
    private readonly SortedList<string, DomainConfiguration> domains = new SortedList<string, DomainConfiguration>();

    /// <summary>
    /// Gets domain configurations.
    /// </summary>
    public SortedList<string, DomainConfiguration> Domains
    {
      get { return domains; }
    }

    /// <summary>
    /// Gets or sets section name of configuration.
    /// </summary>
    public string SectionName
    {
      get { return sectionName; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        sectionName = value;
      }
    }
    
    /// <summary>
    /// Loads configuration from default application configuration file.
    /// </summary>
    /// <param name="sectionName">Name of section to load as configuration.</param>
    /// <returns>Configuration initialized by corresponding section.</returns>
    public static Configuration Load(string sectionName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sectionName, "sectionName");
      var result = new Configuration();
      var section = (ConfigurationSectionHandler)ConfigurationManager.GetSection(sectionName);
      if (section==null) 
        throw new InvalidOperationException(String.Format("Section \"{0}\" not found in application configuration file.", sectionName));
      foreach (DomainConfigurationElement domainElement in section.Domains) {
        var domainConfiguration = new DomainConfiguration(domainElement);
        result.Domains.Add(domainConfiguration.Name, domainConfiguration);
      }
      return result;
    }
  }
}