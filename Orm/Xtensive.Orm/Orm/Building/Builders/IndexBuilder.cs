// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed partial class IndexBuilder
  {
    private readonly BuildingContext context;

    public static void BuildIndexes(BuildingContext context)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Indexes)) {
        new IndexBuilder(context).BuildAll();
      }
    }

    private void BuildAll()
    {
      CreateInterfaceIndexes();
      BuildIndexesForHierarchies();
      BuildInterfaceIndexes();
      CleanupTypedIndexes();
      BuildAffectedIndexes();
      BuildFullTextIndexes();
      BuildFiltersForPartialIndexes();
      ChooseClusteredIndexes();
    }

    private void BuildIndexesForHierarchies()
    {
      foreach (var hierarchy in context.Model.Hierarchies) {
        switch (hierarchy.InheritanceSchema) {
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
    }

    #region Interface support methods

    private void CreateInterfaceIndexes()
    {
      var processedInterfaces = new HashSet<TypeInfo>();
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface))
        CreateInterfaceIndexes(@interface, processedInterfaces);
    }

    private void CreateInterfaceIndexes(TypeInfo @interface, ICollection<TypeInfo> processedInterfaces)
    {
      if (processedInterfaces.Contains(@interface))
        return;

      var interfaceDef = context.ModelDef.Types[@interface.UnderlyingType];
        
      // Build virtual declared interface index
      foreach (var indexDescriptor in interfaceDef.Indexes.Where(i => !i.IsPrimary)) {
        var index = BuildIndex(@interface, indexDescriptor, false);

        @interface.Indexes.Add(index);
        if (@interface.IsMaterialized)
          context.Model.RealIndexes.Add(index);
      }

      var interfaces = @interface.GetInterfaces();
      foreach (var typeInfo in interfaces)
        CreateInterfaceIndexes(typeInfo, processedInterfaces);

      // Build virtual inherited interface index
      foreach (var parent in interfaces)
        foreach (var parentIndex in parent.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          var index = BuildInheritedIndex(@interface, parentIndex, false);
          if (@interface.Indexes.Contains(index.Name)) 
            continue;
          @interface.Indexes.Add(index);
          if (@interface.IsMaterialized)
            context.Model.RealIndexes.Add(index);
        }
      processedInterfaces.Add(@interface);
    }

    private void BuildInterfaceIndexes()
    {
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
            switch (hierarchy.Key.InheritanceSchema) {
              case InheritanceSchema.ClassTable: {
                foreach (var implementor in hierarchy) {
                  var interfaceFields = @interface.Fields.ToHashSet();
                  var typeIndexes = new Queue<IndexInfo>();
                  var type = implementor;
                  var typedIndex = (IndexInfo)null;
                  var foundFields = new List<FieldInfo>();
                  while (interfaceFields.Count > 0) {
                    foundFields.Clear();
                    foreach (var field in interfaceFields) {
                      FieldInfo typeField;
                      if (type.FieldMap.TryGetValue(field, out typeField))
                        if ((typeField.IsDeclared || typeField.IsSystem || typeField.IsPrimaryKey))
                          foundFields.Add(field);
                    }
                    if (foundFields.Count > 0) {
                      var typeIndex = type.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
                      if (context.UntypedIndexes.Contains(typeIndex)) {
                        if (type == hierarchy.Key.Root) {
                          typeIndex = null;
                          typedIndex = type.Indexes.Single(i => i.IsPrimary && i.IsTyped);
                        }
                        else {
                          typeIndex = type.Indexes.Single(i => i.IsPrimary && !i.IsVirtual);
                          if (typedIndex == null)
                            typedIndex = hierarchy.Key.Root.Indexes.Single(i => i.IsPrimary && i.IsTyped);
                        }
                      }
                      if (typeIndex != null)
                        typeIndexes.Enqueue(typeIndex);
                      foreach (var foundField in foundFields)
                        interfaceFields.Remove(foundField);
                    }
                    type = type.GetAncestor();
                  }
                  var filterByTypes = new List<TypeInfo>();
                  if (!implementor.IsAbstract)
                    filterByTypes.Add(implementor);
                  filterByTypes.AddRange(GatherDescendants(implementor, hierarchyImplementors));
                  var indexesToJoin = new List<IndexInfo>();
                  var filterIndex = typedIndex != null
                    ? BuildFilterIndex(implementor, typedIndex, filterByTypes)
                    : BuildFilterIndex(implementor, typeIndexes.Dequeue(), filterByTypes);
                  indexesToJoin.Add(filterIndex);
                  indexesToJoin.AddRange(typeIndexes);
                  var indexToApplyView = indexesToJoin.Count > 1 
                    ? BuildJoinIndex(implementor, indexesToJoin) 
                    : indexesToJoin[0];
                  var indexView = BuildViewIndex(@interface, indexToApplyView);
                  underlyingIndex.UnderlyingIndexes.Add(indexView);
                }
                break;
              }
              case InheritanceSchema.SingleTable: {
                var primaryIndex = hierarchy.Key.Root.Indexes.Single(i => i.ReflectedType == hierarchy.Key.Root && i.IsPrimary && !i.IsVirtual);
                if (context.UntypedIndexes.Contains(primaryIndex))
                  primaryIndex = hierarchy.Key.Root.Indexes.Single(i => i.ReflectedType == hierarchy.Key.Root && i.IsPrimary && i.IsTyped);
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
                  .Where(t => t.Hierarchy == grouping.Key && !t.IsAbstract)
                  .ToList();
                var primaryIndexes = allImplementors
                  .Select(t => new {Index = t.Indexes.Single(i => i.IsPrimary && !i.IsVirtual), Type = t})
                  .Select(p => context.UntypedIndexes.Contains(p.Index) 
                    ? p.Type.Indexes.Single(i => i.IsPrimary && i.IsTyped) 
                    : p.Index)
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
          if (@interface.IsMaterialized)
            context.Model.RealIndexes.Add(index);
        }

        // Building secondary virtual indexes
        foreach (var interfaceIndex in @interface.Indexes.Where(i=>i.IsVirtual && !i.IsPrimary)) {
          var localIndex = interfaceIndex;
          var lookup = implementors.ToLookup(t => t.Hierarchy);
          var underlyingIndexes = new List<IndexInfo>();
          foreach (var hierarchy in lookup) {
            var grouping = hierarchy;
            var underlyingIndex = interfaceIndex.Clone();
            var hierarchyImplementors = hierarchy.ToHashSet();
            switch (hierarchy.Key.InheritanceSchema) {
              case InheritanceSchema.ClassTable: {
                foreach (var implementor in hierarchyImplementors) {
                  var index = implementor.Indexes.SingleOrDefault(i => i.DeclaringIndex == localIndex.DeclaringIndex && !i.IsVirtual);
                  if (index == null)
                    throw new NotSupportedException(string.Format(Strings.ExUnableToBuildIndexXBecauseItWasBuiltOverInheritedFields, interfaceIndex.Name));
                  var filterByTypes = new List<TypeInfo>();
                  if (!implementor.IsAbstract)
                    filterByTypes.Add(implementor);
                  var subHierarchyNodeCount = implementor.GetDescendants(true).Count() + filterByTypes.Count;
                  filterByTypes.AddRange(GatherDescendants(implementor, hierarchyImplementors));
                  if (filterByTypes.Count != subHierarchyNodeCount)
                    index = BuildFilterIndex(implementor, index, filterByTypes);
                  underlyingIndex.UnderlyingIndexes.Add(index);
                }
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
              case InheritanceSchema.SingleTable: {
                var rootIndexes = hierarchy.Key.Root.Indexes.Where(i => i.DeclaringIndex == localIndex.DeclaringIndex && implementors.Contains(i.ReflectedType) && !i.IsVirtual);
                foreach (var rootIndex in rootIndexes) {
                  var index = context.UntypedIndexes.Contains(rootIndex)
                    ? hierarchy.Key.Root.Indexes.Single(i => i.DeclaringIndex == localIndex.DeclaringIndex && i.ReflectedType == rootIndex.ReflectedType && i.IsTyped)
                    : rootIndex;
                  var filterByTypes = new List<TypeInfo>();
                  var reflectedType = rootIndex.ReflectedType;
                  if (!reflectedType.IsAbstract)
                    filterByTypes.Add(reflectedType);
                  filterByTypes.AddRange(GatherDescendants(reflectedType, hierarchyImplementors));
                  index = BuildFilterIndex(reflectedType, index, filterByTypes);
                  underlyingIndex.UnderlyingIndexes.Add(index);
                }
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
              case InheritanceSchema.ConcreteTable: {
                var indexes = @interface.GetImplementors(true)
                  .Where(t => t.Hierarchy == grouping.Key)
                  .Select(t => new { Index = t.Indexes.Single(i => i.DeclaringIndex == localIndex.DeclaringIndex && !i.IsVirtual), Type = t })
                  .Select(p => context.UntypedIndexes.Contains(p.Index)
                    ? p.Type.Indexes.Single(i => i.DeclaringIndex == localIndex.DeclaringIndex && i.IsTyped)
                    : p.Index);
                underlyingIndex.UnderlyingIndexes.AddRange(indexes);
                underlyingIndexes.Add(underlyingIndex);
                break;
              }
            }
          }
          if (underlyingIndexes.Count == 1) {
            var firstUnderlyingIndex = underlyingIndexes.First();
            interfaceIndex.Attributes = firstUnderlyingIndex.Attributes;
            interfaceIndex.FilterByTypes = firstUnderlyingIndex.FilterByTypes;
            interfaceIndex.UnderlyingIndexes.AddRange(firstUnderlyingIndex.UnderlyingIndexes);
          }
          else
            interfaceIndex.UnderlyingIndexes.AddRange(underlyingIndexes);
        }
      }
    }

    #endregion

    #region Build index methods

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private IndexInfo BuildIndex(TypeInfo typeInfo, IndexDef indexDef, bool buildAbstract)
    {
      Log.Info(Strings.LogBuildingIndexX, indexDef.Name);
      var attributes = !buildAbstract ? indexDef.Attributes : indexDef.Attributes | IndexAttributes.Abstract;

      if (typeInfo.IsInterface && !typeInfo.IsMaterialized)
        attributes |= IndexAttributes.Virtual | IndexAttributes.Union;
      else
        attributes |= IndexAttributes.Real;

      var result = new IndexInfo(typeInfo, attributes) {
        FillFactor = indexDef.FillFactor,
        FilterExpression = indexDef.FilterExpression,
        ShortName = indexDef.Name,
        MappingName = indexDef.MappingName
      };

      var skipTypeId = false;
      if (typeInfo.Hierarchy != null) {
        if (typeInfo.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable)
          skipTypeId = true;
        else if (typeInfo.Hierarchy.TypeDiscriminatorMap != null)
          skipTypeId = true;
      }
      if (typeInfo.Fields.Any(f => f.IsTypeId && f.IsPrimaryKey))
        skipTypeId = false;

      // Adding key columns
      foreach (KeyValuePair<string, Direction> pair in indexDef.KeyFields) {
        var fieldInfo = typeInfo.Fields[pair.Key];
        var columns = fieldInfo.Columns;

        if (columns.Count==0)
          throw new DomainBuilderException(
            string.Format(Strings.ExColumnXIsNotFound, pair.Key));

        foreach (var column in columns)
          result.KeyColumns.Add(column, pair.Value);
      }

      // Adding included columns
      foreach (string fieldName in indexDef.IncludedFields) {
        var fieldInfo = typeInfo.Fields[fieldName];
        var columns = fieldInfo.Columns;

        if (columns.Count==0)
          throw new DomainBuilderException(
            string.Format(Strings.ExColumnXIsNotFound, fieldName));

        foreach (var column in columns)
          result.IncludedColumns.Add(column);
      }

      // Adding system columns as included (only if they are not primary key or index is not primary)
      foreach (ColumnInfo column in typeInfo.Columns.Find(ColumnAttributes.System).Where(c => indexDef.IsPrimary ? !c.IsPrimaryKey : true)) {
        if (skipTypeId && column.IsSystem && column.Field.IsTypeId)
          continue;
        result.IncludedColumns.Add(column);
      }

      // Adding value columns
      if (indexDef.IsPrimary) {
        IEnumerable<TypeInfo> types;
        if (typeInfo.IsInterface)
          types = typeInfo.GetInterfaces().Union(new[] { typeInfo });
        else {
          var root = typeInfo.Hierarchy.Root;
          var schema = typeInfo.Hierarchy.InheritanceSchema;
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
        columns.AddRange(types.SelectMany(t => t.Columns
          .Find(ColumnAttributes.Inherited | ColumnAttributes.PrimaryKey, MatchType.None)
          .Where(c => skipTypeId ? !(c.Field.IsTypeId && c.IsSystem) : true)));
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
      if (skipTypeId)
        context.UntypedIndexes.Add(result);

      return result;
    }

    private IndexInfo BuildInheritedIndex(TypeInfo reflectedType, IndexInfo ancestorIndex, bool buildAbstract)
    {
      Log.Info(Strings.LogBuildingIndexX, ancestorIndex.Name);
      var attributes = IndexAttributes.None;

      if (reflectedType.IsInterface && !reflectedType.IsMaterialized)
        attributes = (ancestorIndex.Attributes | IndexAttributes.Virtual | IndexAttributes.Union) &
                     ~(IndexAttributes.Real | IndexAttributes.Join | IndexAttributes.Filtered);
      else
        attributes = (ancestorIndex.Attributes | IndexAttributes.Real) 
          & ~(IndexAttributes.Join | IndexAttributes.Union | IndexAttributes.Filtered | IndexAttributes.Virtual | IndexAttributes.Abstract);
      if (buildAbstract)
        attributes = attributes | IndexAttributes.Abstract;

      var result = new IndexInfo(reflectedType, attributes, ancestorIndex);
      var useFieldMap = ancestorIndex.ReflectedType.IsInterface && !reflectedType.IsInterface;

      var skipTypeId = false;
      if (reflectedType.Hierarchy != null) {
        if (reflectedType.Hierarchy.InheritanceSchema == InheritanceSchema.ConcreteTable)
          skipTypeId = true;
        else if (reflectedType.Hierarchy.TypeDiscriminatorMap != null)
          skipTypeId = true;
      }
      if (reflectedType.Fields.Any(f => f.IsTypeId && f.IsPrimaryKey))
        skipTypeId = false;


      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in ancestorIndex.KeyColumns) {
        var field = useFieldMap ?
          reflectedType.FieldMap[pair.Key.Field] :
          reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in ancestorIndex.IncludedColumns) {
        if (skipTypeId && column.IsSystem && column.Field.IsTypeId)
          continue;
        var field = useFieldMap ? 
          reflectedType.FieldMap[column.Field] : 
          reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      if (!ancestorIndex.IsPrimary)
        foreach (var column in ancestorIndex.ValueColumns) {
          if (skipTypeId && column.IsSystem && column.Field.IsTypeId)
            continue;
          var field = useFieldMap ?
            reflectedType.FieldMap[column.Field] :
            reflectedType.Fields[column.Field.Name];
          result.ValueColumns.Add(field.Column);
        }
      else if (reflectedType.IsMaterialized)
        result.ValueColumns.AddRange(reflectedType.Columns
          .Find(ColumnAttributes.PrimaryKey, MatchType.None)
          .Where(c => skipTypeId ? !(c.IsSystem && c.Field.IsTypeId) : true));

      if (ancestorIndex.IsPrimary && reflectedType.IsEntity) {
        if (reflectedType.Hierarchy.InheritanceSchema==InheritanceSchema.ClassTable) {
          foreach (var column in ancestorIndex.IncludedColumns) {
            if (skipTypeId && column.IsSystem && column.Field.IsTypeId)
              continue;
            var field = reflectedType.Fields[column.Field.Name];
            result.ValueColumns.Add(field.Column);
          }
          foreach (var column in reflectedType.Columns.Find(ColumnAttributes.Inherited | ColumnAttributes.PrimaryKey, MatchType.None)) {
            if (skipTypeId && column.IsSystem && column.Field.IsTypeId)
              continue;
            result.ValueColumns.Add(column);
          }
        }
        else if (reflectedType.Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable) {
          foreach (var column in reflectedType.Columns.Find(ColumnAttributes.PrimaryKey, MatchType.None)) {
            if (skipTypeId && column.IsSystem && column.Field.IsTypeId)
              continue;
            if (!result.ValueColumns.Contains(column.Name))
              result.ValueColumns.Add(column);
          }
        }
      }

      
      result.Name = context.NameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);
      if (skipTypeId)
        context.UntypedIndexes.Add(result);

      return result;
    }

    #endregion

    #region Build virtual index methods

    private IndexInfo BuildTypedIndex(TypeInfo reflectedType, IndexInfo realIndex)
    {
      if (realIndex.IsVirtual)
        throw new InvalidOperationException();
      var nameBuilder = context.NameBuilder;
      var attributes = realIndex.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique | IndexAttributes.Abstract)
        | IndexAttributes.Typed | IndexAttributes.Virtual;
      var result = new IndexInfo(reflectedType, attributes, realIndex, ArrayUtils<IndexInfo>.EmptyArray);

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in realIndex.KeyColumns) {
        var field = reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in realIndex.IncludedColumns) {
        var field = reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding TypeId column
      if (realIndex.IsPrimary)
        result.ValueColumns.Add(reflectedType.Columns.Single(c => c.IsSystem && c.Field.IsTypeId));
      // Adding value columns
      result.ValueColumns.AddRange(realIndex.ValueColumns);
      // Adding TypeId column
      if (!realIndex.IsPrimary)
        result.ValueColumns.Add(reflectedType.Columns.Single(c => c.IsSystem && c.Field.IsTypeId));

      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    private IndexInfo BuildFilterIndex(TypeInfo reflectedType, IndexInfo indexToFilter, IEnumerable<TypeInfo> filterByTypes)
    {
      var nameBuilder = context.NameBuilder;
      var attributes = indexToFilter.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique | IndexAttributes.Abstract)
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

      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    private IndexInfo BuildJoinIndex(TypeInfo reflectedType, IEnumerable<IndexInfo> indexesToJoin)
    {
      var nameBuilder = context.NameBuilder;
      var firstIndex = indexesToJoin.First();
      var otherIndexes = indexesToJoin.Skip(1).ToArray();
      var attributes = firstIndex.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique )
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
      var typeOrder = reflectedType.GetAncestors()
        .AddOne(reflectedType)
        .Select((t, i) => new {Type = t, Index = i})
        .ToDictionary(a => a.Type, a => a.Index);
      var types = reflectedType.GetAncestors()
        .AddOne(reflectedType)
        .ToHashSet();

      var valueColumnMap = new List<List<int>>();
      foreach (var index in indexesToJoin) {
        var columnMap = new List<int>();
        int columnIndex = -1;
        foreach (var column in index.ValueColumns) {
          columnIndex++;
          if (columnIndex < result.IncludedColumns.Count)
            continue;
          var field = column.Field;
          if (!types.Contains(field.DeclaringType))
            continue;
          if (field.IsExplicit) {
            var ancestor = reflectedType;
            var skip = false;
            while (ancestor != field.DeclaringType) {
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
          columnMap.Add(columnIndex);
        }
        valueColumnMap.Add(columnMap);
      }
      var orderedIndexes = indexesToJoin
        .Select((index, i) => new {index, columns = valueColumnMap[i], i})
        .OrderBy(a => typeOrder[a.index.ValueColumns.First().Field.ReflectedType])
        .ToList();

      var columnsToAdd = new List<ColumnInfo>();
      var valueColumnMapping = new List<Pair<int, List<int>>>();
      for (var i = 0; i < orderedIndexes.Count; i++) {
        var item = orderedIndexes[i];
        if (valueColumnMapping.Count == 0)
          item.columns.InsertRange(0, Enumerable.Range(0, result.IncludedColumns.Count));
        foreach (var columnIndex in item.columns) {
          var column = item.index.ValueColumns[columnIndex];
          columnsToAdd.Add(column);
        }
        valueColumnMapping.Add(new Pair<int, List<int>>(item.i, item.columns));
      }

      result.ValueColumnsMap = valueColumnMapping;
      result.ValueColumns.AddRange(GatherValueColumns(columnsToAdd));
      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    private IndexInfo BuildUnionIndex(TypeInfo reflectedType, IEnumerable<IndexInfo> indexesToUnion)
    {
      var nameBuilder = context.NameBuilder;
      var firstIndex = indexesToUnion.First();
      var otherIndexes = indexesToUnion.Skip(1).ToArray();
      var attributes = firstIndex.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique )
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

    private IndexInfo BuildViewIndex(TypeInfo reflectedType, IndexInfo indexToApplyView)
    {
      var nameBuilder = context.NameBuilder;
      var attributes = indexToApplyView.Attributes
        & (IndexAttributes.Primary | IndexAttributes.Secondary | IndexAttributes.Unique | IndexAttributes.Abstract)
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
      var types = (reflectedType.IsInterface
        ? indexToApplyView.ReflectedType.GetAncestors().AddOne(indexToApplyView.ReflectedType)
        : reflectedType.GetAncestors().AddOne(reflectedType)).ToHashSet();
      var interfaces = (reflectedType.IsInterface
        ? reflectedType.GetInterfaces(true).AddOne(reflectedType)
        : Enumerable.Empty<TypeInfo>()).ToHashSet();

      var indexReflectedType = indexToApplyView.ReflectedType;
      var keyLength = indexToApplyView.KeyColumns.Count;
      var columnMap = new List<int>();
      var valueColumns = new List<ColumnInfo>(reflectedType.Columns.Count);
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
          var interfaceField = interfaceFields.FirstOrDefault(f => interfaces.Contains(f.DeclaringType));
          if (interfaceField == null)
            continue;
          var field = reflectedType.Fields[interfaceField.Name];
          valueColumns.Add(field.Column);
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
          valueColumns.Add(field.Column);
        }
        columnMap.Add(keyLength + i);
      }
      var actualColumnMapping = valueColumns
        .Zip(columnMap, (column, sourceIndex) => new {column, sourceIndex})
        .OrderBy(p => reflectedType.Columns.IndexOf(p.column))
        .ToList();
      valueColumns.Clear();
      columnMap.Clear();
      columnMap.AddRange(Enumerable.Range(0, keyLength));
      foreach (var columnMapping in actualColumnMapping) {
        valueColumns.Add(columnMapping.column);
        columnMap.Add(columnMapping.sourceIndex);
      }

      result.ValueColumns.AddRange(valueColumns);
      result.SelectColumns = columnMap;
      result.Name = nameBuilder.BuildIndexName(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }


    #endregion

    #region Helper methods

    private static IEnumerable<TypeInfo> GatherDescendants(TypeInfo type, ICollection<TypeInfo> hierarchyImplementors)
    {
      return type.GetDescendants(true).Where(t => !t.IsAbstract).Except(hierarchyImplementors);
    }

    private IEnumerable<ColumnInfo> GatherValueColumns(IEnumerable<ColumnInfo> columns)
    {
      var nameBuilder = context.NameBuilder;
      var valueColumns = new ColumnInfoCollection(null, "ValueColumns");
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

    private ColumnGroup BuildColumnGroup(IndexInfo index)
    {
      var reflectedType = index.ReflectedType;
      var keyColumns = index.IsPrimary
        ? Enumerable.Range(0, index.KeyColumns.Count).ToList()
        : index.KeyColumns
            .Select(pair => pair.Key)
            .Concat(index.ValueColumns)
            .Select((c, i) => new {c, i})
            .Where(arg => arg.c.IsPrimaryKey)
            .Select(arg => arg.i)
            .ToList();
      var columns = Enumerable.Range(0, index.KeyColumns.Count + index.ValueColumns.Count).ToList();
      return new ColumnGroup(reflectedType, keyColumns, columns);
    }

    private void CleanupTypedIndexes()
    {
      
      foreach (var typeInfo in context.Model.Types.Where(t => t.IsEntity)) {
        var indexes = typeInfo.Indexes.Where(i => i.IsVirtual).ToList();
        var typedIndexes = indexes.Where(i => i.IsTyped);
        foreach (var typedIndex in typedIndexes) {
          bool remove = false;
          foreach (var index in indexes)
            if (index.UnderlyingIndexes.Contains(typedIndex)) {
              remove = true;
              break;
            }
          if (remove)
            typeInfo.Indexes.Remove(typedIndex);
        }
      }
    }

    private void BuildAffectedIndexes()
    {
      
      foreach (var typeInfo in context.Model.Types)
        if (typeInfo.IsEntity)
          BuildAffectedIndexesForEntity(typeInfo);
        else if (typeInfo.IsInterface && typeInfo.IsMaterialized)
          BuildAffectedIndexesForMaterializedInterface(typeInfo);
    }
    
    private void BuildAffectedIndexesForEntity(TypeInfo typeInfo)
    {
      var ancestors = new HashSet<TypeInfo>();
      ProcessAncestors(typeInfo, ancestor => ancestors.Add(ancestor));

      ExtractAffectedIndexes(typeInfo, typeInfo.Indexes, ancestors);
      if (typeInfo.Hierarchy.InheritanceSchema==InheritanceSchema.ClassTable)
        // Add primary indexes of all ancestors to affected indexes list.
        // This is an ugly hack :-(
        foreach (var ancestor in ancestors) {
          var primaryIndex = ancestor.Indexes
            .FindFirst(IndexAttributes.Real | IndexAttributes.Primary);
          if (!typeInfo.AffectedIndexes.Contains(primaryIndex))
            typeInfo.AffectedIndexes.Add(primaryIndex);
        }
    }
    
    private void ExtractAffectedIndexes(
      TypeInfo typeInfo, IEnumerable<IndexInfo> sources, ICollection<TypeInfo> ancestors)
    {
      foreach (var indexInfo in sources) {
        if (!indexInfo.IsVirtual) {
          bool shouldProcess =
            (ancestors.Contains(indexInfo.ReflectedType) || indexInfo.ReflectedType==typeInfo)
            && !typeInfo.AffectedIndexes.Contains(indexInfo);
          if (shouldProcess) {
            typeInfo.AffectedIndexes.Add(indexInfo);
            foreach (var pair in indexInfo.KeyColumns) {
              if (indexInfo.IsPrimary)
                continue;
              var columnInfo = pair.Key;
              if (columnInfo.Indexes.Count==0)
                columnInfo.Indexes = new NodeCollection<IndexInfo>(columnInfo, "Indexes") {
                  indexInfo
                };
              else if (!columnInfo.Indexes.Contains(indexInfo))
                columnInfo.Indexes.Add(indexInfo);
            }
          }
        }
        ExtractAffectedIndexes(typeInfo, indexInfo.UnderlyingIndexes, ancestors);
      }
    }

    private void BuildAffectedIndexesForMaterializedInterface(TypeInfo typeInfo)
    {
      var primaryIndex = typeInfo.Indexes.PrimaryIndex;
      foreach (var descendant in typeInfo.GetDescendants(true).Where(t => t.IsEntity).Distinct()) {
        descendant.AffectedIndexes.Add(primaryIndex);
        foreach (var indexInfo in typeInfo.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          var descendantIndex = descendant.Indexes.Where(i => i.DeclaringIndex==indexInfo.DeclaringIndex).FirstOrDefault();
          if (descendantIndex!=null)
            foreach (var pair in descendantIndex.KeyColumns) {
              var columnInfo = pair.Key;
              if (columnInfo.Indexes.Count==0)
                columnInfo.Indexes = new NodeCollection<IndexInfo>(columnInfo, "Indexes") {
                  indexInfo
                };
              else
                columnInfo.Indexes.Add(indexInfo);
            }
        }
      }
    }

    private void ProcessAncestors(TypeInfo typeInfo, Action<TypeInfo> ancestorProcessor)
    {
      
      var root = typeInfo.Hierarchy.Root;
      if (root==typeInfo)
        return;
      var ancestor = context.Model.Types.FindAncestor(typeInfo);
      if (ancestor==null)
        return;
      do {
        ancestorProcessor.Invoke(ancestor);
        if (ancestor==root)
          break;
        ancestor = context.Model.Types.FindAncestor(ancestor);
      } while (ancestor!=null);
    }

    private void BuildFiltersForPartialIndexes()
    {
      
      foreach (var index in context.Model.RealIndexes.Where(index => index.FilterExpression!=null))
        PartialIndexFilterBuilder.BuildFilter(index);
    }

    #endregion

    // Constructors

    private IndexBuilder(BuildingContext context)
    {
      this.context = context;
    }
  }
}
