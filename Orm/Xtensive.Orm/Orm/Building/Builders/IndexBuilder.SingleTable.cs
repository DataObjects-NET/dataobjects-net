// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.06.17

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{
  partial class IndexBuilder
  {
    private void BuildSingleTableIndexes(TypeInfo type)
    {
      if (type.Indexes.Count > 0)
        return;
      if (type.IsStructure)
        return;

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
        if (indexDescriptor.IsInherited && indexDescriptor.KeyFields.Select(kf => type.Fields[kf.Key]).Any(f => f.IsInherited))
          continue;
        var declaredIndex = BuildIndex(type, indexDescriptor, false);
        root.Indexes.Add(declaredIndex);
        context.Model.RealIndexes.Add(declaredIndex);
      }

      var parent = type.Ancestor;
      // Building inherited from interfaces indexes
      foreach (var @interface in type.Interfaces) {
        foreach (var interfaceIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          if (root.Indexes.Any(i => i.DeclaringIndex == interfaceIndex.DeclaringIndex && i.ReflectedType == type))
            continue;
          var index = BuildInheritedIndex(type, interfaceIndex, false);
          root.Indexes.Add(index);
          context.Model.RealIndexes.Add(index);
        }
      }

      var types = type.Ancestors.ToHashSet();
      types.Add(type);

      // Build typed indexes
      foreach (var realIndex in root.Indexes.Find(IndexAttributes.Real)) {
        if (!types.Contains(realIndex.ReflectedType))
          continue;
        if (!untypedIndexes.Contains(realIndex))
          continue;
        if (root.Indexes.Any(i => i.DeclaringIndex == realIndex.DeclaringIndex && i.ReflectedType == type && i.IsTyped))
          continue;
        var typedIndex = BuildTypedIndex(type, realIndex);
        root.Indexes.Add(typedIndex);
      }

      // Build indexes for descendants
      foreach (var descendant in type.Descendants) {
        BuildSingleTableIndexes(descendant);
      }

      if (type == root) return;
      var descendants = type.RecursiveDescendants;

      var primaryIndexFilterTypes = new List<TypeInfo>();
      if (!type.IsAbstract)
        primaryIndexFilterTypes.Add(type);
      primaryIndexFilterTypes.AddRange(descendants);
      
      // Import inherited indexes
      var ancestorIndexes = root.Indexes
        .Where(i => types.Contains(i.ReflectedType) && !i.IsTyped)
        .Reverse()
        .Select(i => untypedIndexes.Contains(i)
          ? root.Indexes.Single(index => index.DeclaringIndex == i.DeclaringIndex && index.ReflectedType == type && index.IsTyped)
          : i)
        .ToList();
      foreach (var ancestorIndex in ancestorIndexes) {
        if (type.Indexes.Any(i => 
            i.DeclaringIndex == ancestorIndex.DeclaringIndex &&
            i.ReflectedType == type && 
            i.IsVirtual))
          continue;
        if (ancestorIndex.DeclaringType.IsInterface) {
          var filteredDescendants = descendants
            .Where(t => !t.IsAbstract && !t.Interfaces.Contains(ancestorIndex.DeclaringType));
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
    }
  }
}