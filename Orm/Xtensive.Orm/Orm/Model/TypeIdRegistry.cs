// Copyright (C) 2014-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private readonly Dictionary<TypeInfo, int> mapping = new Dictionary<TypeInfo, int>();
    private readonly Dictionary<int, TypeInfo> reverseMapping = new Dictionary<int, TypeInfo>();

    /// <summary>
    /// Gets collection of registered types.
    /// </summary>
    public IEnumerable<TypeInfo> Types => mapping.Keys;

    /// <summary>
    /// Gets collection of registered type identifiers.
    /// </summary>
    public IEnumerable<int> TypeIdentifiers => reverseMapping.Keys;

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
        return mapping.TryGetValue(type, out var result)
          ? result
          : throw new KeyNotFoundException(string.Format(Strings.ExTypeXIsNotRegistered, type.Name));
      }
    }

    /// <summary>
    /// Gets type for the specified <paramref name="typeId"/>.
    /// </summary>
    /// <param name="typeId">Type identifier to get type for.</param>
    /// <returns>Type for the specified <paramref name="typeId"/>.</returns>
    public TypeInfo this[int typeId] =>
      reverseMapping.TryGetValue(typeId, out var result)
        ? result
        : throw new KeyNotFoundException(string.Format(Strings.ExTypeIdXIsNotRegistered, typeId));

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
    /// Gets type identifier for the specified <paramref name="type"/>.
    /// Unlike <see cref="this[Xtensive.Orm.Model.TypeInfo]"/>
    /// this method does not throw <see cref="KeyNotFoundException"/>
    /// if <paramref name="type"/> is not registered.
    /// </summary>
    /// <param name="type">Type to get type identifier for.</param>
    /// <returns>Type identifier for <paramref name="type"/> if it is registered,
    /// otherwise <see cref="TypeInfo.NoTypeId"/>.</returns>
    public int GetTypeId(TypeInfo type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      return mapping.TryGetValue(type, out var result)
        ? result
        : TypeInfo.NoTypeId;
    }

    /// <summary>
    /// Resets all mapping information.
    /// </summary>
    public void Clear()
    {
      EnsureNotLocked();

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
      EnsureNotLocked();

      mapping[type] = typeId;
      reverseMapping[typeId] = type;
    }
  }
}
