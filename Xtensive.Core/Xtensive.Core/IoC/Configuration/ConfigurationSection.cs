// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.15

using System.Configuration;
using Xtensive.Configuration;

namespace Xtensive.IoC.Configuration
{
  /// <summary>
  /// A root element of diagnostics configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    /// <summary>
    /// Gets default section name for IoC configuration.
    /// Value is "Xtensive.IoC".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.IoC";

    private const string CollectionElementName = "containers";

    /// <summary>
    /// Gets the collection of container configurations.
    /// </summary>
    [ConfigurationProperty(CollectionElementName, IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(ConfigurationCollection<ContainerElement>), AddItemName = "container")]
    public ConfigurationCollection<ContainerElement> Containers {
      get {
        return (ConfigurationCollection<ContainerElement>) base[CollectionElementName];
      }
    }
  }
}