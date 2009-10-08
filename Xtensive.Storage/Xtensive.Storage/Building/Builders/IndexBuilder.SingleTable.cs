// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Model;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Building.Builders
{
  internal partial class IndexBuilder
  {
    private static void BuildSingleTableIndexes(TypeInfo type)
    {
      if (type.Indexes.Count > 0)
        return;
      if (type.IsStructure)
        return;

      var context = BuildingContext.Current;
      var typeDef = context.ModelDef.Types[type.UnderlyingType];
      var root = type.Hierarchy.Root;

      // Building declared indexes both secondary and primary (for root of the hierarchy only)
      foreach (var indexDescriptor in typeDef.Indexes) {
        var declaredIndex = BuildIndex(type, indexDescriptor, false);
        root.Indexes.Add(declaredIndex);
        context.Model.RealIndexes.Add(declaredIndex);
      }

      var parent = type.GetAncestor();
      // Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        if ((parent == null) || !parent.GetInterfaces(true).Contains(@interface))
          foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
            if (parentIndex.DeclaringIndex != parentIndex) 
              continue;
            var index = BuildInheritedIndex(type, parentIndex, false);
            if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
              continue;
            root.Indexes.Add(index);
            context.Model.RealIndexes.Add(index);
          }
      }

      // Build indexes for descendants
      foreach (var descendant in type.GetDescendants())
        BuildSingleTableIndexes(descendant);

      if (type == root) return;

      var types = type.GetAncestors().AddOne(type).ToHashSet();
      var filterByTypes = type.GetDescendants(true).AddOne(type).ToList();
      var interfaces = type.GetInterfaces(true).ToHashSet();
      
      // Import inherited indexes
      foreach (var ancestorIndex in root.Indexes) {
        if (ancestorIndex.DeclaringType.IsInterface) {
          if (!interfaces.Contains(ancestorIndex.DeclaringType))
            continue;
          if (!types.Contains(ancestorIndex.ReflectedType))
            continue;
          var filterIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
          var indexView = BuildViewIndex(type, filterIndex);
          type.Indexes.Add(indexView);
        }
        else if (types.Contains(ancestorIndex.ReflectedType)) {
          var filterIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
          var indexView = BuildViewIndex(type, filterIndex);
          type.Indexes.Add(indexView);
        }
      }
    }
  }
}