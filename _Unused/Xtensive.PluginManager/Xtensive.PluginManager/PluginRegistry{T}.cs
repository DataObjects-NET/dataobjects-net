// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.18

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Represents a collection of plugin types by their attributes.
  /// </summary>
  /// <typeparam name="T">Attribute type.</typeparam>
  [FileIOPermission(SecurityAction.Demand)]
  public class PluginRegistry<T>
    where T: Attribute
  {
    private readonly IDictionary<T, ITypeInfo> foundTypes = new Dictionary<T, ITypeInfo>();
    private readonly IDictionary<ITypeInfo, Type> loadedTypes = new Dictionary<ITypeInfo, Type>();
    private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Determines whether plugin with specified attribute exists.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns><see langword="true"/> if a plugin <see cref="Type"/> with specified attribute exists, otherwise <see langword="false"/>.</returns>
    public bool Exists(T attribute)
    {
      return rwLock.ExecuteReader(() => foundTypes.ContainsKey(attribute));
    }

    /// <summary>
    /// Gets the plugin <see cref="System.Type"/> by the specified attribute.
    /// </summary>
    /// <value>The plugin <see cref="System.Type"/></value>
    public Type this[T attribute]
    {
      get
      {
        return rwLock.ExecuteReader(
          delegate {
            if (!Exists(attribute))
              return null;
            else {
              ITypeInfo typeInfo = foundTypes[attribute];
              rwLock.ExecuteWriter(
                delegate {
                  if (!loadedTypes.ContainsKey(typeInfo)) {
                    Assembly assembly = Assembly.LoadFrom(Uri.UnescapeDataString(typeInfo.AssemblyName.CodeBase));
                    loadedTypes[typeInfo] = assembly.GetType(typeInfo.TypeName);
                  }
                });
              return loadedTypes[typeInfo];
            }
          });
      }
    }

    /// <summary>
    /// Registers the specified attributes.
    /// </summary>
    /// <param name="attributes">The attributes.</param>
    /// <param name="typeInfo">The <see cref="ITypeInfo"/>.</param>
    public void Register(T[] attributes, ITypeInfo typeInfo)
    {
      rwLock.ExecuteWriter(
        delegate {
          foreach (T attribute in attributes) {
            foundTypes[attribute] = typeInfo;
          }
        });
    }

    /// <summary>
    /// Gets a collection of found types.
    /// </summary>
    public IDictionary<T, ITypeInfo> GetFoundTypes()
    {
      return new ReadOnlyDictionary<T, ITypeInfo>(foundTypes);
    }
  }
}
