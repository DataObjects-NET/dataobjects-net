// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Building.DependencyGraph;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class TypeBuilder
  {
    /// <summary>
    /// Builds the <see cref="TypeInfo"/> instance, its key fields and <see cref="HierarchyInfo"/> for hierarchy root.
    /// </summary>
    /// <param name="typeDef"><see cref="TypeDef"/> instance.</param>
    public static TypeInfo BuildType(TypeDef typeDef)
    {
      Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetShortName());
      var context = BuildingContext.Current;

      var typeInfo = new TypeInfo(context.Model, typeDef.Attributes) {
        UnderlyingType = typeDef.UnderlyingType,
        Name = typeDef.Name,
        MappingName = typeDef.MappingName,
        HasVersionRoots = typeDef.UnderlyingType.GetInterfaces().Any(type => type == typeof (IHasVersionRoots))
      };
      context.Model.Types.Add(typeInfo);

      // Registering connections between typeInfo & its ancestors
      var node = context.DependencyGraph.TryGetNode(typeDef);
      if (node != null) {
        foreach (var edge in node.OutgoingEdges.Where(e => e.Kind == EdgeKind.Implementation || e.Kind == EdgeKind.Inheritance)) {
          var baseType = context.Model.Types[edge.Head.Value.UnderlyingType];
          switch (edge.Kind) {
            case EdgeKind.Inheritance:
              context.Model.Types.RegisterInheritance(baseType, typeInfo);
              break;
            case EdgeKind.Implementation:
              context.Model.Types.RegisterImplementation(baseType, typeInfo);
              break;
          }
        }
      }

      if (typeDef.IsEntity) {
        var hierarchyDef = context.ModelDef.FindHierarchy(typeDef);

        // Is type a hierarchy root?
        if (typeInfo.UnderlyingType==hierarchyDef.Root.UnderlyingType) {
          foreach (var keyField in hierarchyDef.KeyFields) {
            var fieldInfo = BuildDeclaredField(BuildingContext.Current, typeInfo, typeDef.Fields[keyField.Name]);
            fieldInfo.IsPrimaryKey = true;
          }
          typeInfo.Hierarchy = BuildHierarchy(context, typeInfo, hierarchyDef);
        }
        else {
          var root = context.Model.Types[hierarchyDef.Root.UnderlyingType];
          typeInfo.Hierarchy = root.Hierarchy;
          foreach (var fieldInfo in root.Fields.Where(f => f.IsPrimaryKey && f.Parent == null))
            BuildInheritedField(context, typeInfo, fieldInfo);
        }
      }
      else if (typeDef.IsInterface) {
        var hierarchyDef = context.ModelDef.FindHierarchy(typeDef.Implementors[0]);
        foreach (var keyField in hierarchyDef.KeyFields) {
          var fieldInfo = BuildDeclaredField(BuildingContext.Current, typeInfo, typeDef.Fields[keyField.Name]);
          fieldInfo.IsPrimaryKey = true;
        }
      }

      return typeInfo;
    }

    /// <summary>
    /// Builds the fields.
    /// </summary>
    /// <param name="typeDef">The <see cref="TypeDef"/> instance.</param>
    /// <param name="typeInfo">The corresponding <see cref="TypeInfo"/> instance.</param>
    public static void BuildFields(TypeDef typeDef, TypeInfo typeInfo)
    {
      var context = BuildingContext.Current;

      if (typeInfo.IsInterface) {
        var sourceFields = typeInfo.GetInterfaces()
          .SelectMany(i => i.Fields)
          .Where(f => !f.IsPrimaryKey && f.Parent == null);
        foreach (var srcField in sourceFields) {
          if (!typeInfo.Fields.Contains(srcField.Name))
            BuildInheritedField(context, typeInfo, srcField);
        }
      }
      else {
        var ancestor = typeInfo.GetAncestor();
        if (ancestor != null) {
          foreach (var srcField in ancestor.Fields.Where(f => !f.IsPrimaryKey && f.Parent == null)) {
            FieldDef fieldDef;
            if (typeDef.Fields.TryGetValue(srcField.Name, out fieldDef))
              BuildDeclaredField(context, typeInfo, fieldDef);
            else
              BuildInheritedField(context, typeInfo, srcField);
          }
          foreach (var pair in ancestor.FieldMap)
            typeInfo.FieldMap.Add(pair.Key, typeInfo.Fields[pair.Value.Name]);
        }
      }

      foreach (var fieldDef in typeDef.Fields) {
        FieldInfo field;
        if (typeInfo.Fields.TryGetValue(fieldDef.Name, out field)) {
          if (field.ValueType!=fieldDef.ValueType)
            throw new DomainBuilderException(
              String.Format(Strings.ExFieldXIsAlreadyDefinedInTypeXOrItsAncestor, fieldDef.Name, typeInfo.Name));
        }
        else
          BuildDeclaredField(context, typeInfo, fieldDef);
      }
      typeInfo.Columns.AddRange(typeInfo.Fields.Where(f => f.Column!=null).Select(f => f.Column));

      if (typeInfo.IsEntity && !IsAuxiliaryType(typeInfo)) {
        foreach (var @interface in typeInfo.GetInterfaces())
          BuildFieldMap(context, @interface, typeInfo);
        ProcessImplementedAssociations(context, typeInfo);
      }
    }

    #region Private members

    private static void BuildFieldMap(BuildingContext context, TypeInfo @interface, TypeInfo implementor)
    {
      foreach (var field in @interface.Fields.Where(f => f.IsDeclared)) {
        string explicitName = context.NameBuilder.BuildExplicitFieldName(field.DeclaringType, field.Name);
        FieldInfo implField;
        if (implementor.Fields.TryGetValue(explicitName, out implField))
          implField.IsExplicit = true;
        else {
          if (!implementor.Fields.TryGetValue(field.Name, out implField))
            throw new DomainBuilderException(
              String.Format(Strings.TypeXDoesNotImplementYZField, implementor.Name, @interface.Name, field.Name));
        }

        implField.IsInterfaceImplementation = true;

        if (!implementor.FieldMap.ContainsKey(field))
          implementor.FieldMap.Add(field, implField);
        else
          implementor.FieldMap.Override(field, implField);
      }
    }

    private static void ProcessImplementedAssociations(BuildingContext context, TypeInfo typeInfo)
    {
      var fields = typeInfo.Fields.Where(f => f.IsDeclared && f.IsInterfaceImplementation && (f.IsEntity || f.IsEntitySet));
      foreach (var fieldInfo in fields) {
        var interfaceField = typeInfo.FieldMap.GetImplementedInterfaceFields(fieldInfo).FirstOrDefault();
        if (interfaceField == null)
          continue;
        context.Model.Associations.Remove(fieldInfo.Association);
        fieldInfo.Association = interfaceField.Association;
      }
    }

    private static FieldInfo BuildDeclaredField(BuildingContext context, TypeInfo type, FieldDef fieldDef)
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
        AssociationBuilder.BuildAssociation(fieldDef, fieldInfo);
        return fieldInfo;
      }

      if (fieldInfo.IsEntity) {
        var fields = context.Model.Types[fieldInfo.ValueType].Fields.Where(f => f.IsPrimaryKey);
        BuildNestedFields(context, fieldInfo, fields);

        if (!IsAuxiliaryType(type))
          AssociationBuilder.BuildAssociation(fieldDef, fieldInfo);
      }

      if (fieldInfo.IsStructure)
        BuildNestedFields(context, fieldInfo, context.Model.Types[fieldInfo.ValueType].Fields);

      if (fieldInfo.IsPrimitive)
        fieldInfo.Column = BuildDeclaredColumn(context, fieldInfo);
      return fieldInfo;
    }

    private static void BuildInheritedField(BuildingContext context, TypeInfo type, FieldInfo inheritedField)
    {
      Log.Info(Strings.LogBuildingInheritedFieldXY, type.Name, inheritedField.Name);
      var field = inheritedField.Clone();
      type.Fields.Add(field);
      field.ReflectedType = type;
      field.DeclaringType = inheritedField.DeclaringType;
      field.IsInherited = true;

      BuildNestedFields(context, field, inheritedField.Fields);

      if (inheritedField.Column!=null)
        field.Column = BuildInheritedColumn(context, field, inheritedField.Column);
    }

    private static void BuildNestedFields(BuildingContext context, FieldInfo target, IEnumerable<FieldInfo> fields)
    {
      var buffer = fields.ToList();

      foreach (FieldInfo field in buffer) {
        var clone = field.Clone();
        clone.IsSystem = false;
        clone.IsLazyLoad = field.IsLazyLoad || target.IsLazyLoad;
        if (target.IsDeclared) {
          clone.Name = context.NameBuilder.BuildNestedFieldName(target, field);
          clone.OriginalName = field.OriginalName;
          // One-field reference
          if (target.IsEntity && buffer.Count == 1)
            clone.MappingName = target.MappingName;
          else
            clone.MappingName = context.NameBuilder.BuildMappingName(target, field);
        }
        if (target.Fields.Contains(clone.Name))
          continue;
        clone.Parent = target;
        target.ReflectedType.Fields.Add(clone);

        if (field.IsStructure || field.IsEntity) {
          BuildNestedFields(context, clone, field.Fields);
          foreach (FieldInfo clonedFields in clone.Fields)
            target.Fields.Add(clonedFields);
        }
        else {
          if (field.Column!=null)
            clone.Column = BuildInheritedColumn(context, clone, field.Column);
        }
        if (clone.IsEntity && !IsAuxiliaryType(clone.ReflectedType)) {
          var origin = context.Model.Associations.Find(context.Model.Types[field.ValueType], true).Where(a => a.OwnerField==field).FirstOrDefault();
          if (origin!=null) {
            AssociationBuilder.BuildAssociation(origin, clone);
            context.DiscardedAssociations.Add(origin);
          }
        }
      }
    }

    private static bool IsAuxiliaryType(TypeInfo type)
    {
      if (!type.IsEntity)
        return false;
      var underlyingBaseType = type.UnderlyingType.BaseType;
      return underlyingBaseType!=null
        && underlyingBaseType.IsGenericType
          && underlyingBaseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
    }

    private static ColumnInfo BuildDeclaredColumn(BuildingContext context, FieldInfo field)
    {
      ColumnInfo column;
      if (field.ValueType==typeof (Key))
        column = new ColumnInfo(field, typeof (string));
      else
        column = new ColumnInfo(field);
      column.Name = context.NameBuilder.BuildColumnName(field, column);
      column.IsNullable = field.IsNullable;

      return column;
    }

    private static ColumnInfo BuildInheritedColumn(BuildingContext context, FieldInfo field, ColumnInfo ancestor)
    {
      var column = ancestor.Clone();
      column.Field = field;
      column.Name = context.NameBuilder.BuildColumnName(field, ancestor);
      column.IsDeclared = field.IsDeclared;
      column.IsPrimaryKey = field.IsPrimaryKey;
      column.IsNullable = field.IsNullable;
      column.IsSystem = field.IsSystem;

      return column;
    }

    private static HierarchyInfo BuildHierarchy(BuildingContext context, TypeInfo root, HierarchyDef hierarchyDef)
    {
      var keyColumns = root.Fields
          .Where(f => f.IsPrimaryKey && f.Column != null)
          .Select(f => f.Column)
          .ToList();
      var descriptor = TupleDescriptor.Create(keyColumns.Select(c => c.ValueType));
      var keyProviderInfo = context.Model.KeyProviders
        .SingleOrDefault(kp => 
          kp.KeyGeneratorType != null && 
          kp.KeyGeneratorType == hierarchyDef.KeyGenerator && 
          kp.TupleDescriptor == descriptor);
      if (keyProviderInfo == null) {
        var typeIdColumnIndex = -1;
        if (hierarchyDef.IncludeTypeId)
          for (int i = 0; i < keyColumns.Count; i++)
            if (keyColumns[i].Field.IsTypeId)
              typeIdColumnIndex = i;
        var keyGeneratorType = hierarchyDef.KeyGenerator;
        var cacheSize = hierarchyDef.KeyGeneratorCacheSize.HasValue && hierarchyDef.KeyGeneratorCacheSize > 0
          ? hierarchyDef.KeyGeneratorCacheSize.Value
          : context.Configuration.KeyGeneratorCacheSize;

        keyProviderInfo = new KeyProviderInfo(descriptor, keyGeneratorType, typeIdColumnIndex, cacheSize);
        keyProviderInfo.Name = root.Name;
        if (keyProviderInfo.KeyGeneratorType==typeof (KeyGenerator))
          keyProviderInfo.MappingName = context.NameBuilder.BuildGeneratorName(keyProviderInfo);
        context.Model.KeyProviders.Add(keyProviderInfo);
      }

      var hierarchy = new HierarchyInfo(root, hierarchyDef.Schema, keyProviderInfo) {
        Name = root.Name
      };
      context.Model.Hierarchies.Add(hierarchy);
      return hierarchy;
    }

    #endregion
  }
}