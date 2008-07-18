// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
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
        if (IsDeclaredAsPersistent(propertyInfo))
          try {
            fields.Add(
              DefineField(typeDef, propertyInfo));
          }
          catch (DomainBuilderException e) {
            BuildingScope.Context.RegistError(e);
          }        
      
      return fields;
    }

    private static bool IsDeclaredAsPersistent(PropertyInfo propertyInfo)
    {            
      if (propertyInfo.GetAttribute<FieldAttribute>(false)==null)
        return false;

      if (propertyInfo.DeclaringType!=propertyInfo.ReflectedType)
        return false;

      return true;
    }

    /// <summary>
    /// Defines the field by specified property.
    /// </summary>
    /// <param name="typeDef">The type definition.</param>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the property.</param>
    /// <returns>Defined field.</returns>
    public static FieldDef DefineField(TypeDef typeDef, PropertyInfo propertyInfo)
    {
      BuildingContext context = BuildingScope.Context;
      Log.Info("Defining field '{0}'", propertyInfo.Name);            
      
      ValidateValueType(propertyInfo.PropertyType, typeDef.UnderlyingType);

      // We do not support "persistent" indexers
      ParameterInfo[] indexParameters = propertyInfo.GetIndexParameters();

      if (indexParameters.Length > 0)
        throw new DomainBuilderException(Resources.Strings.IndexedPropertiesAreNotSupported);

      FieldDef fieldDef = new FieldDef(propertyInfo);
      fieldDef.Name = context.NameProvider.BuildName(fieldDef);

      AttributeProcessor.Process(fieldDef, 
        propertyInfo.GetAttribute<FieldAttribute>(false));

      return fieldDef;
    }

    /// <summary>
    /// Defines the field manually (without property).
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="valueType">The value type.</param>
    /// <param name="declaringType">The type where this field is declaring.</param>
    /// <returns>Defined field.</returns>
    public static FieldDef DefineField(string name, Type valueType, Type declaringType)
    {
      ValidateValueType(valueType, declaringType);
      return new FieldDef(valueType) {Name = name};
    }

    /// <summary>
    /// Builds the declared field.
    /// </summary>
    /// <param name="type">The type field belongs to.</param>
    /// <param name="fieldDef">The field definition.</param>
    public static void BuildDeclaredField(TypeInfo type, FieldDef fieldDef)
    {
      Log.Info("Building declared field '{0}.{1}'", type.Name, fieldDef.Name);
      
      FieldInfo field = new FieldInfo(type, fieldDef.Attributes)
        {
          UnderlyingProperty = fieldDef.UnderlyingProperty, 
          Name = fieldDef.Name, 
          MappingName = fieldDef.MappingName, 
          ValueType = fieldDef.ValueType, 
          Length = fieldDef.Length
        };

      type.Fields.Add(field);

      ValidateValueType(field.ValueType, type.UnderlyingType);

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

    private static void ValidateValueType(Type valueType, Type declaringType)
    {
      if (valueType.IsPrimitive || valueType.IsEnum || typeof (string)==valueType 
        || typeof(byte[])==valueType || typeof(Guid)==valueType)
        return;

      if (typeof (Entity).IsAssignableFrom(valueType))
        return;            

      if (valueType.IsSubclassOf(typeof (Structure)))
        return;

      if (valueType.IsInterface && typeof(IEntity).IsAssignableFrom(valueType) && valueType!=typeof(IEntity))
        return;

      if (valueType.IsGenericType && valueType.GetGenericTypeDefinition()==typeof (EntitySet<>)) {
        if (declaringType.IsSubclassOf(typeof(Structure)))
          throw new DomainBuilderException(
            string.Format("Structures do not support fields of type '{0}'.", valueType.Name));

        return;        
      }        
      throw new DomainBuilderException(
        string.Format(Resources.Strings.UnsupportedFieldTypeX, valueType.Name));
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

    private static void BuildNestedFields(FieldInfo target, IEnumerable<FieldInfo> fields)
    {
      BuildingContext context = BuildingScope.Context;

      foreach (FieldInfo field in fields) {

        if (field.IsStructure)
          BuildNestedFields(target, field.Fields);
        else {
          FieldInfo clone = field.Clone();
          if (target.IsDeclared)
            clone.Name = BuildingScope.Context.NameProvider.BuildName(target, field);
          if (target.Fields.Contains(clone.Name))
            continue;
          clone.Parent = target;
          if (field.Column!=null)
            clone.Column = ColumnBuilder.BuildInheritedColumn(clone, field.Column);
          target.ReflectedType.Fields.Add(clone);
          if (clone.IsEntity) {
            FieldInfo refField = field;
            AssociationInfo origin = context.Model.Associations.Find(context.Model.Types[field.ValueType]).Where(a => a.ReferencingField==refField).FirstOrDefault();
            if (origin != null) {
              AssociationBuilder.BuildAssociation(origin, clone);
              context.DiscardedAssociations.Add(origin);
            }
          }
        }
      }
    }
  }
}
