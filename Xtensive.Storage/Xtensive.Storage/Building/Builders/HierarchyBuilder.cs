// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Building.Builders
{
  internal static class HierarchyBuilder
  {
    public static HierarchyDef TryDefineHierarchy(TypeDef typeDef)
    {
      if (!typeDef.IsEntity)
        return null;
      Log.Info("Detecting hierarchy.");

      var hierarchyRootAttribute = typeDef.UnderlyingType.GetAttribute<HierarchyRootAttribute>(true);
      if (hierarchyRootAttribute==null)
        return null;

      TypeDef root = BuildingScope.Context.Definition.FindRoot(typeDef);
      if (root!=null)
        return null;

      HierarchyDef hierarchy = new HierarchyDef(typeDef);
      AttributeProcessor.Process(hierarchy, hierarchyRootAttribute);
      return hierarchy;
    }

    public static HierarchyDef DefineHierarchy(TypeDef typeDef, InheritanceSchema inheritanceSchema)
    {
      BuildingContext buildingContext = BuildingScope.Context;

      Log.Info("Defining hierarchy for type '{0}'", typeDef.UnderlyingType.FullName);

      TypeDef root = buildingContext.Definition.FindRoot(typeDef);
      if (root!=null) {
        Log.Error("TypeDef '{0}' is already belongs to hierarchy with the root = '{1}'.",
          typeDef.UnderlyingType.FullName,
          root.UnderlyingType.FullName);
        return null;
      }

      foreach (HierarchyDef hierarchy in buildingContext.Definition.Hierarchies)
        if (hierarchy.Root.UnderlyingType.IsSubclassOf(typeDef.UnderlyingType)) {
          Log.Error("'{0}' descendant is already a root of another hierarchy.",
            hierarchy.Root.UnderlyingType);
          return null;
        }

      HierarchyDef hierarchyDef = new HierarchyDef(typeDef);
      hierarchyDef.Schema = inheritanceSchema;

      return hierarchyDef;
    }

    public static HierarchyInfo BuildHierarchy(TypeInfo root, HierarchyDef hierarchyDef)
    {
      HierarchyInfo hierarchy = new HierarchyInfo(root, hierarchyDef.Schema, hierarchyDef.KeyProvider);

      foreach (KeyValuePair<KeyField, Direction> pair in hierarchyDef.KeyFields) {
        FieldInfo field;
        if (!root.Fields.TryGetValue(pair.Key.Name, out field))
          Log.Error(String.Format("Key field '{0}' was not found in type '{1}'.", pair.Key.Name, root.Name));
        else
          hierarchy.Fields.Add(field, pair.Value);
      }

      BuildingScope.Context.Model.Hierarchies.Add(hierarchy);
      return hierarchy;
    }

    public static void BuildHierarchyColumns(HierarchyInfo hierarchyInfo)
    {
      DirectionCollection<ColumnInfo> columnsCollection = hierarchyInfo.Root.Indexes.PrimaryIndex.KeyColumns;
      for (int i = 0; i < columnsCollection.Count; i++)
        hierarchyInfo.Columns.Add(columnsCollection[i].Key);
      hierarchyInfo.Columns.Lock();
    }
  }
}