// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
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
        BuildingContext context = BuildingContext.Current;
        CreateInterfaceIndexes();
        foreach (HierarchyInfo hierarchy in context.Model.Hierarchies) {
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
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface)) {
        var interfaceDef = context.ModelDef.Types[@interface.UnderlyingType];
        
        // Build virtual declared interface index
        foreach (var indexDescriptor in interfaceDef.Indexes.Where(i => !i.IsPrimary)) {
          var index = BuildIndex(@interface, indexDescriptor, false);

          @interface.Indexes.Add(index);
          if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
            BuildingContext.Current.Model.RealIndexes.Add(index);
        }

        // Build virtual inherited interface index
        foreach (var parent in @interface.GetInterfaces())
          foreach (var parentIndex in parent.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
            var index = BuildInheritedIndex(@interface, parentIndex, false);

            @interface.Indexes.Add(index);
            if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
              BuildingContext.Current.Model.RealIndexes.Add(index);
          }
      }
    }

    private static void BuildInterfaceIndexes()
    {
      var context = BuildingContext.Current;
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface)) {
        var implementors = new List<TypeInfo>(@interface.GetImplementors(false));

        if (implementors.Count==1)
          @interface.Indexes.Add(implementors[0].Indexes.PrimaryIndex);
        else {
          var interfaceDef = context.ModelDef.Types[@interface.UnderlyingType];
          var indexDef = interfaceDef.Indexes.Where(i => i.IsPrimary).Single();
          var index = BuildIndex(@interface, indexDef, false);
          var lookup = implementors.ToLookup(t => t.Hierarchy);
          var underlyingIndexes = new List<IndexInfo>();
          foreach (var hierarchy in lookup) {
            var underlyingIndex = BuildIndex(@interface, indexDef, false);
            switch (hierarchy.Key.Schema) {
              case InheritanceSchema.Default: {
                var primaryIndexes = implementors.Select(t => t.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary)).ToList();
                underlyingIndex.UnderlyingIndexes.AddRange(primaryIndexes);
                break;
              }
              case InheritanceSchema.SingleTable: {
                underlyingIndex.UnderlyingIndexes.Add(hierarchy.Key.Root.Indexes.PrimaryIndex);
                underlyingIndex.Attributes = (underlyingIndex.Attributes & ~IndexAttributes.Union) | IndexAttributes.Filtered;
                break;
              }
              case InheritanceSchema.ConcreteTable: {
                var grouping = hierarchy;
                var primaryIndexes = @interface.GetImplementors(true)
                  .Where(t => t.Hierarchy == grouping.Key)
                  .Select(t => t.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary));
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

        foreach (var index in @interface.Indexes.Where(i=>i.IsVirtual && !i.IsPrimary)) {
          var localIndex = index;
          var lookup = implementors.ToLookup(t => t.Hierarchy);
          var underlyingIndexes = new List<IndexInfo>();
          foreach (var hierarchy in lookup) {
            switch (hierarchy.Key.Schema) {
              case InheritanceSchema.Default: {
                var underlyingIndex = index.Clone();
                var indexes = implementors.SelectMany(t => t.Indexes.Where(i => i.DeclaringIndex==localIndex)).ToList();
                underlyingIndex.UnderlyingIndexes.AddRange(indexes);
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
              case InheritanceSchema.SingleTable: {
                var rootIndexes = hierarchy.Key.Root.Indexes.Where(i => i.DeclaringIndex==localIndex).ToList();
                foreach (var rootIndex in rootIndexes) {
                  var underlyingIndex = index.Clone();
                  underlyingIndex.Attributes = (underlyingIndex.Attributes & ~IndexAttributes.Union) | IndexAttributes.Filtered;
                  underlyingIndex.UnderlyingIndexes.Add(rootIndex);
                  underlyingIndexes.Add(underlyingIndex);
                }
                break;
              }
              case InheritanceSchema.ConcreteTable: {
                var underlyingIndex = index.Clone();
                var grouping = hierarchy;
                var primaryIndexes = @interface.GetImplementors(true)
                  .Where(t => t.Hierarchy == grouping.Key)
                  .Select(t => t.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary));
                underlyingIndex.UnderlyingIndexes.AddRange(primaryIndexes);
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
            }
          }
          if (underlyingIndexes.Count == 1) {
            var firstUnderlyingIndex = underlyingIndexes.First();
            index.Attributes = firstUnderlyingIndex.Attributes;
            index.UnderlyingIndexes.AddRange(firstUnderlyingIndex.UnderlyingIndexes);
          }
          else
            index.UnderlyingIndexes.AddRange(underlyingIndexes);
        }
      }
    }

    #endregion

    #region Build IndexInfo methods

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
        result.ValueColumns.AddRange(GetValueColumns(columns));
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

      var result = new IndexInfo(reflectedType, ancestorIndex, attributes);
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

    private static IndexInfo BuildVirtualIndex(TypeInfo reflectedType, IndexAttributes indexAttributes, IndexInfo baseIndex, params IndexInfo[] baseIndexes)
    {
      Log.Info(Strings.LogBuildingIndexX, baseIndex.Name);
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var attributes = baseIndex.Attributes 
        & ~(IndexAttributes.Abstract | IndexAttributes.Join | IndexAttributes.Union | IndexAttributes.Filtered | IndexAttributes.Real) 
        | indexAttributes | IndexAttributes.Virtual;
      var result = new IndexInfo(reflectedType, attributes, baseIndex, baseIndexes);

      var allBaseIndexes = new List<IndexInfo> { baseIndex };
      allBaseIndexes.AddRange(baseIndexes);

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in baseIndex.KeyColumns) {
        var field = reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      var useFieldMap = baseIndex.ReflectedType.IsInterface && !reflectedType.IsInterface;

      // Adding included columns
      foreach (var column in baseIndex.IncludedColumns) {
        var field = useFieldMap ?
          reflectedType.FieldMap[column.Field] :
          reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      if ((indexAttributes & IndexAttributes.Join) > 0)
        result.ValueColumns.AddRange(GetValueColumns(allBaseIndexes.SelectMany(i => i.ValueColumns)));
      else if ((indexAttributes & IndexAttributes.Filtered) > 0) {
        var types = reflectedType.GetAncestors().ToList();
        types.Add(reflectedType);
        types.AddRange(reflectedType.GetDescendants(true));
        var columns = types.SelectMany(t => t.Columns.Find(ColumnAttributes.Declared)).ToList();
        var columnsToAdd = new List<ColumnInfo>();
        foreach (var indexColumn in baseIndex.ValueColumns)
          for (int i = 0; i < columns.Count; i++) {
            var column = columns[i];
            if (column.Equals(indexColumn)) {
              columnsToAdd.Add(column);
              columns.RemoveAt(i);
              break;
            }
          }
        result.ValueColumns.AddRange(GetValueColumns(columnsToAdd));
      }
      else if ((indexAttributes & IndexAttributes.Union) > 0) {
//        var ancestors = reflectedType.GetAncestors().AddOne(reflectedType).ToHashSet();
//        var columns = new List<ColumnInfo>(baseIndex.ValueColumns);
//        var columnsToAdd = baseIndexes
//          .SelectMany(i => i.ValueColumns)
//          .Where(c => ancestors.Contains(c.Field.DeclaringType))
//          .ToList();
//        foreach (var column in columnsToAdd) {
//          bool exists = false;
//          foreach (var existingColumn in columns)
//            if (column.Field.Name == existingColumn.Field.Name && column.Field.DeclaringType == existingColumn.Field.DeclaringType)
//            {
//              exists = true;
//              break;
//            }
//          if (!exists)
//            columns.Add(column);
//        }
        result.ValueColumns.AddRange(GetValueColumns(baseIndex.ValueColumns));
      }

      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      foreach (var index in allBaseIndexes)
        if ((index.Attributes & IndexAttributes.Abstract) == IndexAttributes.Abstract)
          result.UnderlyingIndexes.Remove(index);

      return result;
    }

    #endregion

    #region Helper methods

    private static ColumnInfoCollection GetValueColumns(IEnumerable<ColumnInfo> columns)
    {
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var valueColumns = new ColumnInfoCollection();
      foreach (var column in columns)  {
        if (column.Field.IsInterfaceImplementation) {
          bool doNotAdd = false;
          var implementedInterfaceFields = column.Field.ReflectedType.FieldMap.GetImplementedInterfaceFields(column.Field).ToList();
          foreach (var field in implementedInterfaceFields) {
            if (valueColumns.Contains(field.Column)) {
              doNotAdd = true;
              continue;
            }
          }
          if (doNotAdd)
            continue;
        }

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

    public static void BuildAffectedIndexes()
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
                      if (!indexInfo.IsPrimary) {
                        if (pair.Key.Indexes.Count == 0) {
                          pair.Key.Indexes = new NodeCollection<IndexInfo>();
                          pair.Key.Indexes.Add(indexInfo);
                        }
                        else if (!pair.Key.Indexes.Contains(indexInfo))
                          pair.Key.Indexes.Add(indexInfo);
                      }
                    }
                  }
                }
                extractor(indexInfo.UnderlyingIndexes);
              }
            };

          extractor(typeInfo.Indexes);
        }
        else if ((typeInfo.Attributes & TypeAttributes.Materialized) != 0) {
          var primaryIndex = typeInfo.Indexes.PrimaryIndex;
          foreach (var descendant in typeInfo.GetDescendants(true).Where(t => t.IsEntity).Distinct()) {
            descendant.AffectedIndexes.Add(primaryIndex);
            foreach (var indexInfo in typeInfo.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
              var descendantIndex = descendant.Indexes.Where(i => i.DeclaringIndex == indexInfo.DeclaringIndex).FirstOrDefault();
              if (descendantIndex != null)
                foreach (KeyValuePair<ColumnInfo, Direction> pair in descendantIndex.KeyColumns) {
                  if (pair.Key.Indexes.Count == 0) {
                    pair.Key.Indexes = new NodeCollection<IndexInfo>();
                    pair.Key.Indexes.Add(indexInfo);
                  }
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

      if (root != typeInfo) {
        var ancestorTypeInfo = context.Model.Types.FindAncestor(typeInfo);
        if (ancestorTypeInfo != null)
          do {
            ancestorProcessor(ancestorTypeInfo);
            if (ancestorTypeInfo == root)
              break;
            ancestorTypeInfo = context.Model.Types.FindAncestor(ancestorTypeInfo);
          } while (ancestorTypeInfo != null);
      }
    }

    #endregion
  }
}
