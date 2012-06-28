// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.12

using System;
using Xtensive.Collections;
using Xtensive.Core;
using System.Linq;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;

namespace Xtensive.IoC
{
  /// <summary>
  /// Describes single service mapping entry for <see cref="ServiceContainer"/>.
  /// </summary>
  [Serializable]
  public sealed class ServiceRegistration
  {
    /// <summary>
    /// Gets the type of the service.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the type it is mapped to.
    /// </summary>
    public Type MappedType { get; private set; }

    /// <summary>
    /// Gets the instance it is mapped to.
    /// </summary>
    public object MappedInstance { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this service is singleton.
    /// </summary>
    public bool Singleton { get; private set; }


    // Static constructor-like methods

    /// <summary>
    /// Creates an array of <see cref="ServiceRegistration"/> objects
    /// for the specified <paramref name="type"/>
    /// by scanning it <see cref="ServiceAttribute"/>s.
    /// </summary>
    /// <param name="type">The type to provide <see cref="ServiceRegistration"/> objects for.</param>
    /// <returns>
    /// An array of <see cref="ServiceRegistration"/> objects.
    /// </returns>
    public static ServiceRegistration[] CreateAll(Type type)
    {
      return CreateAll(type, false);
    }

    /// <summary>
    /// Creates an array of <see cref="ServiceRegistration"/> objects
    /// for the specified <paramref name="type"/>
    /// by scanning it <see cref="ServiceAttribute"/>s.
    /// </summary>
    /// <param name="type">The type to provide <see cref="ServiceRegistration"/> objects for.</param>
    /// <param name="defaultOnly">Return just registrations for which 
    /// <see cref="ServiceAttribute.Default"/>==<see langword="true" />.</param>
    /// <returns>
    /// An array of <see cref="ServiceRegistration"/> objects.
    /// </returns>
    public static ServiceRegistration[] CreateAll(Type type, bool defaultOnly)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (type.IsAbstract)
        return ArrayUtils<ServiceRegistration>.EmptyArray;

      var attributes = type.GetAttributes<ServiceAttribute>(AttributeSearchOptions.InheritNone);
      if (attributes==null)
        return ArrayUtils<ServiceRegistration>.EmptyArray;
      if (defaultOnly)
        attributes = attributes.Where(a => a.Default).ToArray();
      if (attributes.Length==0)
        return ArrayUtils<ServiceRegistration>.EmptyArray;

      var result = new ServiceRegistration[attributes.Length];
      for (int i = 0; i < attributes.Length; i++) {
        var sa = attributes[i];
        result[i] = new ServiceRegistration(sa.Type, sa.Name.IsNullOrEmpty() ? null : sa.Name, type, sa.Singleton);
      }
      return result;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <param name="mappedInstance">The instance it is mapped to.</param>
    public ServiceRegistration(Type type, object mappedInstance)
      : this (type, null, mappedInstance)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <param name="name">The name of the service.</param>
    /// <param name="mappedInstance">The instance it is mapped to.</param>
    public ServiceRegistration(Type type, string name, object mappedInstance)
    {
      Type = type;
      Name = name;
      MappedType = null;
      MappedInstance = mappedInstance;
      Singleton = true;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <param name="mappedType">The type it is mapped to.</param>
    /// <param name="singleton">A value indicating whether this service is singleton.</param>
    public ServiceRegistration(Type type, Type mappedType, bool singleton)
      : this (type, null, mappedType, singleton)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the service.</param>
    /// <param name="name">The name of the service.</param>
    /// <param name="mappedType">The type it is mapped to.</param>
    /// <param name="singleton">A value indicating whether this service is singleton.</param>
    public ServiceRegistration(Type type, string name, Type mappedType, bool singleton)
    {
      Type = type;
      Name = name;
      MappedType = mappedType;
      Singleton = singleton;
    }
  }
}