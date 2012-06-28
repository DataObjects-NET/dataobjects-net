// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.15

using System;
using System.Configuration;
using Xtensive.Core;

namespace Xtensive.IoC.Configuration
{
  /// <summary>
  /// Service mapping configuration element.
  /// </summary>
  public class ServiceRegistrationElement : ConfigurationCollectionElementBase
  {
    private const string TypeElementName = "type";
    private const string NameElementName = "name";
    private const string MapToElementName = "mapTo";
    private const string SingletonElementName = "singleton";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return this; }
    }

    /// <summary>
    /// Gets or sets the type of the service.
    /// </summary>
    [ConfigurationProperty(TypeElementName, IsRequired=true)]
    public string Type
    {
      get { return (string) this[TypeElementName]; }
      set { this[TypeElementName] = value.IsNullOrEmpty() ? null : value; }
    }

    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false, DefaultValue = null)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value.IsNullOrEmpty() ? null : value; }
    }

    /// <summary>
    /// Gets or sets the type to map the service to.
    /// </summary>
    [ConfigurationProperty(MapToElementName, IsRequired = true)]
    public string MapTo
    {
      get { return (string) this[MapToElementName]; }
      set { this[MapToElementName] = value.IsNullOrEmpty() ? null : value; }
    }

     /// <summary>
    /// Indicates whether instance of service is container-level singleton or not.
    /// </summary>
    [ConfigurationProperty(SingletonElementName, IsRequired=false, DefaultValue = false)]
    public bool Singleton
    {
      get { return (bool) this[SingletonElementName]; }
      set { this[SingletonElementName] = value; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="ServiceRegistration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public ServiceRegistration ToNative()
    {
      return new ServiceRegistration(
        System.Type.GetType(Type), 
        Name.IsNullOrEmpty() ? null : Name, 
        System.Type.GetType(MapTo), 
        Singleton);
    }
  }
}