// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{
  internal partial class IndexBuilder
  {
    private void BuildConcreteTableIndexes(TypeInfo type)
    {
      if (type.Indexes.Count > 0 || type.IsStructure) {
        return;
      }

      var typeDef = context.ModelDef.Types[type.UnderlyingType];
      var root = type.Hierarchy.Root;

      // Building declared indexes both secondary and primary (for root of the hierarchy only)
      foreach (var indexDescriptor in typeDef.Indexes) {
        // Skip indef building for inherited indexes
        //
        // NOTE THAT DO optimizes model and removes entities, which has no hierarchy, from model
        // and if they have some indexes then IndexDef.IsInherited of them will be true and it's truth actually,
        // but fields inherited from removed entities will have FieldInfo.IsInherited = false.
        // So, if we check only IndexDef.IsInherited then some indexes will be ignored.
        if (indexDescriptor.IsInherited && indexDescriptor.KeyFields.Select(kf=> type.Fields[kf.Key]).Any(static f => f.IsInherited)) {
          continue;
        }

        var declaredIndex = BuildIndex(type, indexDescriptor, type.IsAbstract);
        type.Indexes.Add(declaredIndex);
        if (!declaredIndex.IsAbstract) {
          context.Model.RealIndexes.Add(declaredIndex);
        }
      }

      // Building primary index for non root entities
      var parent = type.Ancestor;
      if (parent != null) {
        var parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var inheritedIndex = BuildInheritedIndex(type, parentPrimaryIndex, type.IsAbstract);

        // Registering built primary index
        type.Indexes.Add(inheritedIndex);
        if (!inheritedIndex.IsAbstract) {
          context.Model.RealIndexes.Add(inheritedIndex);
        }
      }

      // Building inherited from interfaces indexes
      foreach (var @interface in type.AllInterfaces) {
        foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None).ToChainedBuffer()) {
          if (parentIndex.DeclaringIndex != parentIndex) {
            continue;
          }

          var index = BuildInheritedIndex(type, parentIndex, type.IsAbstract);
          if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name)) {
            index.Dispose();
            continue;
          }

          type.Indexes.Add(index);
          if (!index.IsAbstract) {
            context.Model.RealIndexes.Add(index);
          }
        }
      }

      // Build typed indexes
      foreach (var realIndex in type.Indexes.Find(IndexAttributes.Real).ToChainedBuffer()) {
        if (!untypedIndexes.Contains(realIndex)) {
          continue;
        }
        var typedIndex = BuildTypedIndex(type, realIndex);
        type.Indexes.Add(typedIndex);
      }

      // Build indexes for descendants
      foreach (var descendant in type.DirectDescendants) {
        BuildConcreteTableIndexes(descendant);
      }

      var ancestors = type.Ancestors;
      var descendants = type.AllDescendants;

      // Build primary virtual union index
      if (descendants.Count > 0) {
        var indexesToUnion = descendants.Select(t => BuildViewIndex(type, t.Indexes.PrimaryIndex))
          .Prepend(type.Indexes.PrimaryIndex);

        var virtualPrimaryIndex = BuildUnionIndex(type, indexesToUnion);
        type.Indexes.Add(virtualPrimaryIndex);

      }

      // Build inherited secondary indexes
      var primaryOrVirtualIndexes = ancestors
        .SelectMany(
          ancestor => ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None).ToChainedBuffer());

      foreach (var ancestorIndex in primaryOrVirtualIndexes) {
        if (ancestorIndex.DeclaringIndex != ancestorIndex) {
          continue;
        }

        var secondaryIndex = BuildInheritedIndex(type, ancestorIndex, type.IsAbstract);
        type.Indexes.Add(secondaryIndex);
        if (!secondaryIndex.IsAbstract) {
          context.Model.RealIndexes.Add(secondaryIndex);
        }
        // Build typed index for secondary one
        if (!untypedIndexes.Contains(secondaryIndex)) {
          continue;
        }

        var typedIndex = BuildTypedIndex(type, secondaryIndex);
        type.Indexes.Add(typedIndex);
      }

      // Build virtual secondary indexes
      if (descendants.Count > 0) {
        foreach (var index in type.Indexes.Where(static i => !i.IsPrimary && !i.IsVirtual).ToChainedBuffer()) {
          var isUntyped = untypedIndexes.Contains(index);
          var indexToUnion = isUntyped
            ? type.Indexes.Single(i => i.DeclaringIndex == index.DeclaringIndex && i.IsTyped)
            : index;

          var indexesToUnion = descendants
            .Select(t => t.Indexes.Single(i => i.DeclaringIndex == index.DeclaringIndex && (isUntyped ? i.IsTyped : !i.IsVirtual)))
            .Prepend(indexToUnion);

          var virtualSecondaryIndex = BuildUnionIndex(type, indexesToUnion);
          type.Indexes.Add(virtualSecondaryIndex);
        }
      }
    }
  }
}