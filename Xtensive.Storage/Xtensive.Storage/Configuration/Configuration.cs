// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
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
    private readonly CollectionBaseSlim<DomainConfiguration> domains = new CollectionBaseSlim<DomainConfiguration>();

    /// <summary>
    /// Gets domain configurations.
    /// </summary>
    public CollectionBaseSlim<DomainConfiguration> Domains
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
      var section = (RootConfigurationSection)ConfigurationManager.GetSection(sectionName);
      if (section==null) 
        throw new InvalidOperationException(String.Format("Section \"{0}\" not found in application configuration file.", sectionName));
      foreach (DomainElement domain in section.Domains) {
        result.Domains.Add(new DomainConfiguration(domain));
      }
      return result;
    }
  }
}