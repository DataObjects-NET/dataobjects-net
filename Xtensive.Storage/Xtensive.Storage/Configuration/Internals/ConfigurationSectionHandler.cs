// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal class ConfigurationSectionHandler : ConfigurationSection
  {
    private const string DomainCollectionElementName = "domains";

    [ConfigurationProperty(DomainCollectionElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<DomainConfigurationElement>), AddItemName = "domain")]
    public ConfigurationCollection<DomainConfigurationElement> Domains {
      get {
        return (ConfigurationCollection<DomainConfigurationElement>)base[DomainCollectionElementName];
      }
    }
  }
}