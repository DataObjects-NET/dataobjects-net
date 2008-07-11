// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

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
        BuildReferencedType(fieldDef);          

        AssociationBuilder.BuildAssociation(fieldDef, field);
        return field;
      }

      if (field.IsEntity) {
        BuildReferencedType(fieldDef);

        AssociationBuilder.BuildAssociation(fieldDef, field);

        TypeDef typeDef = BuildingScope.Context.Definition.Types[field.DeclaringType.UnderlyingType];
        typeDef.Indexes.Add(IndexBuilder.DefineForeignKey(typeDef, fieldDef));

        context.ComplexFields.Add(field);
      }

      if (field.IsStructure) {
        BuildReferencedType(fieldDef);          
        context.ComplexFields.Add(field);
      }

      if (field.IsPrimitive)
        field.Column = ColumnBuilder.BuildColumn(field);

      if (field.IsPrimaryKey && field.LazyLoad)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.FieldXCanTBeLoadOnDemandAsItIsIncludedInPrimaryKey, field.Name));

      return field;
    }

    private static void BuildReferencedType(FieldDef fieldDef)
    {
      TypeDef typeDef;

      if (!BuildingScope.Context.Definition.Types.TryGetValue(fieldDef.ValueType, out typeDef))
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ExTypeXIsNotRegisteredInTheModel, fieldDef.ValueType.FullName));

      TypeBuilder.BuildType(typeDef);      
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
      else 
        if (ancsField.Column!=null)
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