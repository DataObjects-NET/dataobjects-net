// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
  [Serializable]
  public sealed class TypeDiscriminatorMap : Node, 
    IEnumerable<Pair<object, TypeInfo>>
  {
    private TypeInfo @default;
    private readonly Dictionary<object, TypeInfo> map = new Dictionary<object, TypeInfo>();
    private readonly Dictionary<TypeInfo, object> reversedMap = new Dictionary<TypeInfo, object>();
    private FieldInfo field;

    public FieldInfo Field
    {
      get { return field; }
      set
      {
        EnsureNotLocked();
        if (field != null)
          throw new InvalidOperationException(Strings.ExTypeDiscriminatorFieldIsAlreadySet);
        field = value;
      }
    }

    public ColumnInfo Column
    {
      get { return Field.Column; }
    }

    public TypeInfo Default
    {
      get { return @default; }
    }

    public TypeInfo this[object typeDiscriminatorValue]
    {
      get
      {
        TypeInfo result;
        if (map.TryGetValue(typeDiscriminatorValue, out result))
          return result;
        return @default;
      }
    }

    public object this[TypeInfo typeInfo]
    {
      get
      {
        object result;
        if (reversedMap.TryGetValue(typeInfo, out result))
          return result;
        return null;
      }
    }

    public void RegisterTypeMapping(TypeInfo type, object typeDiscriminatorValue)
    {
      EnsureNotLocked();
      map.Add(typeDiscriminatorValue, type);
      reversedMap.Add(type, typeDiscriminatorValue);
    }

    public void RegisterDefaultType(TypeInfo type)
    {
      EnsureNotLocked();
      if (@default != null)
        throw new InvalidOperationException(Strings.ExDefaultTypeIsAlreadyRegistered);

      @default = type;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Pair<object, TypeInfo>> GetEnumerator()
    {
      return map
        .Select(kvp => new Pair<object, TypeInfo>(kvp.Key, kvp.Value))
        .ToList().GetEnumerator();
    }
  }
}