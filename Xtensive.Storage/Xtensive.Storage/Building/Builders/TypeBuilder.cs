// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;
using FieldAttributes=Xtensive.Storage.Model.FieldAttributes;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;
using Xtensive.Storage.Resources;
using System.Linq;
using TypeAttributes=Xtensive.Storage.Model.TypeAttributes;

namespace Xtensive.Storage.Building.Builders
{
  internal static class TypeBuilder
  {
    /// <exception cref="DomainBuilderException">Type is already defined.</exception>
    public static TypeDef DefineType(Type type)
    {
      var context = BuildingContext.Current;
      using (Log.InfoRegion(Strings.LogDefiningX, type.FullName)) {
        if (context.Definition.Types.Contains(type))
          throw new DomainBuilderException(string.Format(
            Strings.TypeXIsAlreadyDefined, type.FullName));

        var typeDef = new TypeDef(type);

        ProcessSystemTypeAttribute(type, typeDef);
        ProcessEntityAtribute(type, typeDef);
        ProcessMaterializedViewAttribute(type, typeDef);

        typeDef.Name = context.NameBuilder.Build(typeDef);

        if (context.Definition.Types.Contains(typeDef.Name))
          throw new DomainBuilderException(string.Format(
            Strings.TypeWithNameXIsAlreadyDefined, typeDef.Name));

        DefineFields(typeDef);

        return typeDef;
      }
    }

    /// <exception cref="DomainBuilderException">Field is already registered.</exception>
    private static void DefineFields(TypeDef typeDef)
    {
      var fields = FieldBuilder.DefineFields(typeDef);

      foreach (var fieldDef in fields) {
        if (typeDef.Fields.Contains(fieldDef.Name))
          throw new DomainBuilderException(string.Format(
            Strings.ExFieldWithNameXIsAlreadyRegistered, fieldDef.Name));

        typeDef.Fields.Add(fieldDef);
      }
    }

    private static void ProcessSystemTypeAttribute(Type type, TypeDef typeDef)
    {
      var attribute = type.GetAttribute<SystemTypeAttribute>(AttributeSearchOptions.Default);
      if (attribute!=null) {
        typeDef.Attributes |= TypeAttributes.System;
        BuildingContext.Current.SystemTypeIds[type] = attribute.TypeId;
      }
    }

    private static void ProcessMaterializedViewAttribute(Type type, TypeDef typeDef)
    {
      var materializedViewAttribute = type.GetAttribute<MaterializedViewAttribute>(
        AttributeSearchOptions.Default);
      if (materializedViewAttribute!=null)
        AttributeProcessor.Process(typeDef, materializedViewAttribute);
    }

    private static void ProcessEntityAtribute(Type type, TypeDef typeDef)
    {
      var entityAttribute = type.GetAttribute<EntityAttribute>(
        AttributeSearchOptions.Default);
      if (entityAttribute!=null)
        AttributeProcessor.Process(typeDef, entityAttribute);
    }

    /// <exception cref="DomainBuilderException">Type is not registered.</exception>
    public static void BuildType(Type type)
    {
      var typeDef = BuildingContext.Current.Definition.Types.TryGetValue(type);

      if (typeDef==null)
        throw new DomainBuilderException(string.Format(
          Strings.ExTypeXIsNotRegisteredInTheModel, type.FullName));

      BuildType(typeDef);
    }

    public static void BuildType(TypeDef typeDef)
    {
      var context = BuildingContext.Current;

      if (context.SkippedTypes.Contains(typeDef.UnderlyingType))
        return;

      if (typeDef.IsInterface) {
        if (context.Model.Types.Contains(typeDef.UnderlyingType))
          return;
        foreach (var implementor in context.Definition.Types) {
          if (!implementor.IsEntity)
            continue;
          if (implementor.UnderlyingType.IsInterface)
            continue;
          if (context.Model.Types.Contains(implementor.UnderlyingType))
            continue;
          foreach (var @interface in context.Definition.Types.FindInterfaces(implementor.UnderlyingType))
            if (@interface==typeDef) {
              BuildType(implementor);
              return;
            }
        }
      }

      if (typeDef.IsEntity)
        if (!context.Model.Types.Contains(typeDef.UnderlyingType))
          BuildEntity(typeDef);

      if (typeDef.IsStructure)
        using (context.CircularReferenceFinder.Enter(typeDef.UnderlyingType))
          if (!context.Model.Types.Contains(typeDef.UnderlyingType))
            BuildStructure(typeDef);
    }

    private static void BuildStructure(TypeDef typeDef)
    {
      BuildAnsector(typeDef);

      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.FullName)) {
        var type = CreateType(typeDef);
        ProcessAncestor(type);
        BuildDeclaredFields(type, typeDef);
      }
    }

    private static void BuildEntity(TypeDef typeDef)
    {
      var hierarchyDef = BuildingContext.Current.Definition.FindHierarchy(typeDef);
      if (hierarchyDef==null) {
        Log.Info("Skipping entity '{0}' as it does not belong to any hierarchy thus it cannot be persistent.",
          typeDef.UnderlyingType);
        BuildingContext.Current.SkippedTypes.Add(typeDef.UnderlyingType);
        return;
      }

      BuildAnsector(typeDef);

      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetShortName())) {
        var type = CreateType(typeDef);

        if (type.UnderlyingType==hierarchyDef.Root.UnderlyingType) {
          ProcessSkippedAncestors(typeDef);
          BuildHierarchyRoot(type, typeDef, hierarchyDef);
          BuildSystemFields(type);
          BuildInterfaces(type);
          BuildDeclaredFields(type, typeDef);
          BuildInterfaceFields(type);
        }
        else {
          AssignHierarchy(type);
          ProcessAncestor(type);
          BuildInterfaces(type);
          BuildDeclaredFields(type, typeDef);
          BuildInterfaceFields(type);
        }
      }
    }

    private static TypeInfo CreateType(TypeDef typeDef)
    {
      var type = new TypeInfo(BuildingContext.Current.Model, typeDef.Attributes) {
        UnderlyingType = typeDef.UnderlyingType, 
        Name = typeDef.Name, 
        MappingName = typeDef.MappingName
      };
      BuildingContext.Current.Model.Types.Add(type);
      return type;
    }

    private static void BuildAnsector(TypeDef type)
    {
      var ancestorDef = BuildingContext.Current.Definition.Types.FindAncestor(type);
      if (ancestorDef!=null)
        BuildType(ancestorDef);
    }

    private static void BuildInterfaces(TypeInfo type)
    {
      foreach (var @interfaceDef in BuildingContext.Current.Definition.Types.FindInterfaces(type.UnderlyingType)) {
        var @interface = BuildInterface(@interfaceDef, type);
        if (@interface!=null)
          BuildingContext.Current.Model.Types.RegisterImplementor(@interface, type);
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void BuildInterfaceFields(TypeInfo implementor)
    {
      foreach (var @interface in implementor.GetInterfaces(false)) {
        foreach (var ancestor in @interface.GetInterfaces(false))
          ProcessBaseInterface(ancestor, @interface);

        // Building other declared & inherited interface fields
        foreach (var fieldDef in BuildingContext.Current.Definition.Types[@interface.UnderlyingType].Fields) {
          if (@interface.Fields.Contains(fieldDef.Name))
            continue;
          string explicitName = BuildingContext.Current.NameBuilder.BuildExplicit(@interface, fieldDef.Name);
          FieldInfo implField;
          if (!implementor.Fields.TryGetValue(explicitName, out implField)) {
            if (!implementor.Fields.TryGetValue(fieldDef.Name, out implField))
              throw new DomainBuilderException(string.Format(
                Strings.TypeXDoesNotImplementYZField, implementor.Name, @interface.Name, fieldDef.Name));
          }

          if (implField!=null)
            FieldBuilder.BuildInterfaceField(@interface, implField, fieldDef);
        }
      }

      foreach (var @interface in implementor.GetInterfaces(true))
        BuildFieldMap(@interface, implementor);
    }

    private static void BuildSystemFields(TypeInfo type)
    {
      if (type.GetAncestor()!=null)
        return;
      if (type.Fields.Contains(BuildingContext.Current.NameBuilder.TypeIdFieldName))
        return;

      var typeId = new FieldDef(typeof (int)) {Name = BuildingContext.Current.NameBuilder.TypeIdFieldName, IsTypeId = true};
      FieldBuilder.BuildDeclaredField(type, typeId);
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void BuildDeclaredFields(TypeInfo type, TypeDef srcType)
    {
      foreach (var srcField in srcType.Fields)
        try {
          FieldInfo field;
          if (type.Fields.TryGetValue(srcField.Name, out field)) {
            if (field.ValueType!=srcField.ValueType)
              throw new DomainBuilderException(
                string.Format(Strings.ExFieldXIsAlreadyDefinedInTypeXOrItsAncestor, srcField.Name, type.Name));
          }
          else
            FieldBuilder.BuildDeclaredField(type, srcField);
        }
        catch (DomainBuilderException e) {
          BuildingContext.Current.RegisterError(e);
        }
    }

    private static void ProcessAncestor(TypeInfo type)
    {
      var ancestor = BuildingContext.Current.Model.Types.FindAncestor(type);
      if (ancestor==null)
        return;

      foreach (var srcField in ancestor.Fields.Find(FieldAttributes.Explicit, MatchType.None).Where(f => f.Parent==null)) {
        if (type.Fields.Contains(srcField.Name))
          continue;

        FieldBuilder.BuildInheritedField(type, srcField);
      }
      foreach (var pair in ancestor.FieldMap)
        if (!pair.Value.IsExplicit)
          type.FieldMap.Add(pair.Key, type.Fields[pair.Value.Name]);
    }

    private static void ProcessBaseInterface(TypeInfo ancestor, TypeInfo @interface)
    {
      foreach (var ancsField in ancestor.Fields.Find(FieldAttributes.Declared).Where(f => f.Parent==null)) {
        if (@interface.Fields.Contains(ancsField.Name))
          continue;

        FieldBuilder.BuildInheritedField(@interface, ancsField);
      }
    }

    private static void BuildHierarchyRoot(TypeInfo type, TypeDef typeDef, HierarchyDef hierarchy)
    {
      foreach (var keyField in hierarchy.KeyFields.Keys)
        BuildKeyField(typeDef, keyField, type);

      type.Hierarchy = HierarchyBuilder.BuildHierarchy(type, hierarchy);

      var indexDef = new IndexDef {IsPrimary = true};
      indexDef.Name = BuildingContext.Current.NameBuilder.Build(typeDef, indexDef);
      if (typeDef.Indexes.Contains(indexDef.Name))
        return;

      foreach (KeyValuePair<KeyField, Direction> pair in hierarchy.KeyFields)
        indexDef.KeyFields.Add(pair.Key.Name, pair.Value);

      typeDef.Indexes.Add(indexDef);
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void BuildKeyField(TypeDef typeDef, KeyField keyField, TypeInfo type)
    {
      var srcFieldDef = typeDef.Fields[keyField.Name];
      FieldBuilder.BuildDeclaredField(type, srcFieldDef);
      var field = type.Fields[srcFieldDef.Name];
      field.IsPrimaryKey = true;

      if (field.IsLazyLoad)
        throw new DomainBuilderException(string.Format(Strings.ExFieldXCanTBeLoadOnDemandAsItIsIncludedInPrimaryKey, field.Name));
      if (field.IsNullable && !field.ValueType.IsClass)
        throw new DomainBuilderException(string.Format(Strings.ExFieldXCanTBeNullableAsItIsIncludedInPrimaryKey, field.Name));
    }

    private static TypeInfo BuildInterface(TypeDef typeDef, TypeInfo implementor)
    {
      var context = BuildingContext.Current;

      TypeInfo type;
      context.Model.Types.TryGetValue(typeDef.UnderlyingType, out type);

      // TODO: Check interface key structure
//      if (type.Hierarchy!=implementor.Hierarchy) 
//          throw new DomainBuilderException(
//            string.Format(Strings.InterfaceXDoesNotBelongToXHierarchy, type.Name, implementor.Hierarchy.Root.Name));

      if (context.SkippedTypes.Contains(typeDef.UnderlyingType))
        return null;

      if (type!=null)
        return type;

      type = CreateType(typeDef);
      type.Hierarchy = implementor.Hierarchy;

      foreach (var @interface in context.Definition.Types.FindInterfaces(typeDef.UnderlyingType)) {
        var ancestor = BuildInterface(@interface, implementor);
        if (ancestor!=null)
          context.Model.Types.RegisterBaseInterface(ancestor, type);
      }

      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.FullName)) {
        // Building key & system fields according to implementor
        foreach (FieldInfo implField in implementor.Fields.Find(FieldAttributes.PrimaryKey | FieldAttributes.System)) {
          if (!type.Fields.Contains(implField.Name))
            FieldBuilder.BuildInterfaceField(type, implField, null);
        }
      }
      return type;
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void BuildFieldMap(TypeInfo @interface, TypeInfo implementor)
    {
      foreach (var field in @interface.Fields) {
        string explicitName = BuildingContext.Current.NameBuilder.BuildExplicit(field.DeclaringType, field.Name);
        FieldInfo implField;
        if (implementor.Fields.TryGetValue(explicitName, out implField))
          implField.IsExplicit = true;
        else {
          if (!implementor.Fields.TryGetValue(field.Name, out implField))
            throw new DomainBuilderException(
              string.Format(Resources.Strings.TypeXDoesNotImplementYZField, implementor.Name, @interface.Name, field.Name));
        }

        implField.IsInterfaceImplementation = true;

        if (!implementor.FieldMap.ContainsKey(field))
          implementor.FieldMap.Add(field, implField);
      }
    }

    private static void ProcessSkippedAncestors(TypeDef type)
    {
      var ancestor = BuildingContext.Current.Definition.Types.FindAncestor(type);
      while (ancestor!=null) {
        foreach (var field in ancestor.Fields) {
          if (field.UnderlyingProperty.DeclaringType.Assembly==Assembly.GetExecutingAssembly())
            field.IsSystem = true;
          type.Fields.Add(field);
        }
        ancestor = BuildingContext.Current.Definition.Types.FindAncestor(ancestor);
      }
    }

    private static void AssignHierarchy(TypeInfo type)
    {
      type.Hierarchy = type.GetRoot().Hierarchy;
    }
  }
}