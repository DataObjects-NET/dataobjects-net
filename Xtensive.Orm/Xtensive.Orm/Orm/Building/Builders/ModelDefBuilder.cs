// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Sorting;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Building.Builders
{
  internal static class ModelDefBuilder
  {
    public static void Run()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ModelDefinition)) {

        var context = BuildingContext.Demand();
        context.ModelDef = new DomainModelDef();

        using (Log.InfoRegion(Strings.LogDefiningX, Strings.Types)) {
          ProcessTypes();
          AdjustTypeDiscriminatorValues();
        }
      }
    }

    public static void ProcessTypes()
    {
      var context = BuildingContext.Demand();
      var typeFilter = GetTypeFilter();
      var closedGenericTypes = new List<Type>();
      var openGenericTypes = new List<Type>();

      while (context.Types.Count != 0) {
        var type = context.Types.Dequeue();
        if (!typeFilter(type))
          continue;
        if (type.IsGenericType) {
          if (type.IsGenericTypeDefinition)
            openGenericTypes.Add(type);
          else
            closedGenericTypes.Add(type);
        }
        else
          ProcessType(type);
      }
      closedGenericTypes = closedGenericTypes.Where(t => !t.FullName.IsNullOrEmpty()).ToList();
      List<Node<Type, object>> loops;
      closedGenericTypes = TopologicalSorter.Sort(
        closedGenericTypes,
        (first, second) => second.GetGenericArguments().Any(argument => argument.IsAssignableFrom(first)), 
        out loops);
      foreach (var type in closedGenericTypes)
        ProcessType(type);
      foreach (var type in openGenericTypes)
        ProcessType(type);
    }

    #region Automatic processing members

    public static TypeDef ProcessType(Type type)
    {
      var modelDef = BuildingContext.Demand().ModelDef;

      var typeDef = modelDef.Types.TryGetValue(type);
      if (typeDef != null)
        return typeDef;
      
      using (Log.InfoRegion(Strings.LogDefiningX, type.GetFullName())) {
        typeDef = DefineType(type);
        if (modelDef.Types.Contains(typeDef.Name))
          throw new DomainBuilderException(string.Format(Strings.ExTypeWithNameXIsAlreadyDefined, typeDef.Name));

        HierarchyDef hierarchyDef = null;
        if (typeDef.IsEntity) {
          // HierarchyRootAttribute is required for hierarchy root
          var hra = type.GetAttribute<HierarchyRootAttribute>(AttributeSearchOptions.Default);
          if (hra!=null)
            hierarchyDef = DefineHierarchy(typeDef, hra);
        }

        ProcessProperties(typeDef, hierarchyDef);

        if (typeDef.IsEntity || typeDef.IsInterface)
          ProcessIndexes(typeDef);

        if (hierarchyDef!=null) {
          Log.Info(Strings.LogHierarchyX, typeDef.Name);
          modelDef.Hierarchies.Add(hierarchyDef);
        }
        modelDef.Types.Add(typeDef);

        ProcessFullTextIndexes(typeDef);

        return typeDef;
      }
    }

    private static void ProcessFullTextIndexes(TypeDef typeDef)
    {
      var fullTextIndexDef = new FullTextIndexDef(typeDef);
      var modelDef = BuildingContext.Demand().ModelDef;
      var hierarchy = modelDef.FindHierarchy(typeDef);
      if (hierarchy == null)
        return;

      foreach (var fieldDef in typeDef.Fields.Where(f => f.UnderlyingProperty != null)) {
        var fullTextAttribute = fieldDef.UnderlyingProperty
          .GetAttribute<FullTextAttribute>(AttributeSearchOptions.InheritAll);
        if (fullTextAttribute == null) 
          continue;

        var fullTextField = new FullTextFieldDef(fieldDef.Name, fullTextAttribute.Analyzed) {
          Configuration = fullTextAttribute.Configuration, 
        };
        fullTextIndexDef.Fields.Add(fullTextField);
      }
      if (fullTextIndexDef.Fields.Count > 0)
        modelDef.FullTextIndexes.Add(fullTextIndexDef);
    }

    public static void ProcessProperties(TypeDef typeDef, HierarchyDef hierarchyDef)
    {
      var context = BuildingContext.Demand();
      var fieldFilter = GetFieldFilter();
      var properties = typeDef.UnderlyingType.GetProperties(
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      foreach (var propertyInfo in properties) {
        // Domain builder stage-related filter
        if (!fieldFilter.Invoke(propertyInfo))
          continue;

        // FieldAttribute presence is required
        var fieldAttributes = GetFieldAttributes<FieldAttribute>(propertyInfo);
        if (fieldAttributes.Length==0)
          continue;

        var field = DefineField(propertyInfo, fieldAttributes);

        // Declared & inherited fields must be processed for hierarchy root
        if (hierarchyDef != null) {
          typeDef.Fields.Add(field);
          Log.Info(Strings.LogFieldX, field.Name);

          var keyAttributes = propertyInfo.GetAttribute<KeyAttribute>(AttributeSearchOptions.InheritAll);
          if (keyAttributes!=null)
            AttributeProcessor.Process(hierarchyDef, field, keyAttributes);
        }
        // Only declared properies must be processed in other cases
        else if (propertyInfo.DeclaringType==propertyInfo.ReflectedType) {
          typeDef.Fields.Add(field);
          Log.Info(Strings.LogFieldX, field.Name);
        }

        // Checking whether property type is registered in model
        var propertyType = field.UnderlyingProperty.PropertyType;
        if (propertyType.IsGenericParameter)
          continue;
        if (propertyType.IsSubclassOf(typeof(Persistent)) && !context.ModelDef.Types.Contains(propertyType))
          context.Types.Enqueue(propertyType);
      }
    }

    private static void ProcessIndexes(TypeDef typeDef)
    {
      var targets = typeDef.Fields
        .Where(f => f.IsIndexed)
        .Select(f => new IndexAttribute(f.Name))
        .Concat(typeDef.UnderlyingType.GetAttributes<IndexAttribute>(AttributeSearchOptions.InheritAll) ??
                EnumerableUtils<IndexAttribute>.Empty)
        .ToList();

      if (targets.Count == 0)
        return;

      foreach (var attribute in targets) {
        var index = DefineIndex(typeDef, attribute);
        if (typeDef.Indexes.Contains(index.Name))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexWithNameXIsAlreadyRegistered, index.Name));

        typeDef.Indexes.Add(index);
        Log.Info(Strings.LogIndexX, index.Name);
      }
    }

    #endregion

    #region Definition-related members

    public static TypeDef DefineType(Type type)
    {
      var typeDef = new TypeDef(type);
      typeDef.Name = BuildingContext.Demand().NameBuilder.BuildTypeName(typeDef);

      if (!(type.UnderlyingSystemType.IsInterface || type.IsAbstract)) {
          var sta = type.GetAttribute<SystemTypeAttribute>(AttributeSearchOptions.Default);
          if (sta!=null)
            AttributeProcessor.Process(typeDef, sta);
      }
      if (typeDef.IsEntity) {
        var tdva = type.GetAttribute<TypeDiscriminatorValueAttribute>(AttributeSearchOptions.Default);
        if (tdva != null)
          AttributeProcessor.Process(typeDef, tdva);
      }
      if (typeDef.IsInterface) {
        var mva = type.GetAttribute<MaterializedViewAttribute>(AttributeSearchOptions.Default);
        if (mva!=null)
          AttributeProcessor.Process(typeDef, mva);
      }
      var ma = type.GetAttribute<TableMappingAttribute>(AttributeSearchOptions.Default);
      if (ma!=null)
        AttributeProcessor.Process(typeDef, ma);
      return typeDef;
    }

    public static HierarchyDef DefineHierarchy(TypeDef typeDef)
    {
      // Prefer standard hierarchy definition flow
      var hra = typeDef.UnderlyingType.GetAttribute<HierarchyRootAttribute>(AttributeSearchOptions.Default);
      if (hra!=null)
        return DefineHierarchy(typeDef, hra);

      Validator.ValidateHierarchyRoot(typeDef);
      var result = new HierarchyDef(typeDef);
      return result;
    }

    public static HierarchyDef DefineHierarchy(TypeDef typeDef, HierarchyRootAttribute attribute)
    {
      Validator.ValidateHierarchyRoot(typeDef);

      var hierarchyDef = new HierarchyDef(typeDef);
      AttributeProcessor.Process(hierarchyDef, attribute);

      // KeyGeneratorAttribute is optional
      var kga = typeDef.UnderlyingType.GetAttribute<KeyGeneratorAttribute>(AttributeSearchOptions.InheritAll);
      if (kga!=null)
        AttributeProcessor.Process(hierarchyDef, kga);

      return hierarchyDef;
    }

    public static FieldDef DefineField(PropertyInfo propertyInfo)
    {
      return DefineField(propertyInfo, GetFieldAttributes<FieldAttribute>(propertyInfo));
    }

    public static FieldDef DefineField(PropertyInfo propertyInfo, FieldAttribute[] fieldAttributes)
    {
      // Persistent indexers are not supported
      var indexParameters = propertyInfo.GetIndexParameters();
      if (indexParameters.Length > 0)
        throw new DomainBuilderException(Strings.ExIndexedPropertiesAreNotSupported);

      var fieldDef = new FieldDef(propertyInfo);
      fieldDef.Name = BuildingContext.Demand().NameBuilder.BuildFieldName(fieldDef);

      if (fieldAttributes.Length > 0) {
        foreach (var attribute in fieldAttributes)
          AttributeProcessor.Process(fieldDef, attribute);
        // Association
        var associationAttributes = GetFieldAttributes<AssociationAttribute>(propertyInfo);
        foreach (var attribute in associationAttributes)
          AttributeProcessor.Process(fieldDef, attribute);
        // Mapping name
        var mappingAttributes = GetFieldAttributes<FieldMappingAttribute>(propertyInfo);
        foreach (var attribute in mappingAttributes)
          AttributeProcessor.Process(fieldDef, attribute);
        // Type discriminator
        var typeDiscriminatorAttribute = propertyInfo.GetAttribute<TypeDiscriminatorAttribute>(AttributeSearchOptions.InheritAll);
        if (typeDiscriminatorAttribute!=null)
          AttributeProcessor.Process(fieldDef, typeDiscriminatorAttribute);
        // Version
        var versionAttribute = propertyInfo.GetAttribute<VersionAttribute>(AttributeSearchOptions.InheritAll);
        if (versionAttribute!=null)
          AttributeProcessor.Process(fieldDef, versionAttribute);
      }

      return fieldDef;
    }

    public static FieldDef DefineField(Type declaringType, string name, Type valueType)
    {
      // Prefer standard field definition flow if corresponding property exists
      var propertyInfo = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic);
      if (propertyInfo!=null)
        return DefineField(propertyInfo);

      return new FieldDef(valueType) {Name = name};
    }

    public static IndexDef DefineIndex(TypeDef typeDef, IndexAttribute attribute)
    {
      var index = new IndexDef(typeDef) {IsSecondary = true};
      AttributeProcessor.Process(index, attribute);

      if (string.IsNullOrEmpty(index.Name) && index.KeyFields.Count > 0)
        index.Name = BuildingContext.Demand().NameBuilder.BuildIndexName(typeDef, index);

      return index;
    }

    private static void AdjustTypeDiscriminatorValues()
    {
      var modelDef = BuildingContext.Demand().ModelDef;
      var hierarchiesToProcess = modelDef.Hierarchies
        .Select(hierarchy => new {
          Root = hierarchy.Root,
          DiscriminatorField = hierarchy.Root.Fields.FirstOrDefault(field => field.IsTypeDiscriminator)
        })
        .Where(item => item.Root!=null && item.DiscriminatorField!=null);
      foreach (var item in hierarchiesToProcess) {
        var rootType = item.Root.UnderlyingType;
        var field = item.DiscriminatorField;
        var inheritorsToProcess = modelDef.Types
          .Where(type => type.TypeDiscriminatorValue!=null && rootType.IsAssignableFrom(type.UnderlyingType));
        foreach (var type in inheritorsToProcess)
          type.TypeDiscriminatorValue = ValueTypeBuilder.AdjustValue(field, type.TypeDiscriminatorValue);
      }
    }

    #endregion

    #region Helper members (to reduce cohesion)

    private static T[] GetFieldAttributes<T>(PropertyInfo property)
      where T : Attribute
    {
      var attributes = property.GetAttributes<T>(AttributeSearchOptions.InheritAll);
      // Attributes will contain attributes from all inheritance chain
      // with the most specific type first.
      // Reverse them for correct processing (i.e. descendants override settings from base).
      Array.Reverse(attributes);
      return attributes;
    }


    private static Func<Type, bool> GetTypeFilter()
    {
      var filter = BuildingContext.Demand().BuilderConfiguration.TypeFilter 
        ?? DomainTypeRegistry.IsPersistentType;
      return (t => filter(t) && t!=typeof (EntitySetItem<,>));
    }

    private static Func<PropertyInfo, bool> GetFieldFilter()
    {
      return BuildingContext.Demand().BuilderConfiguration.FieldFilter ?? (p => true);
    }

    #endregion
  }
}