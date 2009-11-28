// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.26

using System;
using System.Collections.Generic;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class TypeDiscriminatorMap : Node
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
        this.EnsureNotLocked();
        if (field != null)
          throw new InvalidOperationException("TypeDiscriminator field is already set.");
        field = value;
      }
    }

    public ColumnInfo Column
    {
      get { return Field.Column; }
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
      this.EnsureNotLocked();
      map.Add(typeDiscriminatorValue, type);
      reversedMap.Add(type, typeDiscriminatorValue);
    }

    public void RegisterDefaultType(TypeInfo type)
    {
      this.EnsureNotLocked();
      if (@default != null)
        throw new InvalidOperationException("Default type is already registered.");

      @default = type;
    }
  }
}