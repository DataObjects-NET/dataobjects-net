// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.25

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Building.Builders
{
  partial class IndexBuilder
  {
    private static void ChooseClusteredIndexes()
    {
      var context = BuildingContext.Demand();
      foreach (var hierarchy in context.Model.Hierarchies) {
        var chooser = GetClusteredIndexChooser(hierarchy);
        var queue = new Queue<TypeInfo>();
        var clusteredIndexesMap = new Dictionary<TypeInfo, IndexInfo>();
        queue.Enqueue(hierarchy.Root);
        while (queue.Count > 0) {
          var type = queue.Dequeue();
          foreach (var decendant in type.GetDescendants())
            queue.Enqueue(decendant);
          var clusteredIndexes = type.Indexes
            .Where(index => index.IsClustered && index.IsSecondary)
            .ToList();
          if (clusteredIndexes.Count==0) {
            clusteredIndexesMap.Add(type, null);
            continue;
          }
          foreach (var index in clusteredIndexes)
            if (index.IsPartial)
              throw new DomainBuilderException(string.Format(Strings.ExIndexXCanNotBeBothPartialAndClustered, index));
          var clusteredIndex = chooser.Invoke(type, clusteredIndexes, clusteredIndexesMap);
          clusteredIndexesMap.Add(type, clusteredIndex);
        }
      }
    }

    private static Func<TypeInfo, List<IndexInfo>, Dictionary<TypeInfo, IndexInfo>, IndexInfo> GetClusteredIndexChooser(HierarchyInfo hierarchy)
    {
      switch (hierarchy.InheritanceSchema) {
        case InheritanceSchema.ConcreteTable:
          return ChooseClusteredIndexConcreteTable;
        case InheritanceSchema.ClassTable:
          return ChooseClusteredIndexClassTable;
        case InheritanceSchema.SingleTable:
          return ChooseClusteredIndexSingleTable;
        default:
          throw new ArgumentOutOfRangeException("entityType.Hierarchy.InheritanceSchema");
      }
    }

    private static IndexInfo ChooseClusteredIndexSingleTable(TypeInfo type, List<IndexInfo> indexes, Dictionary<TypeInfo, IndexInfo> clusteredIndexesMap)
    {
      if (indexes.Count > 1)
        throw TooManyClusteredIndexes(type, indexes);

      var index = indexes[0];
      // Since IndexBuilder already moved all indexes to root we check DeclaringType.
      if (type != index.DeclaringType)
        throw new DomainBuilderException(string.Format(
          Strings.ExTypeXDeclaresClusteredIndexYButOnlyRootTypeCanDeclareClusteredIndexInSingleTableHierarchy,
          index.DeclaringType, index));

      DeclareNonClustered(type.Indexes.RealPrimaryIndexes);
      return index;
    }

    private static IndexInfo ChooseClusteredIndexClassTable(TypeInfo type, List<IndexInfo> indexes, Dictionary<TypeInfo, IndexInfo> clusteredIndexesMap)
    {
      if (indexes.Count > 1)
        throw TooManyClusteredIndexes(type, indexes);

      DeclareNonClustered(type.Indexes.RealPrimaryIndexes);
      return indexes[0];
    }

    private static IndexInfo ChooseClusteredIndexConcreteTable(TypeInfo type, List<IndexInfo> indexes, Dictionary<TypeInfo, IndexInfo> clusteredIndexesMap)
    {
      var declaredIndexes = indexes.Where(i => i.DeclaringType==i.ReflectedType).ToList();
      var inheritedIndexes = indexes.Except(declaredIndexes).ToList();

      if (declaredIndexes.Count > 1)
        throw TooManyClusteredIndexes(type, indexes);

      if (declaredIndexes.Count==1) {
        // Use secondary clustered index declared in this type.
        DeclareNonClustered(GetPrimaryIndexForCorrespondingTable(type));
        DeclareNonClustered(inheritedIndexes);
        return declaredIndexes[0];
      }

      // Use secondary clustered index inherited from parent (if any).

      if (inheritedIndexes.Count==0)
        return null;

      if (inheritedIndexes.Count==1) {
        // Use the only inherited clustered index.
        DeclareNonClustered(GetPrimaryIndexForCorrespondingTable(type));
        return inheritedIndexes[0];
      }

      // We need to choose index that is clustered in parent type.

      var parentClusteredIndex = clusteredIndexesMap[type.GetAncestor()];
      if (parentClusteredIndex == null)
        throw Exceptions.InternalError("inheritedIndexes is not empty, but parentClusteredIndex is not specified", Log.Instance);

      var winner = inheritedIndexes.FirstOrDefault(i => i.DeclaringIndex==parentClusteredIndex.DeclaringIndex);
      if (winner == null)
        throw Exceptions.InternalError("matching inherited index is not found", Log.Instance);
      DeclareNonClustered(GetPrimaryIndexForCorrespondingTable(type));
      DeclareNonClustered(inheritedIndexes.Where(i => i != winner));
      return winner;
    }

    private static DomainBuilderException TooManyClusteredIndexes(TypeInfo type, IEnumerable<IndexInfo> indexes)
    {
      return new DomainBuilderException(string.Format(
        Strings.ExTypeXHasMultipleClusteredIndexesY + "\n", type, indexes.ToDelimitedString("\n")));
    }

    private static IEnumerable<IndexInfo> GetPrimaryIndexForCorrespondingTable(TypeInfo type)
    {
      // When inheritance schema is concrete table TypeInfo.RealPrimaryIndexes contains
      // PK for all tables in subhierarchy. This method returns PK for just our table.
      return type.Indexes.RealPrimaryIndexes.Where(index => index.ReflectedType==type);
    }

    private static void DeclareNonClustered(IEnumerable<IndexInfo> indexes)
    {
      foreach (var index in indexes)
        index.Attributes = index.Attributes & ~IndexAttributes.Clustered;
    }
  }
}