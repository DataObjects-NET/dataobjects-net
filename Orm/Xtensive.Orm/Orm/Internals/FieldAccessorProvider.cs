// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.02.19

using System;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Orm.Internals.FieldAccessors;
using System.Linq;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Internals
{
  internal sealed class FieldAccessorProvider
  {
    private readonly FieldAccessor[] fieldAccessors;
    private static readonly Type EntityFieldAccessorType = typeof(EntityFieldAccessor<>);
    private static readonly Type EntitySetFieldAccessorType = typeof(EntitySetFieldAccessor<>);
    private static readonly Type StructureFieldAccessorType = typeof(StructureFieldAccessor<>);
    private static readonly Type EnumFieldAccessorType = typeof(EnumFieldAccessor<>);
    private static readonly Type KeyFieldAccessorType = typeof(KeyFieldAccessor<>);
    private static readonly Type DefaultFieldAccessorType = typeof(DefaultFieldAccessor<>);

    public FieldAccessor GetFieldAccessor(FieldInfo field) => fieldAccessors[field.FieldId];

    private static FieldAccessor CreateFieldAccessor(FieldInfo field)
    {
      if (field.IsEntity) {
        return CreateFieldAccessor(EntityFieldAccessorType, field);
      }

      if (field.IsEntitySet) {
        return CreateFieldAccessor(EntitySetFieldAccessorType, field);
      }

      if (field.IsStructure) {
        return CreateFieldAccessor(StructureFieldAccessorType, field);
      }

      if (field.IsEnum) {
        return CreateFieldAccessor(EnumFieldAccessorType, field);
      }

      if (field.ValueType == WellKnownOrmTypes.Key) {
        return CreateFieldAccessor(KeyFieldAccessorType, field);
      }

      return CreateFieldAccessor(DefaultFieldAccessorType, field);
    }

    private static FieldAccessor CreateFieldAccessor(Type accessorType, FieldInfo field)
    {
      var accessor = (FieldAccessor)
        System.Activator.CreateInstance(
          accessorType.MakeGenericType(field.ValueType),
          BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
          null, Array.Empty<object>(), null);
      accessor.Field = field;
      return accessor;
    }


    // Constructors

    public FieldAccessorProvider(TypeInfo typeInfo)
    {
      var fields = typeInfo.Fields;
      fieldAccessors = new FieldAccessor[fields.Count == 0 ? 0 : (fields.Max(field => field.FieldId) + 1)];
      foreach (var field in fields) {
        if (field.FieldId != FieldInfo.NoFieldId) {
          fieldAccessors[field.FieldId] = CreateFieldAccessor(field);
        }
      }
    }
  }
}