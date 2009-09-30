// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.20

using System;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  [Serializable]
  internal static class ModelDefBuilder
  {
    public static void Run()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ModelDefinition)) {

        BuildingContext context = BuildingContext.Current;
        context.ModelDef = new DomainModelDef();

        using (Log.InfoRegion(Strings.LogDefiningX, Strings.Types)) {
          var typeFilter = GetTypeFilter();
          foreach (var type in context.Configuration.Types) {
            if (typeFilter.Invoke(type))
              ProcessType(type);
          }
        }
      }
    }

    #region Automatic processing members

    public static TypeDef ProcessType(Type type)
    {
      var modelDef = BuildingContext.Current.ModelDef;

      if (modelDef.Types.Contains(type))
        return modelDef.Types[type];

      using (Log.InfoRegion(Strings.LogDefiningX, type.GetFullName())) {

        var typeDef = DefineType(type);
        if (modelDef.Types.Contains(typeDef.Name))
          throw new DomainBuilderException(string.Format(Strings.ExTypeWithNameXIsAlreadyDefined, typeDef.Name));

        HierarchyDef hierarchyDef = null;
        if (typeDef.IsEntity && !typeDef.UnderlyingType.IsGenericTypeDefinition) {
          // HierarchyRootAttribute is required for hierarchy root
          var hra = type.GetAttribute<HierarchyRootAttribute>(AttributeSearchOptions.Default);
          if (hra!=null)
            hierarchyDef = DefineHierarchy(typeDef, hra);
        }

        ProcessProperties(typeDef, hierarchyDef);

        if (typeDef.IsEntity || typeDef.IsInterface)
          ProcessIndexes(typeDef);

        if (hierarchyDef!=null) {
          Log.Info("Hierarchy: '{0}'", typeDef.Name);
          modelDef.Hierarchies.Add(hierarchyDef);
        }
        modelDef.Types.Add(typeDef);

        return typeDef;
      }
    }

    public static void ProcessProperties(TypeDef typeDef, HierarchyDef hierarchyDef)
    {
      var fieldFilter = GetFieldFilter();
      var properties = typeDef.UnderlyingType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      foreach (var propertyInfo in properties) {
        // Domain builder stage-related filter
        if (!fieldFilter.Invoke(propertyInfo))
          continue;

        // FieldAttribute presence is required
        var fa = propertyInfo.GetAttribute<FieldAttribute>(AttributeSearchOptions.InheritAll);
        if (fa==null)
          continue;

        FieldDef field = DefineField(propertyInfo);

        // Declared & inherited fields must be processed for hierarchy root
        if (hierarchyDef != null) {
          typeDef.Fields.Add(field);
          Log.Info("Field: '{0}'", field.Name);
          var ka = propertyInfo.GetAttribute<KeyAttribute>(AttributeSearchOptions.InheritAll);
          if (ka == null)
            continue;
          AttributeProcessor.Process(hierarchyDef, field, ka);
        }
        // Only declared properies must be processed in other cases
        else if (propertyInfo.DeclaringType==propertyInfo.ReflectedType) {
          typeDef.Fields.Add(field);
          Log.Info("Field: '{0}'", field.Name);
        }
      }
    }

    public static void ProcessIndexes(TypeDef typeDef)
    {
      var ia = typeDef.UnderlyingType.GetAttributes<IndexAttribute>(AttributeSearchOptions.Default);
      if (ia==null || ia.Length==0)
        return;

      foreach (IndexAttribute attribute in ia) {
        IndexDef index = DefineIndex(typeDef, attribute);
        if (typeDef.Indexes.Contains(index.Name))
          throw new DomainBuilderException(
            string.Format(Strings.ExIndexWithNameXIsAlreadyRegistered, index.Name));

        typeDef.Indexes.Add(index);
        Log.Info("Index: '{0}'", index.Name);
      }
    }

    #endregion

    #region Definition-related members

    public static TypeDef DefineType(Type type)
    {
      var typeDef = new TypeDef(type);
      typeDef.Name = BuildingContext.Current.NameBuilder.BuildTypeName(typeDef);

      if (!(type.UnderlyingSystemType.IsInterface || type.IsAbstract)) {
        var sta = typeDef.UnderlyingType.GetAttribute<SystemTypeAttribute>(AttributeSearchOptions.Default);
        if (sta!=null)
          AttributeProcessor.Process(typeDef, sta);
      }
      if (typeDef.IsInterface) {
        var mva = typeDef.UnderlyingType.GetAttribute<MaterializedViewAttribute>(AttributeSearchOptions.Default);
        if (mva!=null)
          AttributeProcessor.Process(typeDef, mva);
      }
      var ma = typeDef.UnderlyingType.GetAttribute<TableMappingAttribute>(AttributeSearchOptions.Default);
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
      // Persistent indexers are not supported
      var indexParameters = propertyInfo.GetIndexParameters();
      if (indexParameters.Length > 0)
        throw new DomainBuilderException(Strings.ExIndexedPropertiesAreNotSupported);

      var fieldDef = new FieldDef(propertyInfo);
      fieldDef.Name = BuildingContext.Current.NameBuilder.BuildFieldName(fieldDef);

      var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>(AttributeSearchOptions.InheritAll);
      if (fieldAttribute!=null) {
        AttributeProcessor.Process(fieldDef, fieldAttribute);
        var associationAttribute = propertyInfo.GetAttribute<AssociationAttribute>(AttributeSearchOptions.InheritAll);
        if (associationAttribute!=null)
          AttributeProcessor.Process(fieldDef, associationAttribute);
        var mappingAttributes = propertyInfo.GetAttributes<FieldMappingAttribute>(AttributeSearchOptions.InheritAll);
        foreach (var fieldMappingAttribute in mappingAttributes)
          AttributeProcessor.Process(fieldDef, fieldMappingAttribute);
        var versionAttribute = propertyInfo.GetAttribute<VersionAttribute>(AttributeSearchOptions.InheritAll);
        if (versionAttribute!=null)
          AttributeProcessor.Process(fieldDef, versionAttribute);
      }

      return fieldDef;
    }

    public static FieldDef DefineField(Type declaringType, string name, Type valueType)
    {
      // Prefer standard field definition flow if corresponding property exists
      PropertyInfo propertyInfo = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic);
      if (propertyInfo!=null)
        return DefineField(propertyInfo);

      return new FieldDef(valueType) {Name = name};
    }

    public static IndexDef DefineIndex(TypeDef typeDef, IndexAttribute attribute)
    {
      var index = new IndexDef {IsSecondary = true};
      AttributeProcessor.Process(index, attribute);

      if (string.IsNullOrEmpty(index.Name) && index.KeyFields.Count > 0)
        index.Name = BuildingContext.Current.NameBuilder.BuildIndexName(typeDef, index);

      return index;
    }

    #endregion

    #region Helper members (to reduce cohesion)

    private static Func<Type, bool> GetTypeFilter()
    {
      var filter = BuildingContext.Current.BuilderConfiguration.TypeFilter 
        ?? TypeFilteringHelper.IsPersistentType;
      return (t => filter(t) && t!=typeof (EntitySetItem<,>));
    }

    private static Func<PropertyInfo, bool> GetFieldFilter()
    {
      return BuildingContext.Current.BuilderConfiguration.FieldFilter ?? (p => true);
    }

    #endregion
  }
}