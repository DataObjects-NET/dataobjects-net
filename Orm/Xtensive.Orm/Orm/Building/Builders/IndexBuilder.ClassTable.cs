// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    private void BuildClassTableIndexes(TypeInfo type)
    {
      if (type.Indexes.Count > 0)
        return;
      if (type.IsStructure)
        return;

      var root = type.Hierarchy.Root;
      var typeDef = context.ModelDef.Types[type.UnderlyingType];
      var ancestors = type.GetAncestors().ToList();
      var interfaces = type.GetInterfaces();
      
      // Building declared indexes both secondary and primary (for root of the hierarchy only)
      foreach (var indexDescriptor in typeDef.Indexes) {
        // Skip indef building for inherited fields
        var inherited = indexDescriptor.KeyFields
          .Select(kvp => type.Fields[kvp.Key])
          .Any(f => f.IsInherited);
        if (inherited)
          continue;
        var declaredIndex = BuildIndex(type, indexDescriptor, false);

        type.Indexes.Add(declaredIndex);
        context.Model.RealIndexes.Add(declaredIndex);
      }

      // Building primary index for non root entities
      var parent = type.GetAncestor();
      if (parent != null) {
        var parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var inheritedIndex = BuildInheritedIndex(type, parentPrimaryIndex, false);
       
        type.Indexes.Add(inheritedIndex);
        context.Model.RealIndexes.Add(inheritedIndex);
      }

      // Building inherited from interfaces indexes
      foreach (var @interface in interfaces) {
        foreach (var interfaceIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          if (interfaceIndex.DeclaringIndex != interfaceIndex &&
              parent != null &&
              parent.Indexes.Any(i => i.DeclaringIndex == interfaceIndex))
            continue;
          if (type.Indexes.Any(i => i.DeclaringIndex == interfaceIndex))
            continue;
          var index = BuildInheritedIndex(type, interfaceIndex, false);
          if (IndexBuiltOverInheritedFields(index))
            Log.Warning(string.Format(Strings.ExUnableToBuildIndexXBecauseItWasBuiltOverInheritedFields, index.Name));
          else {
            type.Indexes.Add(index);
            context.Model.RealIndexes.Add(index);
          }
        }
      }

      // Build typed indexes
      if (type == root)
        foreach (var realIndex in type.Indexes.Find(IndexAttributes.Real)) {
          if (!untypedIndexes.Contains(realIndex)) 
            continue;
          var typedIndex = BuildTypedIndex(type, realIndex);
          type.Indexes.Add(typedIndex);
        }

      // Build indexes for descendants
      foreach (var descendant in type.GetDescendants())
        BuildClassTableIndexes(descendant);

      // Import inherited indexes
      var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
      if (untypedIndexes.Contains(primaryIndex) && primaryIndex.ReflectedType == root)
        primaryIndex = type.Indexes.Single(i => i.DeclaringIndex == primaryIndex.DeclaringIndex && i.IsTyped);
      var filterByTypes = type.GetDescendants(true).AddOne(type).ToList();

      // Build virtual primary index
      if (ancestors.Count > 0) {
        var baseIndexes = new Stack<IndexInfo>();
        foreach (var ancestor in ancestors.Where(t => t.Fields.Any(f => !f.IsPrimaryKey && !f.IsTypeId && f.IsDeclared))) {
          var ancestorIndex = ancestor.Indexes.Single(i => i.IsPrimary && !i.IsVirtual);
          if (untypedIndexes.Contains(ancestorIndex) && ancestorIndex.ReflectedType == root)
            ancestorIndex = ancestor.Indexes.Single(i => i.DeclaringIndex == ancestorIndex.DeclaringIndex && i.IsTyped);
          if (ancestorIndex.ValueColumns.Count > 0)
            baseIndexes.Push(ancestorIndex);
        }
        if (baseIndexes.Count > 0) {
          if (primaryIndex.ValueColumns.Count > 0 && type.Fields.Any(f => !f.IsPrimaryKey && !f.IsTypeId && f.IsDeclared))
            baseIndexes.Push(primaryIndex);
          else {
            var ancestorIndex = baseIndexes.Pop();
            var filteredIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
            baseIndexes.Push(filteredIndex);
          }
          var virtualPrimaryIndex = baseIndexes.Count == 1 
            ? baseIndexes.Pop()
            : BuildJoinIndex(type, baseIndexes);
          type.Indexes.Add(virtualPrimaryIndex);
        }
      }

      // Build virtual secondary index
      foreach (var ancestorIndex in ancestors.SelectMany(ancestor => ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None))) {
        if (ancestorIndex.DeclaringIndex != ancestorIndex)
          continue;
        var ancestorType = ancestorIndex.ReflectedType;
        var indexToFilter = untypedIndexes.Contains(ancestorIndex) && ancestorIndex.ReflectedType == root
          ? ancestorType.Indexes.Single(i => i.DeclaringIndex == ancestorIndex.DeclaringIndex && i.IsTyped)
          : ancestorIndex;
        var virtualIndex = BuildFilterIndex(type, ancestorIndex, filterByTypes);
        type.Indexes.Add(virtualIndex);
      }
    }

    static bool IndexBuiltOverInheritedFields(IndexInfo index)
    {
      if (index.IsVirtual)
        return false;
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns)
        if (pair.Key.Field.IsInherited && !pair.Key.Field.IsPrimaryKey)
          return true;
      return false;
    }
  }
}