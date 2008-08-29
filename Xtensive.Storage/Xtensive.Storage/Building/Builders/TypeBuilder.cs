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
using Xtensive.Storage.Providers;
using FieldAttributes=Xtensive.Storage.Model.FieldAttributes;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class TypeBuilder
  {
    public static TypeDef DefineType(Type type)
    {
      BuildingContext context = BuildingContext.Current;
      using (Log.InfoRegion(Strings.LogDefiningX, type.FullName)) {

        if (context.Definition.Types.Contains(type))
          throw new DomainBuilderException(
            string.Format(Strings.TypeXIsAlreadyDefined, type.FullName));

        TypeDef typeDef = new TypeDef(type);

        ProcessEntityAtribute(type, typeDef);
        ProcessMaterializedViewAttribute(type, typeDef);

        typeDef.Name = context.NameBuilder.Build(typeDef);

        if (context.Definition.Types.Contains(typeDef.Name))
          throw new DomainBuilderException(
            string.Format(Strings.TypeWithNameXIsAlreadyDefined, typeDef.Name));

        DefineFields(typeDef);

        return typeDef;
      }
    }

    private static void DefineFields(TypeDef typeDef)
    {
      var fields = FieldBuilder.DefineFields(typeDef);

      foreach (FieldDef fieldDef in fields) {
        if (typeDef.Fields.Contains(fieldDef.Name))
          throw new DomainBuilderException(
            string.Format(Strings.FieldWithNameXIsAlreadyRegistered, fieldDef.Name));

        typeDef.Fields.Add(fieldDef);
      }
    }

    private static void ProcessMaterializedViewAttribute(Type type, TypeDef typeDef)
    {
      var materializedViewAttribute =type.GetAttribute<MaterializedViewAttribute>(
        AttributeSearchOptions.Default);
      if (materializedViewAttribute != null)
        AttributeProcessor.Process(typeDef, materializedViewAttribute);
    }

    private static void ProcessEntityAtribute(Type type, TypeDef typeDef)
    {
      var entityAttribute = type.GetAttribute<EntityAttribute>(
        AttributeSearchOptions.InheritAll);
      if (entityAttribute != null)
        AttributeProcessor.Process(typeDef, entityAttribute);
    }

    public static void BuildType(Type type)
    {
      TypeDef typeDef = BuildingContext.Current.Definition.Types.TryGetValue(type);

      if (typeDef == null)
        throw new DomainBuilderException(
          string.Format(Strings.ExTypeXIsNotRegisteredInTheModel, type.FullName));

      BuildType(typeDef);
    }

    public static void BuildType(TypeDef typeDef)
    {
      BuildingContext context = BuildingContext.Current;

      if (typeDef.IsInterface)
        return;

      if (context.SkippedTypes.Contains(typeDef.UnderlyingType))
        return;

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
        TypeInfo type = CreateType(typeDef);
        ProcessAncestor(type);
        BuildDeclaredFields(type, typeDef);
      }
    }

    private static void BuildEntity(TypeDef typeDef)
    {
      HierarchyDef hierarchy = BuildingContext.Current.Definition.FindHierarchy(typeDef);
      if (hierarchy==null) {
        Log.Info("Skipping entity '{0}' as it does not belong to any hierarchy thus it cannot be persistent.",
          typeDef.UnderlyingType);
        BuildingContext.Current.SkippedTypes.Add(typeDef.UnderlyingType);
        return;
      }

      BuildAnsector(typeDef);

      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetShortName())) {

        TypeInfo type = CreateType(typeDef);

        if (type.UnderlyingType==hierarchy.Root.UnderlyingType) {
          ProcessSkippedAncestors(typeDef);
          BuildHierarchyRoot(type, typeDef, hierarchy);
          BuildSystemFields(type);
          BuildDeclaredFields(type, typeDef);
          BuildInterfaces(type);
        }
        else {
          AssignHierarchy(type);
          ProcessAncestor(type);
          BuildDeclaredFields(type, typeDef);
          BuildInterfaces(type);
        }
      }
    }

    private static TypeInfo CreateType(TypeDef typeDef)
    {
      TypeInfo type = new TypeInfo(BuildingContext.Current.Model, typeDef.Attributes);
      type.UnderlyingType = typeDef.UnderlyingType;
      type.Name = typeDef.Name;
      type.MappingName = typeDef.MappingName;
      BuildingContext.Current.Model.Types.Add(type);
      return type;
    }

    private static void BuildAnsector(TypeDef type)
    {
      TypeDef ancestor = BuildingContext.Current.Definition.Types.FindAncestor(type);
      if (ancestor != null)
        BuildType(ancestor);
    }

    private static void BuildInterfaces(TypeInfo type)
    {
      foreach (TypeDef @interfaceDef in BuildingContext.Current.Definition.Types.FindInterfaces(type.UnderlyingType)) {
        TypeInfo @interface = BuildInterface(@interfaceDef, type);
        if (@interface != null)
          BuildingContext.Current.Model.Types.RegisterImplementor(@interface, type);
      }
      foreach (TypeInfo @interface in type.GetInterfaces(true))
        BuildFieldMap(@interface, type);
    }

    private static void BuildSystemFields(TypeInfo type)
    {
      if (type.GetAncestor() != null)
        return;
      if (type.Fields.TryGetValue(NameBuilder.TypeIdFieldName) != null)
        return;

      var typeId = new FieldDef(typeof(int)) {Name = NameBuilder.TypeIdFieldName, IsSystem = true};
      FieldBuilder.BuildDeclaredField(type, typeId);
    }

    private static void BuildDeclaredFields(TypeInfo type, TypeDef srcType)
    {
      foreach (FieldDef srcField in srcType.Fields)
        try {
          FieldInfo field = type.Fields.TryGetValue(srcField.Name);
          if (field != null) {
            if (type.Fields[srcField.Name].ValueType!=srcField.ValueType)
              throw new DomainBuilderException(
                string.Format(Resources.Strings.FieldXIsAlreadyDefinedInTypeXOrItsAncestor, srcField.Name, type.Name));
          }
          else
            FieldBuilder.BuildDeclaredField(type, srcField);
        }
        catch(DomainBuilderException e) {
          BuildingContext.Current.RegisterError(e);
        }
    }


    private static void ProcessAncestor(TypeInfo type)
    {
      TypeInfo ancestor = BuildingContext.Current.Model.Types.FindAncestor(type);
      if (ancestor==null)
        return;

      foreach (FieldInfo srcField in ancestor.Fields.Find(FieldAttributes.Explicit, MatchType.None)) {
        FieldInfo field = type.Fields.TryGetValue(srcField.Name);
        if (field != null)
          continue;

        FieldBuilder.BuildInheritedField(type, srcField);
      }
      foreach (KeyValuePair<FieldInfo, FieldInfo> pair in ancestor.FieldMap)
        if (!pair.Value.IsExplicit)
          type.FieldMap.Add(pair.Key, type.Fields[pair.Value.Name]);
    }

    private static void ProcessBaseInterface(TypeInfo ancestor, TypeInfo @interface)
    {
      foreach (FieldInfo ancsField in ancestor.Fields.Find(FieldAttributes.Declared)) {
        FieldInfo field = @interface.Fields.TryGetValue(ancsField.Name);
        if (field != null)
          continue;

        FieldBuilder.BuildInheritedField(@interface, ancsField);
      }
    }

    private static void BuildHierarchyRoot(TypeInfo type, TypeDef typeDef, HierarchyDef hierarchy)
    {
      foreach (KeyField keyField in hierarchy.KeyFields.Keys)
        BuildKeyField( typeDef, keyField, type);

      type.Hierarchy = HierarchyBuilder.BuildHierarchy(type, hierarchy);

      IndexDef index = new IndexDef {IsPrimary = true};
      index.Name = BuildingContext.Current.NameBuilder.Build(typeDef, index);
      if (typeDef.Indexes.Contains(index.Name))
        return;

      foreach (KeyValuePair<KeyField, Direction> pair in hierarchy.KeyFields)
        index.KeyFields.Add(pair.Key.Name, pair.Value);

      typeDef.Indexes.Add(index);
    }

    private static void BuildKeyField(TypeDef typeDef, KeyField keyField, TypeInfo type)
    {
      FieldDef srcField = typeDef.Fields.TryGetValue(keyField.Name);

      if (srcField == null)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ExKeyFieldXWasNotFoundInTypeY, keyField.Name, typeDef.Name));

      if (srcField.ValueType!=keyField.ValueType)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ValueTypeMismatchForFieldX, keyField.Name));

      FieldBuilder.BuildDeclaredField(type, srcField);
      FieldInfo field = type.Fields[srcField.Name];
      field.IsPrimaryKey = true;
    }

    private static TypeInfo BuildInterface(TypeDef typeDef, TypeInfo implementor)
    {
      BuildingContext context = BuildingContext.Current;

      // EnsureBelongsToHierarchy
      TypeInfo type = context.Model.Types.TryGetValue(typeDef.UnderlyingType);
      if (type != null)
        if (type.Hierarchy!=implementor.Hierarchy) 
          throw new DomainBuilderException(
            string.Format(Resources.Strings.InterfaceXDoesNotBelongToXHierarchy, type.Name, implementor.Hierarchy.Root.Name));

      if (context.SkippedTypes.Contains(typeDef.UnderlyingType))
        return null;

      if (type!=null)
        return type;

      type = CreateType(typeDef);
      type.Hierarchy = implementor.Hierarchy;

      foreach (TypeDef @interface in context.Definition.Types.FindInterfaces(typeDef.UnderlyingType)) {
        TypeInfo ancestor = BuildInterface(@interface, implementor);
        if (ancestor != null)
          context.Model.Types.RegisterBaseInterface(ancestor, type);
      }

      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.FullName)) {

        foreach (TypeInfo @interface in context.Model.Types.FindInterfaces(type, false))
          ProcessBaseInterface(@interface, type);

        // Building key & system fields according to implementor
        foreach (FieldInfo implField in implementor.Fields.Find(FieldAttributes.PrimaryKey | FieldAttributes.System)) {
          FieldInfo field = type.Fields.TryGetValue(implField.Name);
          if (field == null)
            FieldBuilder.BuildInterfaceField(type, implField, null);
        }

        // Building other declared & inherited interface fields
        foreach (FieldDef fieldDef in typeDef.Fields) {
          string explicitName = context.NameBuilder.BuildExplicit(type, fieldDef.Name);
          FieldInfo implField = implementor.Fields.TryGetValue(explicitName);
          if (implField == null) {
            implField = implementor.Fields.TryGetValue(fieldDef.Name);
            if (implField == null)
              throw new DomainBuilderException(
                string.Format(Resources.Strings.TypeXDoesNotImplementYZField, implementor.Name, type.Name, fieldDef.Name));}

          if (implField!=null)
            FieldBuilder.BuildInterfaceField(type, implField, fieldDef);
        }
      }
      return type;
    }

    private static void BuildFieldMap(TypeInfo @interface, TypeInfo implementor)
    {
      foreach (FieldInfo field in @interface.Fields) {
        string explicitName = BuildingContext.Current.NameBuilder.BuildExplicit(field.DeclaringType, field.Name);
        FieldInfo implField = implementor.Fields.TryGetValue(explicitName);

        if (implField != null) 
          implField.IsExplicit = true;
        else {
          implField = implementor.Fields.TryGetValue(field.Name);
          if (implField == null)
            throw new DomainBuilderException(
              string.Format(Resources.Strings.TypeXDoesNotImplementYZField, implementor.Name, @interface.Name, field.Name));}

        implField.IsInterfaceImplementation = true;

        if (!implementor.FieldMap.ContainsKey(field))
          implementor.FieldMap.Add(field, implField);
      }
    }

    private static void ProcessSkippedAncestors(TypeDef type)
    {
      TypeDef ancestor = BuildingContext.Current.Definition.Types.FindAncestor(type);
      while (ancestor != null) {
        foreach (FieldDef field in ancestor.Fields) {
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
