// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System;
using System.Configuration;
using Xtensive.Core.Configuration;

namespace Xtensive.Core.IoC.Configuration
{
  [Serializable]
  public class ContainerElement : ConfigurationCollectionElementBase
  {
    private const string CollectionElementName = "types";
    private const string NameElementName = "name";

    /// <summary>
    /// Name to use when registering this type. 
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false, DefaultValue = null)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// Gets the collection of type configurations.
    /// </summary>
    [ConfigurationProperty(CollectionElementName, IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(ConfigurationCollection<TypeElement>), AddItemName = "type")]
    public ConfigurationCollection<TypeElement> Types {
      get {
        return (ConfigurationCollection<TypeElement>)base[CollectionElementName];
      }
    }

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return Name; }
    }
  }
}