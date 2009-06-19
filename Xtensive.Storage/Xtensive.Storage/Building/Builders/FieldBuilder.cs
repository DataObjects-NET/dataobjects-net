// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Building.Builders
{
  internal static class FieldBuilder
  {
    /// <summary>
    /// Builds the declared field.
    /// </summary>
    /// <param name="type">The type field belongs to.</param>
    /// <param name="fieldDef">The field definition.</param>
    public static FieldInfo BuildDeclaredField(TypeInfo type, FieldDef fieldDef)
    {
      Log.Info(Strings.LogBuildingDeclaredFieldXY, type.Name, fieldDef.Name);

      var fieldInfo = new FieldInfo(type, fieldDef.Attributes)
        {
          UnderlyingProperty = fieldDef.UnderlyingProperty,
          Name = fieldDef.Name,
          OriginalName = fieldDef.Name,
          MappingName = fieldDef.MappingName,
          ValueType = fieldDef.ValueType,
          ItemType = fieldDef.ItemType,
          Length = fieldDef.Length,
          Scale = fieldDef.Scale,
          Precision = fieldDef.Precision
        };

      type.Fields.Add(fieldInfo);


      if (fieldInfo.IsEntitySet) {
        TypeBuilder.BuildType(fieldInfo.ItemType);

        AssociationBuilder.BuildAssociation(fieldDef, fieldInfo);
        return fieldInfo;
      }

      if (fieldInfo.IsEntity) {
        TypeBuilder.BuildType(fieldInfo.ValueType);
        BuildReferenceField(fieldInfo);

        var baseType = fieldInfo.ReflectedType.UnderlyingType.BaseType;
        if (!baseType.IsGenericType || baseType.GetGenericTypeDefinition()!=typeof (EntitySetItem<,>))
          AssociationBuilder.BuildAssociation(fieldDef, fieldInfo);
      }

      if (fieldInfo.IsStructure) {
        TypeBuilder.BuildType(fieldInfo.ValueType);
        BuildStructureField(fieldInfo);
      }

      if (fieldInfo.IsPrimitive) {
        fieldInfo.Column = ColumnBuilder.BuildColumn(fieldInfo);
        if (fieldInfo.ValueType==typeof(Key)) {
          var typeDef = BuildingContext.Current.ModelDef.Types[fieldInfo.DeclaringType.UnderlyingType];
          typeDef.Indexes.Add(IndexBuilder.DefineForeignKey(typeDef, fieldDef));
        }
      }
      return fieldInfo;
    }

    public static void BuildInheritedField(TypeInfo type, FieldInfo inheritedField)
    {
      Log.Info(Strings.LogBuildingInheritedFieldXY, type.Name, inheritedField.Name);
      var field = inheritedField.Clone();
      type.Fields.Add(field);
      field.ReflectedType = type;
      field.DeclaringType = inheritedField.DeclaringType;
      field.IsInherited = true;

      BuildNestedFields(field, field, inheritedField.Fields);

      if (inheritedField.Column!=null)
        field.Column = ColumnBuilder.BuildInheritedColumn(field, inheritedField.Column);
    }

    public static void BuildInterfaceField(TypeInfo type, FieldInfo implField, FieldDef fieldDef)
    {
      string name = fieldDef!=null ? fieldDef.Name : implField.Name;
      Log.Info(Strings.LogBuildingInterfaceFieldXY, type.Name, name);
      var field = implField.Clone();
      field.Name = name;
      field.OriginalName = name;
      field.ReflectedType = type;
      field.DeclaringType = type;
      field.IsInherited = false;
      type.Fields.Add(field);

      if (implField.Column!=null)
        field.Column = ColumnBuilder.BuildInterfaceColumn(field, implField.Column);
    }

    public static void BuildReferenceField(FieldInfo field)
    {
      var context = BuildingContext.Current;
      var type = context.Model.Types[field.ValueType];
      var fields = type.Hierarchy.KeyInfo.Fields.Keys.Join(type.Fields, key => key.Name,
        fld => fld.Name, (key, fld) => fld);

      BuildNestedFields(field, field, fields);
    }

    public static void BuildStructureField(FieldInfo field)
    {
      var context = BuildingContext.Current;
      var type = context.Model.Types[field.ValueType];

      BuildNestedFields(field, field, type.Fields);
    }

    private static void BuildNestedFields(FieldInfo root, FieldInfo target, IEnumerable<FieldInfo> fields)
    {
      var context = BuildingContext.Current;

      foreach (FieldInfo field in fields) {
        var clone = field.Clone();
        clone.IsSystem = false;
        if (target.IsDeclared) {
          clone.Name = BuildingContext.Current.NameBuilder.Build(target, field);
          clone.OriginalName = field.OriginalName;
          clone.MappingName = BuildingContext.Current.NameBuilder.BuildMappingName(target, field);
        }
        if (target.Fields.Contains(clone.Name))
          continue;
        clone.Parent = target;
        target.ReflectedType.Fields.Add(clone);

        if (field.IsStructure || field.IsEntity) {
          BuildNestedFields(root, clone, field.Fields);
          foreach (FieldInfo clonedFields in clone.Fields)
            target.Fields.Add(clonedFields);
        }
        else {
          if (field.Column != null)
            clone.Column = ColumnBuilder.BuildInheritedColumn(clone, field.Column);
          if (clone.IsEntity && !IsEntitySetItem(clone.ReflectedType)) {
            var refField = field;
            var origin = context.Model.Associations.Find(context.Model.Types[field.ValueType], true).Where(a => a.OwnerField == refField).FirstOrDefault();
            if (origin != null) {
              AssociationBuilder.BuildAssociation(origin, clone);
              context.DiscardedAssociations.Add(origin);
            }
          }
        }
      }
    }

    private static bool IsEntitySetItem(TypeInfo type)
    {
      Type underlyingBaseType = type.UnderlyingType.BaseType;
      return underlyingBaseType!=null
        && underlyingBaseType.IsGenericType
          && underlyingBaseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
    }
  }
}
