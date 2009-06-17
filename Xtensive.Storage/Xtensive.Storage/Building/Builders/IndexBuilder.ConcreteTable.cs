// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
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

      var primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
      var indexDefinitions = typeDef.Indexes.Where(i => !i.IsPrimary).ToList();

      // Building primary index for root of the hierarchy
      if (primaryIndexDefinition != null)
        using (var scope = new LogCaptureScope(context.Log)) {
          var primaryIndex = BuildIndex(root, primaryIndexDefinition);
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            type.Indexes.Add(primaryIndex);
            context.Model.RealIndexes.Add(primaryIndex);
          }
        }

      // Building declared indexes
      foreach (var indexDescriptor in indexDefinitions)
        using (var scope = new LogCaptureScope(context.Log)) {
          var indexInfo = BuildIndex(type, indexDescriptor); 
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            type.Indexes.Add(indexInfo);
            context.Model.RealIndexes.Add(indexInfo);
          }
        }

      // Building primary index for non root entities
      var parent = type.GetAncestor();
      if (parent != null) {
        var parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var primaryIndex = BuildInheritedIndex(type, parentPrimaryIndex);
       
        // Registering built primary index
        type.Indexes.Add(primaryIndex);
        context.Model.RealIndexes.Add(primaryIndex);
      }

      // Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {

          if (parentIndex.DeclaringIndex == parentIndex)
            using (var scope = new LogCaptureScope(context.Log)) {
              var index = BuildInheritedIndex(type, parentIndex);
              // TODO: AK: discover this check
              if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
                continue;
              if (!scope.IsCaptured(LogEventTypes.Error)) {
                type.Indexes.Add(index);
                context.Model.RealIndexes.Add(index);
              }
            }
        }
      }

      // Build indexes for descendants
      foreach (var descendant in type.GetDescendants())
        BuildConcreteTableIndexes(descendant);

      // Import inherited indexes
      if (type.IsEntity) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var ancestors = type.GetAncestors().ToList();
        var descendants = type.GetDescendants(true).ToList();

        // Build virtual primary index
        if (descendants.Count > 0) {
          var baseIndexes = descendants.SelectMany(t => t.Indexes.Where(i => i.IsPrimary && !i.IsVirtual)).ToList();
          var virtualPrimaryIndex = BuildVirtualIndex(type, IndexAttributes.Union, primaryIndex, baseIndexes.ToArray());
          type.Indexes.Add(virtualPrimaryIndex);
        }

        // Build inherited secondary indexes
        foreach (var ancestor in ancestors)
          foreach (var ancestorSecondaryIndex in ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None)) {
            if (ancestorSecondaryIndex.DeclaringIndex == ancestorSecondaryIndex) {
              var secondaryIndex = BuildInheritedIndex(type, ancestorSecondaryIndex);
              type.Indexes.Add(secondaryIndex);
              context.Model.RealIndexes.Add(secondaryIndex);
            }
          }

        // Build virtual secondary indexes
        if (descendants.Count > 0)
          foreach (var index in type.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None)) {
            var secondaryIndex = index;
            var baseIndexes = descendants.SelectMany(t => t.Indexes.Where(i => i.DeclaringIndex==secondaryIndex.DeclaringIndex)).ToList();
            var virtualSecondaryIndex = BuildVirtualIndex(type, IndexAttributes.Union, index, baseIndexes.ToArray());
            type.Indexes.Add(virtualSecondaryIndex);
          }
      }
    }
  }
}