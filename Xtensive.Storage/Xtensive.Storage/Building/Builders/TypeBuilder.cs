// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using Xtensive.Core;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;
using FieldAttributes=Xtensive.Storage.Model.FieldAttributes;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;
using Xtensive.Storage.Resources;
using System.Linq;

namespace Xtensive.Storage.Building.Builders
{
  internal static class TypeBuilder
  {
    /// <exception cref="DomainBuilderException">Type is not registered.</exception>
    public static void BuildType(Type type)
    {
      BuildType(BuildingContext.Current.ModelDef.Types.TryGetValue(type));
    }

    public static void BuildType(TypeDef typeDef)
    {
      if (BuildingContext.Current.Model.Types.Contains(typeDef.UnderlyingType))
        return;

      if (typeDef.IsEntity)
        BuildEntity(typeDef);

      if (typeDef.IsStructure)
        BuildStructure(typeDef);
    }

    private static void BuildStructure(TypeDef typeDef)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetFullName())) {
        CreateType(typeDef);
      }
    }

    private static void BuildEntity(TypeDef typeDef)
    {
      var hierarchyDef = BuildingContext.Current.ModelDef.FindHierarchy(typeDef);

      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetShortName())) {
        var typeInfo = CreateType(typeDef);

        if (typeInfo.UnderlyingType==hierarchyDef.Root.UnderlyingType)
          BuildHierarchyRoot(hierarchyDef, typeDef, typeInfo);
        else
          AssignHierarchy(typeInfo);
        BuildInterfaces(typeInfo);
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

    private static void BuildInterfaces(TypeInfo type)
    {
      foreach (var @interfaceDef in BuildingContext.Current.ModelDef.Types.FindInterfaces(type.UnderlyingType)) {
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
        foreach (var fieldDef in BuildingContext.Current.ModelDef.Types[@interface.UnderlyingType].Fields) {
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

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void BuildDeclaredFields(TypeInfo type, TypeDef srcType)
    {
      foreach (var srcField in srcType.Fields) {
        FieldInfo field;
        if (type.Fields.TryGetValue(srcField.Name, out field)) {
          if (field.ValueType!=srcField.ValueType)
            throw new DomainBuilderException(
              string.Format(Strings.ExFieldXIsAlreadyDefinedInTypeXOrItsAncestor, srcField.Name, type.Name));
        }
        else
          FieldBuilder.BuildDeclaredField(type, srcField);
      }
    }

    private static void ProcessAncestor(TypeInfo type)
    {
      var ancestor = BuildingContext.Current.Model.Types.FindAncestor(type);
      if (ancestor==null)
        return;

      foreach (var srcField in ancestor.Fields.Find(FieldAttributes.Explicit, MatchType.None).Where(f => f.Parent==null)) {
        if (!type.Fields.Contains(srcField.Name))
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

    private static void BuildHierarchyRoot(HierarchyDef hierarchy, TypeDef typeDef, TypeInfo typeInfo)
    {
      foreach (var keyField in hierarchy.KeyFields) {
        var fieldInfo = FieldBuilder.BuildDeclaredField(typeInfo, typeDef.Fields[keyField.Name]);
        fieldInfo.IsPrimaryKey = true;
      }

      if (!typeInfo.Fields.Contains(WellKnown.TypeIdField))
        FieldBuilder.BuildDeclaredField(typeInfo, typeDef.Fields[WellKnown.TypeIdField]);

      typeInfo.Hierarchy = HierarchyBuilder.BuildHierarchy(typeInfo, hierarchy);
    }

    private static TypeInfo BuildInterface(TypeDef typeDef, TypeInfo implementor)
    {
      var context = BuildingContext.Current;

      TypeInfo type;
      context.Model.Types.TryGetValue(typeDef.UnderlyingType, out type);

      if (type!=null)
        return type;

      type = CreateType(typeDef);
      type.Hierarchy = implementor.Hierarchy;

      foreach (var @interface in context.ModelDef.Types.FindInterfaces(typeDef.UnderlyingType)) {
        var ancestor = BuildInterface(@interface, implementor);
        if (ancestor!=null)
          context.Model.Types.RegisterBaseInterface(ancestor, type);
      }

      using (Log.InfoRegion(Strings.LogBuildingX, typeDef.UnderlyingType.GetFullName())) {
        // Building key & system fields according to implementor
        foreach (FieldInfo implField in implementor.Fields.Find(FieldAttributes.PrimaryKey | FieldAttributes.TypeId)) {
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

    private static void AssignHierarchy(TypeInfo typeInfo)
    {
      var root = typeInfo.GetRoot();
      typeInfo.Hierarchy = root.Hierarchy;
      foreach (var fieldInfo in root.Fields)
        FieldBuilder.BuildInheritedField(typeInfo, fieldInfo);
    }

    public static void BuildFields(TypeDef typeDef)
    {
      TypeInfo typeInfo = BuildingContext.Current.Model.Types[typeDef.UnderlyingType];

      ProcessAncestor(typeInfo);
      BuildDeclaredFields(typeInfo, typeDef);
      if (typeInfo.IsEntity)
        BuildInterfaceFields(typeInfo);
    }
  }
}