// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Building.Builders
{
  internal static class FieldBuilder
  {
    public static IList<FieldDef> DefineFields(TypeDef typeDef)
    {
      BuildingContext buildingContext = BuildingScope.Context;
      Log.Info("Defining fields.");

      var fields = new List<FieldDef>();
      var properties = typeDef.UnderlyingType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      foreach (var propertyInfo in properties) {
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {

          // [Field] attribute must be applied on any persistent field
          var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>(false);
          if (fieldAttribute==null)
            continue;

          if (propertyInfo.DeclaringType!=propertyInfo.ReflectedType)
            continue;

          FieldDef field = DefineField(typeDef, propertyInfo);
          if (!scope.IsCaptured(LogEventTypes.Error))
            fields.Add(field);
        }
      }

      return fields;
    }

    public static FieldDef DefineField(TypeDef typeDef, string name, Type valueType)
    {
      Log.Info("Defining field '{0}'", name);
      FieldDef fieldDef = new FieldDef(valueType);
      fieldDef.Name = name;
      return fieldDef;
    }

    public static FieldDef DefineField(TypeDef typeDef, PropertyInfo propertyInfo)
    {
      BuildingContext context = BuildingScope.Context;
      Log.Info("Defining field '{0}'", propertyInfo.Name);

      // We do not support "persistent" indexers
      ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();
      if (indexParameters.Length > 0) {
        Log.Error("Indexed properties are not supported.");
        return null;
      }

      FieldDef fieldDef = new FieldDef(propertyInfo);
      fieldDef.Name = context.NameProvider.BuildName(fieldDef);

      var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>(false);
      if (fieldAttribute!=null)
        AttributeProcessor.Process(fieldDef, fieldAttribute);

//      foreach (ConstraintAttribute attribute in propertyInfo.GetAttributes<ConstraintAttribute>(false)) 
//        AttributeProcessor.Process(fieldDef, attribute);
     
      return fieldDef;
    }

    public static FieldInfo BuildDeclaredField(TypeInfo typeInfo, FieldDef fieldDef)
    {
      BuildingContext context = BuildingScope.Context;
      Log.Info("Building declared field '{0}.{1}'", typeInfo.Name, fieldDef.Name);

      FieldInfo field = new FieldInfo(typeInfo, fieldDef.Attributes);
      field.UnderlyingProperty = fieldDef.UnderlyingProperty;
      field.Name = fieldDef.Name;
      field.MappingName = fieldDef.MappingName;
      field.ValueType = fieldDef.ValueType;
      field.Length = fieldDef.Length;

      if (field.IsEntitySet) {
        if (!BuildReferencedType(fieldDef))
          return null;

        AssociationBuilder.BuildAssociation(fieldDef, field);
        return field;
      }

      if (field.IsEntity) {
        if (!BuildReferencedType(fieldDef))
          return null;

        AssociationBuilder.BuildAssociation(fieldDef, field);

        TypeDef typeDef = BuildingScope.Context.Definition.Types[field.DeclaringType.UnderlyingType];
        typeDef.Indexes.Add(IndexBuilder.DefineForeignKey(typeDef, fieldDef));

        context.ComplexFields.Add(field);
      }

      if (field.IsStructure) {
        if (!BuildReferencedType(fieldDef))
          return null;
        context.ComplexFields.Add(field);
      }

      if (field.IsPrimitive)
        field.Column = ColumnBuilder.BuildColumn(field);

      ValidationResult vrLazyLoad = Validator.ValidateLazyLoad(field);
      if (!vrLazyLoad.Success) {
        Log.Error(vrLazyLoad.Message);
      }
      return field;
    }

    private static bool BuildReferencedType(FieldDef fieldDef)
    {
      TypeDef typeDef;
      if (!BuildingScope.Context.Definition.Types.TryGetValue(fieldDef.ValueType, out typeDef)) {
        Log.Error("Type '{0}' is not registered in the model.", fieldDef.ValueType.FullName);
        return false;
      }
      TypeBuilder.BuildType(typeDef);
      return true;
    }

    public static FieldInfo BuildInheritedField(TypeInfo type, FieldInfo ancsField)
    {
      BuildingContext buildingContext = BuildingScope.Context;
      Log.Info("Building inherited field '{0}.{1}'", type.Name, ancsField.Name);
      FieldInfo field = ancsField.Clone();
      field.ReflectedType = type;
      field.DeclaringType = ancsField.DeclaringType;
      field.IsInherited = true;
      if (field.IsStructure || field.IsEntity)
        buildingContext.ComplexFields.Add(field);
      else if (ancsField.Column!=null)
        field.Column = ColumnBuilder.BuildInheritedColumn(field, ancsField.Column);

      return field;
    }

    public static FieldInfo BuildInterfaceField(TypeInfo type, FieldInfo implField, FieldDef fieldDef) 
    {
      string name = fieldDef!=null ? fieldDef.Name : implField.Name;
      Log.Info("Building interface field '{0}.{1}'", type.Name, name);
      FieldInfo field = implField.Clone();
      field.Name = name;
      field.ReflectedType = type;
      field.DeclaringType = type;
      field.IsInherited = false;
      if (field.IsStructure || field.IsEntity)
        BuildingScope.Context.ComplexFields.Add(field);
      else if (implField.Column!=null)
        field.Column = ColumnBuilder.BuildInterfaceColumn(field, implField.Column);

      return field;
    }

    public static void BuildComplexField(FieldInfo field)
    {
      BuildingContext buildingContext = BuildingScope.Context;

      if (field.IsInherited) {
        FieldInfo parent = field.DeclaringType.Fields[field.Name];
        BuildNestedFields(field, parent.Fields);
      }
      else {
        TypeInfo type = buildingContext.Model.Types[field.ValueType];

        if (field.IsStructure)
          BuildNestedFields(field, type.Fields);

        if (field.IsEntity) {
          IEnumerable<FieldInfo> fields = type.Hierarchy.Fields.Keys.Join(type.Fields, key => key.Name,
            fld => fld.Name, (key, fld) => fld);
          BuildNestedFields(field, fields);
        }
      }

      int index = field.ReflectedType.Fields.IndexOf(field);
      foreach (FieldInfo nestedField in field.Fields.Find(NestingLevel.All))
        field.ReflectedType.Fields.Insert(++index, nestedField);
    }

    private static void BuildNestedFields(FieldInfo target, IEnumerable<FieldInfo> sourceFields)
    {
      foreach (FieldInfo srcField in sourceFields) {

        if (srcField.IsStructure)
          BuildNestedFields(target, srcField.Fields);
        else {
          FieldInfo clone = srcField.Clone();
          if (target.IsDeclared)
            clone.Name = BuildingScope.Context.NameProvider.BuildName(target, srcField);
          if (target.Fields.Contains(clone.Name))
            continue;
          clone.Parent = target;
          if (srcField.Column!=null)
            clone.Column = ColumnBuilder.BuildInheritedColumn(clone, srcField.Column);
          target.Fields.Add(clone);
        }
      }
    }
  }
}