// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System.Collections.Generic;
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
      foreach (var @interface in type.GetInterfaces()) {
        foreach (var interfaceIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          if (root.Indexes.Any(i => i.DeclaringIndex == interfaceIndex.DeclaringIndex && i.ReflectedType == type))
            continue;
          var index = BuildInheritedIndex(type, interfaceIndex, false);
          root.Indexes.Add(index);
          context.Model.RealIndexes.Add(index);
        }
      }

      // Build indexes for descendants
      foreach (var descendant in type.GetDescendants())
        BuildSingleTableIndexes(descendant);

      if (type == root) return;

      var types = type.GetAncestors().AddOne(type).ToHashSet();
      var descendants = type.GetDescendants(true).ToList();
      var primaryIndexFilterTypes = new List<TypeInfo>();
      if (!type.IsAbstract)
        primaryIndexFilterTypes.Add(type);
      primaryIndexFilterTypes.AddRange(descendants);
      
      // Import inherited indexes
      var ancestorIndexes = root.Indexes
        .Where(i => types.Contains(i.ReflectedType))
        .Reverse()
        .ToList();
      foreach (var ancestorIndex in ancestorIndexes) {
        if (type.Indexes.Any(i => 
            i.DeclaringIndex == ancestorIndex.DeclaringIndex &&
            i.ReflectedType == type && 
            i.IsVirtual))
          continue;
        if (ancestorIndex.DeclaringType.IsInterface) {
          var filteredDescendants = descendants
            .Where(t => !t.IsAbstract && !t.GetInterfaces().Contains(ancestorIndex.DeclaringType));
          var filterByTypes = new List<TypeInfo>();
          if (!type.IsAbstract)
            filterByTypes.Add(type);
          filterByTypes.AddRange(filterByTypes);
          var filterIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
          var indexView = BuildViewIndex(type, filterIndex);
          type.Indexes.Add(indexView);
        }
        else {
          if (ancestorIndex.IsPrimary) {
            var filterIndex = BuildFilterIndex(type, ancestorIndex, primaryIndexFilterTypes);
            var indexView = BuildViewIndex(type, filterIndex);
            type.Indexes.Add(indexView);
          }
          else {
            var filterIndex = BuildFilterIndex(type, ancestorIndex, primaryIndexFilterTypes);
            type.Indexes.Add(filterIndex);
          }
        }
      }

//      foreach (var ancestorIndex in root.Indexes) {
//        if (ancestorIndex.DeclaringType.IsInterface) {
//          if (!interfaces.Contains(ancestorIndex.DeclaringType))
//            continue;
//          if (!types.Contains(ancestorIndex.ReflectedType))
//            continue;
//          var filterIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
//          var indexView = BuildViewIndex(type, filterIndex);
//          type.Indexes.Add(indexView);
//        }
//        else if (types.Contains(ancestorIndex.ReflectedType)) {
//          if (ancestorIndex.IsPrimary) {
//            var filterIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
//            var indexView = BuildViewIndex(type, filterIndex);
//            type.Indexes.Add(indexView);
//          }
//          else {
//            var filterIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
//            type.Indexes.Add(filterIndex);
//          }
//        }
//      }
    }
  }
}