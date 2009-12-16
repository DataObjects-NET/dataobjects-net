// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.15

using System.Configuration;
using Xtensive.Core.Configuration;

namespace Xtensive.Core.IoC.Configuration
{
  /// <summary>
  /// A root element of diagnostics configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    /// <summary>
    /// Gets default section name for diagnostics configuration.
    /// Value is "Xtensive.Core.Diagnostics".
    /// </summary>
    public static readonly string DefaultSectionName = "Xtensive.Core.IoC";
    
    private const string CollectionElementName = "containers";

    /// <summary>
    /// Gets the collection of container configurations.
    /// </summary>
    [ConfigurationProperty(CollectionElementName, IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(ContainerElementCollection), AddItemName = "container")]
    public ContainerElementCollection Containers {
      get {
        return (ContainerElementCollection)base[CollectionElementName];
      }
    }
  }
}