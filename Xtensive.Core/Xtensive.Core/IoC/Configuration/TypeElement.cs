// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.15

using System;
using System.Configuration;
using Xtensive.Core.Configuration;

namespace Xtensive.Core.IoC.Configuration
{
  /// <summary>
  /// Type mapping configuration element.
  /// </summary>
  public class TypeElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string TypeElementName = "type";
    private const string MapToElementName = "mapTo";
    private const string SingletonElementName = "singleton";

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
    /// Source type to configure in the container.
    /// </summary>
    [ConfigurationProperty(TypeElementName, IsRequired=true)]
    public string Type
    {
      get { return (string) this[TypeElementName]; }
      set { this[TypeElementName] = value; }
    }

     /// <summary>
    /// Destination type for type mapping.
    /// </summary>
    [ConfigurationProperty(MapToElementName, IsRequired=true)]
    public string MapTo
    {
      get { return (string) this[MapToElementName]; }
      set { this[MapToElementName] = value; }
    }

     /// <summary>
    /// Indicates whether instance of specified type should has singleton behavior or not.
    /// </summary>
    [ConfigurationProperty(SingletonElementName, IsRequired=false, DefaultValue = false)]
    public bool Singleton
    {
      get { return (bool) this[SingletonElementName]; }
      set { this[SingletonElementName] = value; }
    }

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return this; }
    }
  }
}