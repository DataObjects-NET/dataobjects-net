// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.26

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Helpers;
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

    /// <summary>
    /// Gets or sets the field.
    /// </summary>
    /// <value>
    /// The field.
    /// </value>
    public FieldInfo Field
    {
      get { return field; }
      set
      {
        this.EnsureNotLocked();
        if (field != null)
          throw new InvalidOperationException(Strings.ExTypeDiscriminatorFieldIsAlreadySet);
        field = value;
      }
    }

    /// <summary>
    /// Gets the column.
    /// </summary>
    public ColumnInfo Column
    {
      get { return Field.Column; }
    }

    /// <summary>
    /// Gets the default type.
    /// </summary>
    public TypeInfo Default
    {
      get { return @default; }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Orm.Model.TypeInfo"/> with the specified type discriminator value.
    /// </summary>
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

    /// <summary>
    /// Gets the <see cref="System.Object"/> with the specified type info.
    /// </summary>
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

    /// <summary>
    /// Registers the type mapping.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="typeDiscriminatorValue">The type discriminator value.</param>
    public void RegisterTypeMapping(TypeInfo type, object typeDiscriminatorValue)
    {
      this.EnsureNotLocked();
      map.Add(typeDiscriminatorValue, type);
      reversedMap.Add(type, typeDiscriminatorValue);
    }

    /// <summary>
    /// Registers the default type.
    /// </summary>
    /// <param name="type">The type.</param>
    public void RegisterDefaultType(TypeInfo type)
    {
      this.EnsureNotLocked();
      if (@default != null)
        throw new InvalidOperationException(Strings.ExDefaultTypeIsAlreadyRegistered);

      @default = type;
    }

    
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Pair<object, TypeInfo>> GetEnumerator()
    {
      return map
        .Select(kvp => new Pair<object, TypeInfo>(kvp.Key, kvp.Value))
        .ToList().GetEnumerator();
    }
  }
}