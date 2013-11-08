// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System.Configuration;
using JetBrains.Annotations;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// A root element of storage configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    private const string DomainCollectionElementName = "domains";
    private const string LogsCollectionElementName = "logging";
    private const string XmlNamespaceElementName = "xmlns";

    /// <summary>
    /// Gets or sets XML namespace.
    /// </summary>
    [ConfigurationProperty(XmlNamespaceElementName)]
    [UsedImplicitly]
    public string XmlNamespace
    {
      get { return (string) this[XmlNamespaceElementName]; }
      set { this[XmlNamespaceElementName] = value; }
    }

    /// <summary>
    /// Gets the collection of domain configurations.
    /// </summary>
    [ConfigurationProperty(DomainCollectionElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<DomainConfigurationElement>), AddItemName = "domain")]
    public ConfigurationCollection<DomainConfigurationElement> Domains {
      get {
        return (ConfigurationCollection<DomainConfigurationElement>)base[DomainCollectionElementName];
      }
    }

    /// <summary>
    /// Gets configuration of logging.
    /// </summary>
    [ConfigurationProperty(LogsCollectionElementName, IsRequired = false)]
    public LoggingElement Logging
    {
      get { return (LoggingElement)this[LogsCollectionElementName]; }
    }
  }
}