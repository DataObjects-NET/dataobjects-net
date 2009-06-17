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
    private static void BuildSingleTableIndexes(TypeInfo type)
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
            root.Indexes.Add(primaryIndex);
            context.Model.RealIndexes.Add(primaryIndex);
          }
        }

      // Building declared indexes
      foreach (var indexDescriptor in indexDefinitions)
        using (var scope = new LogCaptureScope(context.Log)) {
          var indexInfo = BuildIndex(type, indexDescriptor); 
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            root.Indexes.Add(indexInfo);
            context.Model.RealIndexes.Add(indexInfo);
          }
        }

      var parent = type.GetAncestor();
      // Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        if ((parent == null) || !parent.GetInterfaces(true).Contains(@interface))
          foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
            if (parentIndex.DeclaringIndex == parentIndex)
              using (var scope = new LogCaptureScope(context.Log)) {
                var index = BuildInheritedIndex(type, parentIndex);
                // TODO: AK: discover this check
                if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
                  continue;
                if (!scope.IsCaptured(LogEventTypes.Error)) {
                  root.Indexes.Add(index);
                  context.Model.RealIndexes.Add(index);
                }
              }
        }
      }

      // Build indexes for descendants
      foreach (var descendant in type.GetDescendants())
        BuildSingleTableIndexes(descendant);

      if (type != root) {
        var ancestors = type.GetAncestors().ToList();
        ancestors.Add(type);
        foreach (var ancestorIndex in root.Indexes) {
          var interfaces = type.GetInterfaces(true);
          if ((ancestorIndex.DeclaringType.IsInterface && interfaces.Contains(ancestorIndex.DeclaringType)) || ancestors.Contains(ancestorIndex.DeclaringType)) {
            var virtualIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorIndex);
            if (!type.Indexes.Contains(virtualIndex.Name))
              type.Indexes.Add(virtualIndex);
          }
        }
      }
    }
  }
}