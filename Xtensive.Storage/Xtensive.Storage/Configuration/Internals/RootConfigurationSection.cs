// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal class RootConfigurationSection : ConfigurationSection
  {
    private const string DomainsElementName = "domains";

    [ConfigurationProperty(DomainsElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<DomainElement>), AddItemName = "domain")]
    public ConfigurationCollection<DomainElement> Domains {
      get {
        return (ConfigurationCollection<DomainElement>)base[DomainsElementName];
      }
    }
  }
}