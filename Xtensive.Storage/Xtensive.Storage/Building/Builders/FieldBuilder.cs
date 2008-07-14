// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
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
      Log.Info("Defining fields.");

      var fields = new List<FieldDef>();
      var properties = 
        typeDef.UnderlyingType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      foreach (PropertyInfo propertyInfo in properties)
        try {
          FieldDef field = DefineField(typeDef, propertyInfo);

          if (field!=null)
            fields.Add(field);
        }
        catch (DomainBuilderException e) {
          BuildingScope.Context.RegistError(e);
        }        
      
      return fields;
    }

    public static FieldDef DefineField(TypeDef typeDef, PropertyInfo propertyInfo)
    {
      BuildingContext context = BuildingScope.Context;
      Log.Info("Defining field '{0}'", propertyInfo.Name);

      // [Field] attribute must be applied on any persistent field
      var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>(false);
      if (fieldAttribute==null)
        return null;

      if (propertyInfo.DeclaringType!=propertyInfo.ReflectedType)
        return null;

      // We do not support "persistent" indexers
      ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();

      if (indexParameters.Length > 0)
        throw new DomainBuilderException(Resources.Strings.IndexedPropertiesAreNotSupported);

      FieldDef fieldDef = new FieldDef(propertyInfo);
      fieldDef.Name = context.NameProvider.BuildName(fieldDef);
      
      AttributeProcessor.Process(fieldDef, fieldAttribute);

      return fieldDef;
    }

    public static void BuildDeclaredField(TypeInfo type, FieldDef fieldDef)
    {
      Log.Info("Building declared field '{0}.{1}'", type.Name, fieldDef.Name);

      FieldInfo field = new FieldInfo(type, fieldDef.Attributes);
      field.UnderlyingProperty = fieldDef.UnderlyingProperty;
      field.Name = fieldDef.Name;
      field.MappingName = fieldDef.MappingName;
      field.ValueType = fieldDef.ValueType;
      field.Length = fieldDef.Length;
      type.Fields.Add(field);

      if (field.IsEntitySet) {
        TypeBuilder.BuildType(field.ValueType);

        AssociationBuilder.BuildAssociation(fieldDef, field);
        return;
      }

      if (field.IsEntity) {
        TypeBuilder.BuildType(field.ValueType);
        BuildReferenceField(field);

        AssociationBuilder.BuildAssociation(fieldDef, field);

        TypeDef typeDef = BuildingScope.Context.Definition.Types[field.DeclaringType.UnderlyingType];
        typeDef.Indexes.Add(IndexBuilder.DefineForeignKey(typeDef, fieldDef));
      }

      if (field.IsStructure) {
        TypeBuilder.BuildType(field.ValueType);
        BuildStructureField(field);
      }

      if (field.IsPrimitive)
        field.Column = ColumnBuilder.BuildColumn(field);

      if (field.IsPrimaryKey && field.LazyLoad)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.FieldXCanTBeLoadOnDemandAsItIsIncludedInPrimaryKey, field.Name));

      return;
    }

    public static void BuildInheritedField(TypeInfo type, FieldInfo inheritedField)
    {
      BuildingContext context = BuildingScope.Context;
      Log.Info("Building inherited field '{0}.{1}'", type.Name, inheritedField.Name);
      FieldInfo field = inheritedField.Clone();
      type.Fields.Add(field);
      if (inheritedField.Parent!=null)
        field.Parent = type.Fields[inheritedField.Parent.Name];
      else {
        field.ReflectedType = type;
        field.DeclaringType = inheritedField.DeclaringType;
        field.IsInherited = true;
      }

      if (inheritedField.Column!=null)
        field.Column = ColumnBuilder.BuildInheritedColumn(field, inheritedField.Column);
    }

    public static void BuildInterfaceField(TypeInfo type, FieldInfo implField, FieldDef fieldDef) 
    {
      string name = fieldDef!=null ? fieldDef.Name : implField.Name;
      Log.Info("Building interface field '{0}.{1}'", type.Name, name);
      FieldInfo field = implField.Clone();
      field.Name = name;
      field.ReflectedType = type;
      field.DeclaringType = type;
      field.IsInherited = false;
      type.Fields.Add(field);

      if (implField.Column!=null)
        field.Column = ColumnBuilder.BuildInterfaceColumn(field, implField.Column);
    }

    public static void BuildReferenceField(FieldInfo field)
    {
      BuildingContext context = BuildingScope.Context;
      TypeInfo type = context.Model.Types[field.ValueType];
      IEnumerable<FieldInfo> fields = type.Hierarchy.Fields.Keys.Join(type.Fields, key => key.Name,
        fld => fld.Name, (key, fld) => fld);

      BuildNestedFields(field, fields);
    }

    public static void BuildStructureField(FieldInfo field)
    {
      BuildingContext context = BuildingScope.Context;
      TypeInfo type = context.Model.Types[field.ValueType];

      BuildNestedFields(field, type.Fields);
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
          target.ReflectedType.Fields.Add(clone);
        }
      }
    }
  }
}