// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.11.26

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using System.Linq;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Type discriminator map.
  /// </summary>
  public sealed class TypeDiscriminatorMap : Node, 
    IEnumerable<Pair<object, TypeInfo>>
  {
    private readonly Dictionary<object, TypeInfo> map = new();
    private readonly Dictionary<TypeInfo, object> reversedMap = new();
    private TypeInfo @default;
    private FieldInfo @field;

    public FieldInfo Field
    {
      get => @field;
      set {
        EnsureNotLocked();
        if (@field is not null)
          throw new InvalidOperationException(Strings.ExTypeDiscriminatorFieldIsAlreadySet);
        @field = value;
      }
    }

    public ColumnInfo Column => Field.Column;

    public TypeInfo Default => @default;

    public TypeInfo this[object typeDiscriminatorValue]
    {
      get {
        if (map.TryGetValue(typeDiscriminatorValue, out var result))
          return result;
        return @default;
      }
    }

    public object this[TypeInfo typeInfo]
    {
      get {
        if (reversedMap.TryGetValue(typeInfo, out var result))
          return result;
        return null;
      }
    }

    public int Count => map.Count;

    public void RegisterTypeMapping(TypeInfo type, object typeDiscriminatorValue)
    {
      EnsureNotLocked();
      map.Add(typeDiscriminatorValue, type);
      reversedMap.Add(type, typeDiscriminatorValue);
    }

    public void RegisterDefaultType(TypeInfo type)
    {
      EnsureNotLocked();
      if (@default is not null)
        throw new InvalidOperationException(Strings.ExDefaultTypeIsAlreadyRegistered);

      @default = type;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IEnumerator<Pair<object, TypeInfo>> GetEnumerator()
    {
      return map
        .Select(kvp => new Pair<object, TypeInfo>(kvp.Key, kvp.Value))
        .ToList().GetEnumerator();
    }
  }
}