// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Builders
{
  internal partial class IndexBuilder
  {
    private static void BuildClassTableIndexes(TypeInfo type)
    {
      if (type.Indexes.Count > 0)
        return;
      if (type.IsStructure)
        return;

      var context = BuildingContext.Current;
      var typeDef = context.ModelDef.Types[type.UnderlyingType];

      var primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
      var indexDefinitions = typeDef.Indexes.Where(i => !i.IsPrimary).ToList();

      // Building primary index for root of the hierarchy
      if (primaryIndexDefinition != null)
        BuildHierarchyPrimaryIndex(type, primaryIndexDefinition);

      // Building declared indexes
      foreach (var indexDescriptor in indexDefinitions)
        BuildDeclaredIndex(type, indexDescriptor);

      // Building primary index for non root entities
      var parent = type.GetAncestor();
      if (parent != null) {
        var parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var primaryIndex = BuildInheritedIndex(type, parentPrimaryIndex, false);
       
        // Registering built primary index
        type.Indexes.Add(primaryIndex);
        context.Model.RealIndexes.Add(primaryIndex);
      }

      // Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        if ((parent==null) || !parent.GetInterfaces(true).Contains(@interface))
          foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          if (parentIndex.DeclaringIndex == parentIndex)
            using (var scope = new LogCaptureScope(context.Log)) {
              var index = BuildInheritedIndex(type, parentIndex, false);
              //TODO: AK: discover this check
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
        BuildClassTableIndexes(descendant);

      // Import inherited indexes
      if (type.IsEntity) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var ancestors = type.GetAncestors().ToList();
        // Build virtual primary index
        if (ancestors.Count > 0) {
          var baseIndexes = new List<IndexInfo>();
          foreach (var ancestor in ancestors) {
            var ancestorIndex = ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Real, MatchType.Full).First();
            var baseIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorIndex);
            baseIndexes.Add(baseIndex);
          }
          baseIndexes.Add(primaryIndex);
          
          var virtualPrimaryIndex = BuildVirtualIndex(type, IndexAttributes.Join, baseIndexes[0], baseIndexes.Skip(1).ToArray());
          virtualPrimaryIndex.IsPrimary = true;
          type.Indexes.Add(virtualPrimaryIndex);
        }

        // Build virtual secondary index
        foreach (var ancestor in ancestors)
          foreach (var ancestorSecondaryIndex in ancestor.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
            var virtualSecondaryIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorSecondaryIndex);
            type.Indexes.Add(virtualSecondaryIndex);
          }
      }
    }

    private static void BuildDeclaredIndex(TypeInfo type, IndexDef indexDescriptor)
    {
      var indexInfo = BuildIndex(type, indexDescriptor, false); 

      type.Indexes.Add(indexInfo);
      BuildingContext.Current.Model.RealIndexes.Add(indexInfo);
    }

    private static void BuildHierarchyPrimaryIndex(TypeInfo type, IndexDef primaryIndexDefinition)
    {
      var primaryIndex = BuildIndex(type.Hierarchy.Root, primaryIndexDefinition, false);

      type.Indexes.Add(primaryIndex);
      BuildingContext.Current.Model.RealIndexes.Add(primaryIndex);
    }
  }
}