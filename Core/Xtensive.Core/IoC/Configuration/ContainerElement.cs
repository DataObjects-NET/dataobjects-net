// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System;
using System.Configuration;
using Xtensive.Collections.Configuration;
using Xtensive.Configuration;
using Xtensive.Core;

namespace Xtensive.IoC.Configuration
{
  /// <summary>
  /// IoC container configuration element.
  /// </summary>
  [Serializable]
  public class ContainerElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string TypeElementName = "type";
    private const string ParentElementName = "parent";
    private const string ExplicitElementName = "explicit";
    private const string AutoElementName = "auto";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return Name.IsNullOrEmpty() ? string.Empty : Name; }
    }

    /// <summary>
    /// Gets or sets the name of the container. 
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false, DefaultValue = null)]
    public string Name {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the type of the container. 
    /// </summary>
    [ConfigurationProperty(TypeElementName, IsRequired = false, DefaultValue = null)]
    public string Type {
      get { return (string) this[TypeElementName]; }
      set { this[TypeElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the name of the parent container. 
    /// </summary>
    [ConfigurationProperty(ParentElementName, IsRequired = false, DefaultValue = null)]
    public string Parent {
      get { return (string) this[ParentElementName]; }
      set { this[ParentElementName] = value; }
    }

    /// <summary>
    /// Gets the collection of service configurations.
    /// </summary>
    [ConfigurationProperty(ExplicitElementName, IsRequired = false, IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(ConfigurationCollection<ServiceRegistrationElement>), AddItemName = "add")]
    public ConfigurationCollection<ServiceRegistrationElement> Explicit {
      get {
        return (ConfigurationCollection<ServiceRegistrationElement>) base[ExplicitElementName];
      }
    }

    /// <summary>
    /// Gets the automatic .
    /// </summary>
    /// <value>The types.</value>
    [ConfigurationProperty(AutoElementName, IsRequired = false, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<TypeRegistrationElement>), AddItemName = "add")]
    public ConfigurationCollection<TypeRegistrationElement> Auto {
      get { return (ConfigurationCollection<TypeRegistrationElement>) base[AutoElementName]; }
    }
  }
}