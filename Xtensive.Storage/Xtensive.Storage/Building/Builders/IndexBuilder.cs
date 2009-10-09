// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static partial class IndexBuilder
  {
    public static void BuildIndexes()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Indexes)) {
        var context = BuildingContext.Current;
        CreateInterfaceIndexes();
        foreach (var hierarchy in context.Model.Hierarchies) {
          switch (hierarchy.Schema) {
          case InheritanceSchema.Default:
            BuildClassTableIndexes(hierarchy.Root);
            break;
          case InheritanceSchema.SingleTable:
            BuildSingleTableIndexes(hierarchy.Root);
            break;
          case InheritanceSchema.ConcreteTable:
            BuildConcreteTableIndexes(hierarchy.Root);
            break;
          }
        }
        BuildInterfaceIndexes();
        BuildAffectedIndexes();
      }
    }

    #region Interface support methods

    private static void CreateInterfaceIndexes()
    {
      var context = BuildingContext.Current;
      var processedInterfaces = new HashSet<TypeInfo>();
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface))
        CreateInterfaceIndexes(@interface, processedInterfaces);
    }

    private static void CreateInterfaceIndexes(TypeInfo @interface, ICollection<TypeInfo> processedInterfaces)
    {
      if (processedInterfaces.Contains(@interface))
        return;

      var context = BuildingContext.Current;
      var interfaceDef = context.ModelDef.Types[@interface.UnderlyingType];
        
      // Build virtual declared interface index
      foreach (var indexDescriptor in interfaceDef.Indexes.Where(i => !i.IsPrimary)) {
        var index = BuildIndex(@interface, indexDescriptor, false);

        @interface.Indexes.Add(index);
        if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
          BuildingContext.Current.Model.RealIndexes.Add(index);
      }

      var interfaces = @interface.GetInterfaces();
      foreach (var typeInfo in interfaces)
        CreateInterfaceIndexes(typeInfo, processedInterfaces);


      // Build virtual inherited interface index
      foreach (var parent in interfaces)
        foreach (var parentIndex in parent.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          var index = BuildInheritedIndex(@interface, parentIndex, false);

          @interface.Indexes.Add(index);
          if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
            BuildingContext.Current.Model.RealIndexes.Add(index);
        }
      processedInterfaces.Add(@interface);
    }

    private static void BuildInterfaceIndexes()
    {
      var context = BuildingContext.Current;
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface)) {
        var implementors = @interface.GetImplementors(false).ToList();

        // Building primary indexes
        if (implementors.Count==1) {
          var primaryIndex = implementors[0].Indexes.PrimaryIndex;
          var indexView = BuildViewIndex(@interface, primaryIndex);
          @interface.Indexes.Add(indexView);
        }
        else {
          var interfaceDef = context.ModelDef.Types[@interface.UnderlyingType];
          var indexDef = interfaceDef.Indexes.Single(i => i.IsPrimary);
          var index = BuildIndex(@interface, indexDef, false);
          var lookup = implementors.ToLookup(t => t.Hierarchy);
          var underlyingIndexes = new List<IndexInfo>();
          foreach (var hierarchy in lookup) {
            var underlyingIndex = BuildIndex(@interface, indexDef, false);
            var hierarchyImplementors = hierarchy.ToHashSet();
            switch (hierarchy.Key.Schema) {
              case InheritanceSchema.ClassTable: {
                foreach (var implementor in hierarchy) {
                  var primaryIndex = implementor.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
                  var filterByTypes = new List<TypeInfo>();
                  if (!implementor.IsAbstract)
                    filterByTypes.Add(implementor);
                  filterByTypes.AddRange(GatherDescendants(implementor, hierarchyImplementors));
                  var filterIndex = BuildFilterIndex(implementor, primaryIndex, filterByTypes);
                  var indexView = BuildViewIndex(@interface, filterIndex);
                  underlyingIndex.UnderlyingIndexes.Add(indexView);
                }
                break;
              }
              case InheritanceSchema.SingleTable: {
                var primaryIndex = hierarchy.Key.Root.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
                foreach (var implementor in hierarchy) {
                  var typesToFilter = new List<TypeInfo>();
                  if (!implementor.IsAbstract)
                    typesToFilter.Add(implementor);
                  typesToFilter.AddRange(GatherDescendants(implementor, hierarchyImplementors));
                  var filterIndex = BuildFilterIndex(implementor, primaryIndex, typesToFilter);
                  var indexView = BuildViewIndex(@interface, filterIndex);
                  underlyingIndex.UnderlyingIndexes.Add(indexView);
                }
                break;
              }
              case InheritanceSchema.ConcreteTable: {
                var grouping = hierarchy;
                var allImplementors = @interface.GetImplementors(true)
                  .Where(t => t.Hierarchy == grouping.Key)
                  .ToList();
                var primaryIndexes = allImplementors
                  .Select(t => t.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary))
                  .Select(i => BuildViewIndex(@interface, i));
                underlyingIndex.UnderlyingIndexes.AddRange(primaryIndexes);
                break;
              }
            }
            underlyingIndexes.Add(underlyingIndex);
          }
          if (underlyingIndexes.Count == 1)
            index = underlyingIndexes.First();
          else
            index.UnderlyingIndexes.AddRange(underlyingIndexes);

          @interface.Indexes.Add(index);
          if ((@interface.Attributes & TypeAttributes.Materialized)!=0)
            BuildingContext.Current.Model.RealIndexes.Add(index);
        }

        // Building secondary virtual indexes
        foreach (var index in @interface.Indexes.Where(i=>i.IsVirtual && !i.IsPrimary)) {
          var localIndex = index;
          var lookup = implementors.ToLookup(t => t.Hierarchy);
          var underlyingIndexes = new List<IndexInfo>();
          foreach (var hierarchy in lookup) {
            var hierarchyImplementors = hierarchy.ToHashSet();
            switch (hierarchy.Key.Schema) {
              case InheritanceSchema.ClassTable: {
                var underlyingIndex = index.Clone();
                var indexes = implementors.SelectMany(t => t.Indexes.Where(i => i.DeclaringIndex==localIndex.DeclaringIndex && !i.IsVirtual));
                underlyingIndex.UnderlyingIndexes.AddRange(indexes);
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
              case InheritanceSchema.SingleTable: {
                var rootIndexes = hierarchy.Key.Root.Indexes.Where(i => i.DeclaringIndex == localIndex.DeclaringIndex && !i.IsVirtual);
                var underlyingIndex = index.Clone();
                foreach (var rootIndex in rootIndexes) {
                  var typesToFilter = new List<TypeInfo>();
                  var reflectedType = rootIndex.ReflectedType;
                  if (!reflectedType.IsAbstract)
                    typesToFilter.Add(reflectedType);
                  typesToFilter.AddRange(GatherDescendants(reflectedType, hierarchyImplementors));
                  var filterIndex = BuildFilterIndex(reflectedType, rootIndex, typesToFilter);
                  underlyingIndex.UnderlyingIndexes.Add(rootIndex);
                }
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
              case InheritanceSchema.ConcreteTable: {
                var underlyingIndex = index.Clone();
                var grouping = hierarchy;
                var indexes = @interface.GetImplementors(true)
                  .Where(t => t.Hierarchy == grouping.Key)
                  .Select(t => t.Indexes.Single(i => i.DeclaringIndex == localIndex.DeclaringIndex && !i.IsVirtual));
                underlyingIndex.UnderlyingIndexes.AddRange(indexes);
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
            }
          }
          if (underlyingIndexes.Count == 1) {
            var firstUnderlyingIndex = underlyingIndexes.First();
            index.Attributes = firstUnderlyingIndex.Attributes;
            index.FilterByTypes = firstUnderlyingIndex.FilterByTypes;
            index.UnderlyingIndexes.AddRange(firstUnderlyingIndex.UnderlyingIndexes);
          }
          else
            index.UnderlyingIndexes.AddRange(underlyingIndexes);
        }
      }
    }

    #endregion

    #region Build index methods

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static IndexInfo BuildIndex(TypeInfo typeInfo, IndexDef indexDef, bool buildAbstract)
    {
      var context = BuildingContext.Current;
      Log.Info(Strings.LogBuildingIndexX, indexDef.Name);
      var attributes = !buildAbstract ? indexDef.Attributes : indexDef.Attributes | IndexAttributes.Abstract;

      if (typeInfo.IsInterface && (typeInfo.Attributes & TypeAttributes.Materialized) == 0)
        attributes |= IndexAttributes.Virtual | IndexAttributes.Union;
      else
        attributes |= IndexAttributes.Real;

      var result = new IndexInfo(typeInfo, attributes) {
        FillFactor = indexDef.FillFactor,
        ShortName = indexDef.Name,
        MappingName = indexDef.MappingName
      };

      // Adding key columns
      foreach (KeyValuePair<string, Direction> pair in indexDef.KeyFields) {
        var fieldInfo = typeInfo.Fields[pair.Key];
        var columns = fieldInfo.Columns;

        if (columns.Count==0)
          throw new DomainBuilderException(
            string.Format(Resources.Strings.ExColumnXIsNotFound, pair.Key));

        foreach (var column in columns)
          result.KeyColumns.Add(column, pair.Value);
      }

      // Adding included columns
      foreach (string fieldName in indexDef.IncludedFields) {
        var fieldInfo = typeInfo.Fields[fieldName];
        var columns = fieldInfo.Columns;

        if (columns.Count==0)
          throw new DomainBuilderException(
            string.Format(Resources.Strings.ExColumnXIsNotFound, fieldName));

        foreach (var column in columns)
          result.IncludedColumns.Add(column);
      }

      // Adding system columns as included (only if they are not primary key or index is not primary)
      foreach (ColumnInfo column in typeInfo.Columns.Find(ColumnAttributes.System).Where(c => indexDef.IsPrimary ? !c.IsPrimaryKey : true))
        result.IncludedColumns.Add(column);

      // Adding value columns
      if (indexDef.IsPrimary) {
        IEnumerable<TypeInfo> types;
        if (typeInfo.IsInterface)
          types = typeInfo.GetInterfaces().Union(new[] { typeInfo });
        else {
          var root = typeInfo.Hierarchy.Root;
          var schema = typeInfo.Hierarchy.Schema;
          switch (schema) {
            case InheritanceSchema.SingleTable:
              types = new[] {typeInfo}.Union(root.GetDescendants(true));
              break;
            case InheritanceSchema.ConcreteTable:
              types = typeInfo.GetAncestors().Union(new[] {typeInfo});
              break;
            default:
              types = new[] {typeInfo};
              break;
          }
        }

        var columns = new List<ColumnInfo>();
        columns.AddRange(result.IncludedColumns);
        columns.AddRange(types.SelectMany(t => t.Columns.Find(ColumnAttributes.Inherited | ColumnAttributes.PrimaryKey, MatchType.None)));
        result.ValueColumns.AddRange(GatherValueColumns(columns));
      }
      else {
        foreach (var column in typeInfo.Columns.Where(c => c.IsPrimaryKey)) {
          if (!result.KeyColumns.ContainsKey(column))
            result.ValueColumns.Add(column);
        }
        result.ValueColumns.AddRange(result.IncludedColumns.Where(ic => !result.ValueColumns.Contains(ic.Name)));
      }

      result.Name = context.NameBuilder.BuildIndexName(typeInfo, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    private static IndexInfo BuildInheritedIndex(TypeInfo reflectedType, IndexInfo ancestorIndex, bool buildAbstract)
    {
      Log.Info(Strings.LogBuildingIndexX, ancestorIndex.Name);
      var attributes = IndexAttributes.None;

      if (reflectedType.IsInterface && (reflectedType.Attributes & TypeAttributes.Materialized) == 0)
        attributes = (ancestorIndex.Attributes | IndexAttributes.Virtual | IndexAttributes.Union) &
                     ~(IndexAttributes.Real | IndexAttributes.Join | IndexAttributes.Filtered);
      else
        attributes = (ancestorIndex.Attributes | IndexAttributes.Real) 
          & ~(IndexAttributes.Join | IndexAttributes.Union | IndexAttributes.Filtered | IndexAttributes.Virtual | IndexAttributes.Abstract);
      if (buildAbstract)
        attributes = attributes | IndexAttributes.Abstract;

      var result = new IndexInfo(reflectedType, attributes, ancestorIndex);
      var useFieldMap = ancestorIndex.ReflectedType.IsInterface && !reflectedType.IsInterface;

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in ancestorIndex.KeyColumns) {
        var field = useFieldMap ?
          reflectedType.FieldMap[pair.Key.Field] :
          reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in ancestorIndex.IncludedColumns) {
        var field = useFieldMap ? 
          reflectedType.FieldMap[column.Field] : 
          reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      if (!ancestorIndex.IsPrimary)
        foreach (var column in ancestorIndex.ValueColumns) {
          var field = useFieldMap ?
            reflectedType.FieldMap[column.Field] :
            reflectedType.Fields[column.Field.Name];
          result.ValueColumns.Add(field.Column);
        }
      else if ((reflectedType.Attributes & TypeAttributes.Materialized) != 0)
        result.ValueColumns.AddRange(reflectedType.Columns.Find(ColumnAttributes.PrimaryKey, MatchType.None));

      if (ancestorIndex.IsPrimary && reflectedType.IsEntity) {
        if (reflectedType.Hierarchy.Schema==InheritanceSchema.ClassTable) {
          foreach (var column in ancestorIndex.IncludedColumns) {
            var field = reflectedType.Fields[column.Field.Name];
            result.ValueColumns.Add(field.Column);
          }
          foreach (var column in reflectedType.Columns.Find(ColumnAttributes.Inherited | ColumnAttributes.PrimaryKey, MatchType.None))
            result.ValueColumns.Add(column);
        }
        else if (reflectedType.Hierarchy.Schema==InheritanceSchema.ConcreteTable) {
          foreach (var column in reflectedType.Columns.Find(ColumnAttributes.PrimaryKey, MatchType.None))
            if (!result.ValueColumns.Contains(column.Name))
              result.ValueColumns.Add(column);
        }
      }

      result.Name = BuildingContext.Current.NameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    #endregion

    #region Build virtual index methods

    private static IndexInfo BuildFilterIndex(TypeInfo reflectedType, IndexInfo indexToFilter, IEnumerable<TypeInfo> filterByTypes)
    {
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var attributes = indexToFilter.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique | IndexAttributes.FullText)
        | IndexAttributes.Filtered | IndexAttributes.Virtual;
      var result = new IndexInfo(reflectedType, attributes, indexToFilter, ArrayUtils<IndexInfo>.EmptyArray) {FilterByTypes = filterByTypes.ToList()};

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in indexToFilter.KeyColumns) {
        var field = reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in indexToFilter.IncludedColumns) {
        var field = reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      result.ValueColumns.AddRange(indexToFilter.ValueColumns);

//      var types = reflectedType.GetAncestors()
//        .AddOne(reflectedType)
//        .ToHashSet();
//
//      foreach (var column in indexToFilter.ValueColumns) {
//        var field = column.Field;
//        if (!types.Contains(field.ReflectedType)) 
//          continue;
//        if (field.IsExplicit && field.DeclaringType != reflectedType) 
//          continue;
//        result.ValueColumns.Add(column);
//      }

      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    private static IndexInfo BuildJoinIndex(TypeInfo reflectedType, IEnumerable<IndexInfo> indexesToJoin)
    {
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var firstIndex = indexesToJoin.First();
      var otherIndexes = indexesToJoin.Skip(1).ToArray();
      var attributes = firstIndex.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique | IndexAttributes.FullText)
        | IndexAttributes.Join | IndexAttributes.Virtual;
      var result = new IndexInfo(reflectedType, attributes, firstIndex, otherIndexes);

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in firstIndex.KeyColumns) {
        var field = reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in firstIndex.IncludedColumns) {
        var field = reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      var types = reflectedType.GetAncestors()
        .AddOne(reflectedType)
        .ToHashSet();

      var columnsToAdd = new List<ColumnInfo>();
      foreach (var column in indexesToJoin.SelectMany(i => i.ValueColumns)) {
        var field = column.Field;
        if (!types.Contains(field.DeclaringType)) 
          continue;
        if (field.IsExplicit) {
          var ancestor = reflectedType;
          var skip = false;
          while (ancestor != field.DeclaringType ) {
            FieldInfo ancestorField;
            if (ancestor.Fields.TryGetValue(field.Name, out ancestorField))
              skip = ancestorField.IsDeclared;
            if (skip)
              break;
            ancestor = ancestor.GetAncestor();
          }
          if (skip)
            continue;
        }
        columnsToAdd.Add(column);
      }

      result.ValueColumns.AddRange(GatherValueColumns(columnsToAdd));

      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      foreach (var index in indexesToJoin)
        if ((index.Attributes & IndexAttributes.Abstract) == IndexAttributes.Abstract)
          result.UnderlyingIndexes.Remove(index);

      return result;
    }

    private static IndexInfo BuildUnionIndex(TypeInfo reflectedType, IEnumerable<IndexInfo> indexesToUnion)
    {
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var firstIndex = indexesToUnion.First();
      var otherIndexes = indexesToUnion.Skip(1).ToArray();
      var attributes = firstIndex.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique | IndexAttributes.FullText)
        | IndexAttributes.Union | IndexAttributes.Virtual;
      var result = new IndexInfo(reflectedType, attributes, firstIndex, otherIndexes);

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in firstIndex.KeyColumns) {
        var field = reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in firstIndex.IncludedColumns) {
        var field = reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      result.ValueColumns.AddRange(firstIndex.ValueColumns);

      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      foreach (var index in indexesToUnion)
        if ((index.Attributes & IndexAttributes.Abstract) == IndexAttributes.Abstract)
          result.UnderlyingIndexes.Remove(index);

      return result;
    }

    private static IndexInfo BuildViewIndex(TypeInfo reflectedType, IndexInfo indexToApplyView)
    {
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var attributes = indexToApplyView.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique | IndexAttributes.FullText)
        | IndexAttributes.View | IndexAttributes.Virtual;
      var result = new IndexInfo(reflectedType, attributes, indexToApplyView, ArrayUtils<IndexInfo>.EmptyArray);

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in indexToApplyView.KeyColumns) {
        var field = reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in indexToApplyView.IncludedColumns) {
        var field = reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      var types = reflectedType.IsInterface
        ? indexToApplyView.ReflectedType.GetAncestors().AddOne(indexToApplyView.ReflectedType).ToHashSet()
        : reflectedType.GetAncestors().AddOne(reflectedType).ToHashSet();

      var indexReflectedType = indexToApplyView.ReflectedType;
      var keyLength = indexToApplyView.KeyColumns.Count;
      var columnMap = new List<int>(Enumerable.Range(0, keyLength));
      for (int i = 0; i < indexToApplyView.ValueColumns.Count; i++) {
        var column = indexToApplyView.ValueColumns[i];
        var columnField = column.Field;
        var declaringType = columnField.DeclaringType;
        if (!types.Contains(declaringType)) 
          continue;

        if (reflectedType.IsInterface) {
          if (!columnField.IsInterfaceImplementation)
            continue;

          var interfaceFields = indexReflectedType.FieldMap.GetImplementedInterfaceFields(columnField);
          var field = interfaceFields.FirstOrDefault(f => f.DeclaringType == reflectedType);
          if (field == null)
            continue;
          result.ValueColumns.Add(field.Column);
        }
        else {
          if (columnField.IsExplicit) {
            var ancestor = reflectedType;
            var skip = false;
            while (ancestor != columnField.DeclaringType ) {
              FieldInfo ancestorField;
              if (ancestor.Fields.TryGetValue(columnField.Name, out ancestorField))
                skip = ancestorField.IsDeclared;
              if (skip)
                break;
              ancestor = ancestor.GetAncestor();
            }
            if (skip)
              continue;
          }
          var field = reflectedType.Fields[columnField.Name];
            result.ValueColumns.Add(field.Column);
        }
        columnMap.Add(keyLength + i);
      }

      result.SelectColumns = columnMap;
      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }


    #endregion

    #region Helper methods

    private static IEnumerable<TypeInfo> GatherDescendants(TypeInfo type, ICollection<TypeInfo> hierarchyImplementors)
    {
      foreach (var descendant in type.GetDescendants()) {
        if (hierarchyImplementors.Contains(descendant) || descendant.IsAbstract)
          continue;
        yield return descendant;
        foreach (var typeInfo in GatherDescendants(descendant, hierarchyImplementors))
          yield return typeInfo;
      }
    }

    private static IEnumerable<ColumnInfo> GatherValueColumns(IEnumerable<ColumnInfo> columns)
    {
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var valueColumns = new ColumnInfoCollection();
      foreach (var column in columns)  {
        if (valueColumns.Contains(column.Name)) {
          if (column.IsSystem)
            continue;
          var clone = column.Clone();
          clone.Name = nameBuilder.BuildColumnName(column);
          valueColumns.Add(clone);
        }
        else
          valueColumns.Add(column);
      }
      return valueColumns;
    }

    private static ColumnGroup BuildColumnGroup(IndexInfo index)
    {
      var reflectedType = index.ReflectedType;
      List<int> keyColumns;
      List<int> columns;
      if (index.IsPrimary) {
        keyColumns = new List<int>(index.KeyColumns.Select((_, i) => i));
        columns = new List<int>(keyColumns);
        columns.AddRange(index.ValueColumns.Select((_,i) => keyColumns.Count + i));
      }
      else {
        keyColumns = new List<int>(index.ValueColumns.Take(reflectedType.Columns.Count(c => c.IsPrimaryKey)).Select((_, i) => index.KeyColumns.Count + i));
        columns = new List<int>(index.KeyColumns.Select((_, i) => i));
        columns.AddRange(index.ValueColumns.Select((_, i) => index.KeyColumns.Count + i));
      }
      return new ColumnGroup(reflectedType, keyColumns, columns);
    }

    private static void BuildAffectedIndexes()
    {
      var context = BuildingContext.Current;
      foreach (var typeInfo in context.Model.Types) {
        if (typeInfo.IsEntity) {
          var ancestors = new HashSet<TypeInfo>();
          ProcessAncestors(typeInfo, ancestor => ancestors.Add(ancestor));

          var type = typeInfo;
          Action<IEnumerable<IndexInfo>> extractor = null;
          extractor = 
            delegate(IEnumerable<IndexInfo> source) {
              foreach (var indexInfo in source) {
                if (!indexInfo.IsVirtual) {
                  if ((ancestors.Contains(indexInfo.ReflectedType) || indexInfo.ReflectedType == type) &&
                    !type.AffectedIndexes.Contains(indexInfo)) {
                    type.AffectedIndexes.Add(indexInfo);
                    foreach (KeyValuePair<ColumnInfo, Direction> pair in indexInfo.KeyColumns) {
                      if (indexInfo.IsPrimary) continue;
                      if (pair.Key.Indexes.Count == 0)
                        pair.Key.Indexes = new NodeCollection<IndexInfo> {indexInfo};
                      else if (!pair.Key.Indexes.Contains(indexInfo))
                        pair.Key.Indexes.Add(indexInfo);
                    }
                  }
                }
                extractor(indexInfo.UnderlyingIndexes);
              }
            };

          extractor(typeInfo.Indexes);
          if (typeInfo.Hierarchy.Schema == InheritanceSchema.ClassTable) {
            var rootIndex = typeInfo.Hierarchy.Root.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
            if (!type.AffectedIndexes.Contains(rootIndex))
              type.AffectedIndexes.Add(rootIndex);
          }
        }
        else if ((typeInfo.Attributes & TypeAttributes.Materialized) != 0) {
          var primaryIndex = typeInfo.Indexes.PrimaryIndex;
          foreach (var descendant in typeInfo.GetDescendants(true).Where(t => t.IsEntity).Distinct()) {
            descendant.AffectedIndexes.Add(primaryIndex);
            foreach (var indexInfo in typeInfo.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
              var descendantIndex = descendant.Indexes.Where(i => i.DeclaringIndex == indexInfo.DeclaringIndex).FirstOrDefault();
              if (descendantIndex != null)
                foreach (KeyValuePair<ColumnInfo, Direction> pair in descendantIndex.KeyColumns) {
                  if (pair.Key.Indexes.Count == 0)
                    pair.Key.Indexes = new NodeCollection<IndexInfo> {indexInfo};
                  else
                    pair.Key.Indexes.Add(indexInfo);
                }
            }
          }
        }
      }
    }

    private static void ProcessAncestors(TypeInfo typeInfo, Action<TypeInfo> ancestorProcessor)
    {
      var context = BuildingContext.Current;
      var root = typeInfo.Hierarchy.Root;

      if (root == typeInfo) return;
      var ancestorTypeInfo = context.Model.Types.FindAncestor(typeInfo);
      if (ancestorTypeInfo != null)
        do {
          ancestorProcessor(ancestorTypeInfo);
          if (ancestorTypeInfo == root)
            break;
          ancestorTypeInfo = context.Model.Types.FindAncestor(ancestorTypeInfo);
        } while (ancestorTypeInfo != null);
    }

    #endregion
  }
}
