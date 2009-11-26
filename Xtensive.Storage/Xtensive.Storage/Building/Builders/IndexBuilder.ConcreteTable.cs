// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Builders
{
  internal partial class IndexBuilder
  {
    private static void BuildConcreteTableIndexes(TypeInfo type)
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
        var declaredIndex = BuildIndex(type, indexDescriptor, type.UnderlyingType.IsAbstract); 

        type.Indexes.Add(declaredIndex);
        context.Model.RealIndexes.Add(declaredIndex);
      }

      // Building primary index for non root entities
      var parent = type.GetAncestor();
      if (parent != null) {
        var parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var inheritedIndex = BuildInheritedIndex(type, parentPrimaryIndex, type.UnderlyingType.IsAbstract);
       
        // Registering built primary index
        type.Indexes.Add(inheritedIndex);
        context.Model.RealIndexes.Add(inheritedIndex);
      }

      // Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          if (parentIndex.DeclaringIndex != parentIndex) 
            continue;
          var index = BuildInheritedIndex(type, parentIndex, type.UnderlyingType.IsAbstract);
          if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
            continue;
          type.Indexes.Add(index);
          context.Model.RealIndexes.Add(index);
        }
      }

      // Build typed indexes
      foreach (var realIndex in type.Indexes.Find(IndexAttributes.Real)) {
        var typedIndex = BuildTypedIndex(type, realIndex);
        type.Indexes.Add(typedIndex);
      }

      // Build indexes for descendants
      foreach (var descendant in type.GetDescendants())
        BuildConcreteTableIndexes(descendant);

      var ancestors = type.GetAncestors().ToList();
      var descendants = type.GetDescendants(true).ToList();

      // Build primary virtual union index
      if (descendants.Count > 0) {
        var indexesToUnion = new List<IndexInfo>(){type.Indexes.PrimaryIndex};
        foreach (var index in descendants.Select(t => t.Indexes.PrimaryIndex)) {
          var indexView = BuildViewIndex(type, index);
          indexesToUnion.Add(indexView);
        }
        var virtualPrimaryIndex = BuildUnionIndex(type, indexesToUnion);
        type.Indexes.Add(virtualPrimaryIndex);

      }

      // Build inherited secondary indexes
      foreach (var ancestorIndex in ancestors.SelectMany(ancestor => ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None))) {
        if (ancestorIndex.DeclaringIndex != ancestorIndex) 
          continue;
        var secondaryIndex = BuildInheritedIndex(type, ancestorIndex, type.UnderlyingType.IsAbstract);
        type.Indexes.Add(secondaryIndex);
        context.Model.RealIndexes.Add(secondaryIndex);
        // Build typed index for secondary one
        var typedIndex = BuildTypedIndex(type, secondaryIndex);
        type.Indexes.Add(typedIndex);
      }

      // Build virtual secondary indexes
      if (descendants.Count > 0)
        foreach (var index in type.Indexes.Where(i => !i.IsPrimary && i.IsVirtual && i.IsTyped).ToList()) {
          var indexesToUnion = new List<IndexInfo>() {index};
          indexesToUnion.AddRange(
            descendants.Select(t => t.Indexes.Single(i => i.DeclaringIndex == index.DeclaringIndex && i.IsTyped)));
          var virtualSecondaryIndex = BuildUnionIndex(type, indexesToUnion);
          type.Indexes.Add(virtualSecondaryIndex);
        }
    }
  }
}