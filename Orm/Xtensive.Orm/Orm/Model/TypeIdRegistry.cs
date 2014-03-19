// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.13

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Dual-mapping between type identifiers and <see cref="TypeInfo"/>.
  /// </summary>
  [Serializable]
  public sealed class TypeIdRegistry : LockableBase
  {
    private readonly Dictionary<TypeInfo, int> mapping;
    private readonly Dictionary<int, TypeInfo> reverseMapping;

    /// <summary>
    /// Gets collection of registered types.
    /// </summary>
    public IEnumerable<TypeInfo> Types { get { return mapping.Keys; } }

    /// <summary>
    /// Gets collection of registered type identifiers.
    /// </summary>
    public IEnumerable<int> TypeIdentifiers { get { return reverseMapping.Keys; } }

    /// <summary>
    /// Gets type identifier for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to get type identifier for.</param>
    /// <returns>Type identifier for the specified <paramref name="type"/>.</returns>
    public int this[TypeInfo type]
    {
      get
      {
        ArgumentValidator.EnsureArgumentNotNull(type, "type");
        int result;
        if (!mapping.TryGetValue(type, out result))
          throw new KeyNotFoundException(string.Format(Strings.ExTypeXIsNotRegisteredInTheModel, type.Name));
        return result;
      }
    }

    /// <summary>
    /// Gets type for the specified <paramref name="typeId"/>.
    /// </summary>
    /// <param name="typeId">Type identifier to get type for.</param>
    /// <returns>Type for the specified <paramref name="typeId"/>.</returns>
    public TypeInfo this[int typeId]
    {
      get
      {
        TypeInfo result;
        if (!reverseMapping.TryGetValue(typeId, out result))
          throw new KeyNotFoundException(string.Format(Strings.TypeIdXIsNotRegistered, typeId));
        return result;
      }
    }

    /// <summary>
    /// Checks if specified <paramref name="type"/> is registered.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>True if <paramref name="type"/> is registered,
    /// otherwise false.</returns>
    public bool Contains(TypeInfo type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      return mapping.ContainsKey(type);
    }

    /// <summary>
    /// Resets all mapping information.
    /// </summary>
    public void Clear()
    {
      this.EnsureNotLocked();

      mapping.Clear();
      reverseMapping.Clear();
    }

    /// <summary>
    /// Registers mapping between <paramref name="typeId"/>
    /// and <paramref name="type"/>.
    /// </summary>
    /// <param name="typeId">Type identifier.</param>
    /// <param name="type">Type.</param>
    public void Register(int typeId, TypeInfo type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      this.EnsureNotLocked();

      mapping[type] = typeId;
      reverseMapping[typeId] = type;
    }

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public TypeIdRegistry()
    {
      mapping = new Dictionary<TypeInfo, int>();
      reverseMapping = new Dictionary<int, TypeInfo>();
    }
  }
}