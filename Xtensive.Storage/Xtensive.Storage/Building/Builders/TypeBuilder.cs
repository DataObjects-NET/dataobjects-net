// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;
using FieldAttributes=Xtensive.Storage.Model.FieldAttributes;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Building.Builders
{
  internal static class TypeBuilder
  {
    public static TypeDef DefineType(Type type)
    {
      BuildingContext context = BuildingScope.Context;

      using (Log.InfoRegion(String.Format("Defining type '{0}'", type.FullName))) {

        if (context.Definition.Types.Contains(type))
          throw new DomainBuilderException(
            string.Format(Resources.Strings.TypeXIsAlreadyDefined, type.FullName));

        TypeDef typeDef = new TypeDef(type);

        ProcessEntityAtribute(type, typeDef);
        ProcessMaterializedViewAttribute(type, typeDef);

        typeDef.Name = context.NameProvider.BuildName(typeDef);

        if (context.Definition.Types.Contains(typeDef.Name))
          throw new DomainBuilderException(
            string.Format(Resources.Strings.TypeWithNameXIsAlreadyDefined, typeDef.Name));

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
            string.Format(Resources.Strings.FieldWithNameXIsAlreadyRegistered, fieldDef.Name));

        typeDef.Fields.Add(fieldDef);
      }
    }

    private static void ProcessMaterializedViewAttribute(Type type, TypeDef typeDef)
    {
      var materializedViewAttribute =type.GetAttribute<MaterializedViewAttribute>(false);
      if (materializedViewAttribute != null)
        AttributeProcessor.Process(typeDef, materializedViewAttribute);
    }

    private static void ProcessEntityAtribute(Type type, TypeDef typeDef)
    {
      var entityAttribute = type.GetAttribute<EntityAttribute>(true);
      if (entityAttribute != null)
        AttributeProcessor.Process(typeDef, entityAttribute);
    }

    public static void BuildType(TypeDef typeDef)
    {
      BuildingContext buildingContext = BuildingScope.Context;

      if (typeDef.IsInterface)
        return;

      if (buildingContext.Model.Types.Contains(typeDef.UnderlyingType))
        return;

      if (buildingContext.SkippedTypes.Contains(typeDef.UnderlyingType))
        return;

      if (typeDef.IsStructure)
        BuildStructure(typeDef);
      else if (typeDef.IsEntity)
        BuildEntity(typeDef);
    }

    private static void BuildStructure(TypeDef typeDef)
    {
      BuildAnsector(typeDef);

      using (Log.InfoRegion(String.Format("Building structure '{0}'", typeDef.UnderlyingType.FullName))) {
        TypeInfo type = CreateType(typeDef);
        ProcessAncestor(type);
        BuildDeclaredFields(type, typeDef);
      }
    }

    private static void BuildEntity(TypeDef typeDef)
    {
      HierarchyDef hierarchy = BuildingScope.Context.Definition.FindHierarchy(typeDef);
      if (hierarchy==null) {
        Log.Info("Skipping entity '{0}' as it does not belong to any hierarchy thus it cannot be persistent.",
          typeDef.UnderlyingType);
        BuildingScope.Context.SkippedTypes.Add(typeDef.UnderlyingType);
        return;
      }

      BuildAnsector(typeDef);

      using (Log.InfoRegion(String.Format("Building entity '{0}'", typeDef.UnderlyingType.GetShortName()))) {

        TypeInfo type = CreateType(typeDef);

        if (type.UnderlyingType==hierarchy.Root.UnderlyingType) {
          ProcessSkippedAncestors(typeDef);
          BuildHierarchyRoot(type, typeDef, hierarchy);
          BuildSystemFields(type);
          BuildDeclaredFields(type, typeDef);
          BuildInterfaces(type);
        }
        else {
          ProcessAncestor(type);
          BuildDeclaredFields(type, typeDef);
          AssignHierarchy(type);
          BuildInterfaces(type);
        }
      }
    }

    private static TypeInfo CreateType(TypeDef typeDef)
    {
      TypeInfo type = new TypeInfo(BuildingScope.Context.Model, typeDef.Attributes);
      type.UnderlyingType = typeDef.UnderlyingType;
      type.Name = typeDef.Name;
      type.MappingName = typeDef.MappingName;
      BuildingScope.Context.Model.Types.Add(type);
      return type;
    }

    private static void BuildAnsector(TypeDef type)
    {
      TypeDef ancestor = BuildingScope.Context.Definition.Types.FindAncestor(type);
      if (ancestor != null)
        BuildType(ancestor);
    }

    private static void BuildInterfaces(TypeInfo type)
    {
      foreach (TypeDef @interfaceDef in BuildingScope.Context.Definition.Types.FindInterfaces(type.UnderlyingType)) {
        TypeInfo @interface = BuildInterface(@interfaceDef, type);
        if (@interface != null)
          BuildingScope.Context.Model.Types.RegisterImplementor(@interface, type);
      }
      foreach (TypeInfo @interface in type.GetInterfaces(true))
        BuildFieldMap(@interface, type);
    }

    private static void BuildSystemFields(TypeInfo type)
    {
      if (type.GetAncestor() != null)
        return;
      var typeId = new FieldDef(typeof(int)) {Name = BuildingScope.Context.NameProvider.TypeId, IsSystem = true};
      
      FieldInfo field = FieldBuilder.BuildDeclaredField(type, typeId);
      type.Fields.Add(field);     
    }

    private static void BuildDeclaredFields(TypeInfo type, TypeDef srcType)
    {
      foreach (FieldDef srcField in srcType.Fields)
        try {
          FieldInfo field;
          if (type.Fields.TryGetValue(srcField.Name, out field)) {
            if (type.Fields[srcField.Name].ValueType!=srcField.ValueType)
              throw new DomainBuilderException(
                string.Format(Resources.Strings.FieldXIsAlreadyDefinedInTypeXOrItsAncestor, srcField.Name, type.Name));
          }
          else {
            field = FieldBuilder.BuildDeclaredField(type, srcField);
            type.Fields.Add(field);
          }
        }
        catch(DomainBuilderException e) {
          BuildingContext.Current.RegistError(e);
        }
    }


    private static void ProcessAncestor(TypeInfo type)
    {
      TypeInfo ancestor = BuildingScope.Context.Model.Types.FindAncestor(type);
      if (ancestor==null)
        return;

      foreach (FieldInfo srcField in ancestor.Fields.Find(FieldAttributes.Explicit, MatchType.None)) {
        FieldInfo field;
        if (type.Fields.TryGetValue(srcField.Name, out field))
          continue;
        
        field = FieldBuilder.BuildInheritedField(type, srcField);      
        type.Fields.Add(field);        
      }
      foreach (KeyValuePair<FieldInfo, FieldInfo> pair in ancestor.FieldMap)
        if (!pair.Value.IsExplicit)
          type.FieldMap.Add(pair.Key, type.Fields[pair.Value.Name]);
    }

    private static void ProcessBaseInterface(TypeInfo ancestor, TypeInfo @interface)
    {
      foreach (FieldInfo ancsField in ancestor.Fields.Find(FieldAttributes.Declared)) {
        FieldInfo field;
        if (@interface.Fields.TryGetValue(ancsField.Name, out field))
          continue;
                
        field = FieldBuilder.BuildInheritedField(@interface, ancsField);
        @interface.Fields.Add(field);                  
      }
    }

    private static void BuildHierarchyRoot(TypeInfo type, TypeDef typeDef, HierarchyDef hierarchy)
    {      
      foreach (KeyField keyField in hierarchy.KeyFields.Keys)
        BuildHierarchyKeyField( typeDef, keyField, type);
                 
      type.Hierarchy = HierarchyBuilder.BuildHierarchy(type, hierarchy);

      IndexDef index = new IndexDef {IsPrimary = true};
      index.Name = BuildingScope.Context.NameProvider.BuildName(typeDef, index);
      if (typeDef.Indexes.Contains(index.Name))
        return;

      foreach (KeyValuePair<KeyField, Direction> pair in hierarchy.KeyFields)
        index.KeyFields.Add(pair.Key.Name, pair.Value);

      typeDef.Indexes.Add(index);
    }

    private static void BuildHierarchyKeyField(TypeDef typeDef, KeyField keyField, TypeInfo typeInfo)
    {
      FieldDef srcField;

      if (!typeDef.Fields.TryGetValue(keyField.Name, out srcField))
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ExKeyFieldXWasNotFoundInTypeY, keyField.Name, typeDef.Name));

      if (srcField.ValueType!=keyField.ValueType)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ValueTypeMismatchForFieldX, keyField.Name));

      FieldInfo field = FieldBuilder.BuildDeclaredField(typeInfo, srcField);      

      field.IsPrimaryKey = true;
      typeInfo.Fields.Add(field);
    }

    private static TypeInfo BuildInterface(TypeDef typeDef, TypeInfo implementor)
    {
      BuildingContext buildingContext = BuildingScope.Context;

      // EnsureBelongsToHierarchy
      TypeInfo type;
      if (buildingContext.Model.Types.TryGetValue(typeDef.UnderlyingType, out type))
        if (type.Hierarchy!=implementor.Hierarchy) 
          throw new DomainBuilderException(
            string.Format(Resources.Strings.InterfaceXDoesNotBelongToXHierarchy, type.Name, implementor.Hierarchy.Root.Name));

      if (buildingContext.SkippedTypes.Contains(typeDef.UnderlyingType))
        return null;

      if (type!=null)
        return type;

      type = CreateType(typeDef);
      type.Hierarchy = implementor.Hierarchy;

      foreach (TypeDef @interface in buildingContext.Definition.Types.FindInterfaces(typeDef.UnderlyingType)) {
        TypeInfo ancestor = BuildInterface(@interface, implementor);
        if (ancestor != null)
          buildingContext.Model.Types.RegisterBaseInterface(ancestor, type);
      }

      using (Log.InfoRegion(String.Format("Building interface '{0}'", typeDef.UnderlyingType.FullName))) {

        foreach (TypeInfo @interface in buildingContext.Model.Types.FindInterfaces(type, false))
          ProcessBaseInterface(@interface, type);

        // Building key & system fields according to implementor
        foreach (FieldInfo implField in implementor.Fields.Find(FieldAttributes.PrimaryKey | FieldAttributes.System)) {
          FieldInfo field;
          if (!type.Fields.TryGetValue(implField.Name, out field)) {
            field = FieldBuilder.BuildInterfaceField(type, implField, null);
            type.Fields.Add(field);
          }
        }

        // Building other declared & inherited interface fields
        foreach (FieldDef fieldDef in typeDef.Fields) {
          FieldInfo implField;
          string explicitName = buildingContext.NameProvider.BuildExplicitName(type, fieldDef.Name);
          if (!implementor.Fields.TryGetValue(explicitName, out implField))
            if (!implementor.Fields.TryGetValue(fieldDef.Name, out implField))
              throw new DomainBuilderException(
                string.Format(Resources.Strings.TypeXDoesNotImplementYZField, implementor.Name, type.Name, fieldDef.Name));
              
          if (implField != null) {
            FieldInfo field = FieldBuilder.BuildInterfaceField(type, implField, fieldDef);
            type.Fields.Add(field);
          }
        }
      }
      return type;
    }

    private static void BuildFieldMap(TypeInfo @interface, TypeInfo implementor)
    {
      foreach (FieldInfo field in @interface.Fields) {
        FieldInfo implField;
        string explicitName = BuildingScope.Context.NameProvider.BuildExplicitName(field.DeclaringType, field.Name);

        if (implementor.Fields.TryGetValue(explicitName, out implField)) 
          implField.IsExplicit = true;
        else
          if (!implementor.Fields.TryGetValue(field.Name, out implField))
            throw new DomainBuilderException(
              string.Format(Resources.Strings.TypeXDoesNotImplementYZField, implementor.Name, @interface.Name, field.Name));                    
        
        implField.IsInterfaceImplementation = true;

        if (!implementor.FieldMap.ContainsKey(field))
          implementor.FieldMap.Add(field, implField);
      }
    }

    private static void ProcessSkippedAncestors(TypeDef type)
    {
      TypeDef ancestor = BuildingScope.Context.Definition.Types.FindAncestor(type);
      while (ancestor != null) {
        foreach (FieldDef field in ancestor.Fields) {
          if (field.UnderlyingProperty.DeclaringType.Assembly==Assembly.GetExecutingAssembly())
            field.IsSystem = true;
          type.Fields.Add(field);
        }
        ancestor = BuildingScope.Context.Definition.Types.FindAncestor(ancestor);
      }
    }

    private static void AssignHierarchy(TypeInfo type)
    {
      type.Hierarchy = type.GetRoot().Hierarchy;
    }
  }
}
