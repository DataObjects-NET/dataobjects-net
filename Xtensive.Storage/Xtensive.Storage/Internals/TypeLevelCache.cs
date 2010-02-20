// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.19

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Storage.Internals.FieldAccessors;
using Xtensive.Storage.Model;
using System.Linq;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  internal sealed class TypeLevelCache
  {
    public readonly TypeInfo TypeInfo;
    private readonly FieldAccessor[] fieldAccessors;

    public FieldAccessor GetFieldAccessor(FieldInfo field)
    {
      return fieldAccessors[field.FieldId];
    }

    private static FieldAccessor CreateFieldAccessor(FieldInfo field)
    {
      if (field.IsEntity)
        return CreateFieldAccessor(typeof(EntityFieldAccessor<>), field);
      if (field.IsEntitySet)
        return CreateFieldAccessor(typeof(EntitySetFieldAccessor<>), field);
      if (field.IsStructure)
        return CreateFieldAccessor(typeof(StructureFieldAccessor<>), field);
      if (field.IsEnum)
        return CreateFieldAccessor(typeof(EnumFieldAccessor<>), field);
      if (field.ValueType==typeof(Key))
        return CreateFieldAccessor(typeof(KeyFieldAccessor<>), field);
      return CreateFieldAccessor(typeof(DefaultFieldAccessor<>), field);
    }

    private static FieldAccessor CreateFieldAccessor(Type accessorType, FieldInfo field)
    {
      var accessor = (FieldAccessor)
        System.Activator.CreateInstance(
          accessorType.MakeGenericType(field.ValueType),
          BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
          null, ArrayUtils<object>.EmptyArray, null);
      accessor.Field = field;
      return accessor;
    }


    // Constructors

    public TypeLevelCache(TypeInfo typeInfo)
    {
      TypeInfo = typeInfo;
      var fields = typeInfo.Fields;
      fieldAccessors = new FieldAccessor[fields.Count==0 ? 0 : (fields.Max(field => field.FieldId) + 1)];
      foreach (var field in fields)
        if (field.FieldId!=FieldInfo.NoFieldId)
          fieldAccessors[field.FieldId] = CreateFieldAccessor(field);
    }
  }
}