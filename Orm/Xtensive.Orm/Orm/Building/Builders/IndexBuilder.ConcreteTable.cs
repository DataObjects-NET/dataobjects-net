// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{
  partial class IndexBuilder
  {
    private void BuildConcreteTableIndexes(TypeInfo type)
    {
      if (type.Indexes.Count > 0)
        return;
      if (type.IsStructure)
        return;

      var typeDef = context.ModelDef.Types[type.UnderlyingType];
      var root = type.Hierarchy.Root;

      var possiblyInheritedIndexes = new List<IndexDef>();

      // Building declared indexes both secondary and primary (for root of the hierarchy only)
      foreach (var indexDescriptor in typeDef.Indexes) {
        // Skip indexdef building if all fields are inherited and parent class has this indexdef
        var inherited = indexDescriptor.KeyFields
          .Select(kvp => type.Fields[kvp.Key])
          .All(f => f.IsInherited);
        if (inherited) {
          possiblyInheritedIndexes.Add(indexDescriptor);
          continue;
        }

        var declaredIndex = BuildIndex(type, indexDescriptor, type.IsAbstract); 

        type.Indexes.Add(declaredIndex);
        if (!declaredIndex.IsAbstract)
          context.Model.RealIndexes.Add(declaredIndex);
      }

      foreach (var possiblyInheritedIndex in possiblyInheritedIndexes) {
        var indexIsInherited = possiblyInheritedIndex.KeyFields
          .Select(keyField => context.ModelDef.Types[type.Fields[keyField.Key].DeclaringType.UnderlyingType])
          .Any(t => t.Indexes.Contains(possiblyInheritedIndex.Name));
        if (indexIsInherited)
          continue;
        var declaredIndex = BuildIndex(type, possiblyInheritedIndex, type.IsAbstract);
        type.Indexes.Add(declaredIndex);
        if (!declaredIndex.IsAbstract)
          context.Model.RealIndexes.Add(declaredIndex);
      }

      // Building primary index for non root entities
      var parent = type.GetAncestor();
      if (parent != null) {
        var parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var inheritedIndex = BuildInheritedIndex(type, parentPrimaryIndex, type.IsAbstract);
       
        // Registering built primary index
        type.Indexes.Add(inheritedIndex);
        if (!inheritedIndex.IsAbstract)
          context.Model.RealIndexes.Add(inheritedIndex);
      }

      // Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          if (parentIndex.DeclaringIndex != parentIndex) 
            continue;
          var index = BuildInheritedIndex(type, parentIndex, type.IsAbstract);
          if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
            continue;
          type.Indexes.Add(index);
          if (!index.IsAbstract)
            context.Model.RealIndexes.Add(index);
        }
      }

      // Build typed indexes
      foreach (var realIndex in type.Indexes.Find(IndexAttributes.Real)) {
        if (!untypedIndexes.Contains(realIndex)) 
          continue;
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
        var secondaryIndex = BuildInheritedIndex(type, ancestorIndex, type.IsAbstract);
        type.Indexes.Add(secondaryIndex);
        if (!secondaryIndex.IsAbstract)
          context.Model.RealIndexes.Add(secondaryIndex);
        // Build typed index for secondary one
        if (!untypedIndexes.Contains(secondaryIndex))
          continue;
        var typedIndex = BuildTypedIndex(type, secondaryIndex);
        type.Indexes.Add(typedIndex);
      }

      // Build virtual secondary indexes
      if (descendants.Count > 0)
        foreach (var index in type.Indexes.Where(i => !i.IsPrimary && !i.IsVirtual).ToList()) {
          var isUntyped = untypedIndexes.Contains(index);
          var indexToUnion = isUntyped
            ? type.Indexes.Single(i => i.DeclaringIndex == index.DeclaringIndex && i.IsTyped)
            : index;

          var indexesToUnion = new List<IndexInfo>() {indexToUnion};
          indexesToUnion.AddRange(descendants
            .Select(t => t.Indexes.Single(i => i.DeclaringIndex == index.DeclaringIndex && (isUntyped ? i.IsTyped : !i.IsVirtual))));
          var virtualSecondaryIndex = BuildUnionIndex(type, indexesToUnion);
          type.Indexes.Add(virtualSecondaryIndex);
        }
    }
  }
}