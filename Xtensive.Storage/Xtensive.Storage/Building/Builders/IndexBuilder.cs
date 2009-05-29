// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.02

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class IndexBuilder
  {
    public static IndexDef DefineForeignKey(TypeDef type, FieldDef field)
    {
      var index = new IndexDef {IsSecondary = true};
      index.KeyFields.Add(field.Name);
      index.Name = BuildingContext.Current.NameBuilder.Build(type, index);
      return index;
    }

    public static void BuildIndexes()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Indexes)) {
        BuildingContext context = BuildingContext.Current;
        foreach (HierarchyInfo hierarchy in context.Model.Hierarchies) {

          CreateInterfaceIndexes(hierarchy);

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
          BuildInterfaceIndexes(hierarchy);
        }

        BuildAffectedIndexes();
      }
    }

    #region Interface support methods

    private static void CreateInterfaceIndexes(HierarchyInfo hierarchy)
    {
      var context = BuildingContext.Current;
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface).Where(i => i.Hierarchy==hierarchy)) {
        var interfaceDef = context.ModelDef.Types[@interface.UnderlyingType];
        
        // Build virtual declared interface index
        foreach (var indexDescriptor in interfaceDef.Indexes)
          BuildVirtualDeclaredInterfaceIndex(@interface, indexDescriptor);

        // Build virtual inherited interface index
        foreach (var parent in @interface.GetInterfaces())
          foreach (var parentIndex in parent.Indexes.Find(IndexAttributes.Primary, MatchType.None))
            BuildVirtualInheritedInterfaceIndex(@interface, parentIndex);
      }
    }

    private static void BuildVirtualDeclaredInterfaceIndex(TypeInfo @interface, IndexDef indexDescriptor)
    {
      var index = BuildIndex(@interface, indexDescriptor);

      @interface.Indexes.Add(index);
      if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
        BuildingContext.Current.Model.RealIndexes.Add(index);
    }

    private static void BuildVirtualInheritedInterfaceIndex(TypeInfo @interface, IndexInfo parentIndex)
    {
      var index = BuildInheritedIndex(@interface, parentIndex);

      @interface.Indexes.Add(index);
      if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
        BuildingContext.Current.Model.RealIndexes.Add(index);
    }

    private static void BuildInterfaceIndexes(HierarchyInfo hierarchy)
    {
      var context = BuildingContext.Current;
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface).Where(i => i.Hierarchy == hierarchy)) {
        var implementors = new List<TypeInfo>(@interface.GetImplementors(false));

        if (implementors.Count==1)
          @interface.Indexes.Add(implementors[0].Indexes.PrimaryIndex);
        else {
          var rootDef = context.ModelDef.Types[hierarchy.Root.UnderlyingType];
          var primaryIndexDefinition = rootDef.Indexes.Where(i => i.IsPrimary).First();
          var index = BuildIndex(@interface, primaryIndexDefinition);
          switch (hierarchy.Schema) {
          case InheritanceSchema.Default: {
            var primaryIndexes = implementors.Select(t => t.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary)).ToList();
            index.UnderlyingIndexes.AddRange(primaryIndexes);
          }
            break;
          case InheritanceSchema.SingleTable:
            index.UnderlyingIndexes.Add(hierarchy.Root.Indexes.PrimaryIndex);
            break;
          case InheritanceSchema.ConcreteTable: {
            var allImplementors = new List<TypeInfo>(@interface.GetImplementors(true));
            var primaryIndexes = allImplementors.Select(t => t.Indexes.FindFirst(IndexAttributes.Real | IndexAttributes.Primary)).ToList();
            index.UnderlyingIndexes.AddRange(primaryIndexes);
          }
            break;
          }
          @interface.Indexes.Add(index);
          if ((@interface.Attributes & TypeAttributes.Materialized)!=0)
            BuildingContext.Current.Model.RealIndexes.Add(index);
        }

        foreach (var index in @interface.Indexes.Where(i=>i.IsVirtual && !i.IsPrimary)) {
          var localIndex = index;
          switch(hierarchy.Schema) {
            case InheritanceSchema.Default:
              index.UnderlyingIndexes.AddRange(implementors.SelectMany(t => t.Indexes).Where(i => i.DeclaringIndex==localIndex));
              break;
            case InheritanceSchema.SingleTable:
              index.UnderlyingIndexes.AddRange(hierarchy.Root.Indexes.Where(i => i.DeclaringIndex==localIndex));
              break;
            case InheritanceSchema.ConcreteTable: {
                var allImplementors = new List<TypeInfo>(@interface.GetImplementors(true));
                index.UnderlyingIndexes.AddRange(allImplementors.SelectMany(t => t.Indexes).Where(i => i.DeclaringIndex == localIndex && !i.IsVirtual));
              }
              break;
            }
        }
      }
    }

    #endregion

    #region Inheritance schema support methods

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
        var primaryIndex = BuildInheritedIndex(type, parentPrimaryIndex);
       
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
              var index = BuildInheritedIndex(type, parentIndex);
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
      var indexInfo = BuildIndex(type, indexDescriptor); 

      type.Indexes.Add(indexInfo);
      BuildingContext.Current.Model.RealIndexes.Add(indexInfo);
    }

    private static void BuildHierarchyPrimaryIndex(TypeInfo type, IndexDef primaryIndexDefinition)
    {
      var primaryIndex = BuildIndex(type.Hierarchy.Root, primaryIndexDefinition);

      type.Indexes.Add(primaryIndex);
      BuildingContext.Current.Model.RealIndexes.Add(primaryIndex);
    }

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

    #endregion

    #region Build IndexInfo methods

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static IndexInfo BuildIndex(TypeInfo typeInfo, IndexDef indexDef)
    {
      var context = BuildingContext.Current;
      Log.Info(Strings.LogBuildingIndexX, indexDef.Name);
      var result = new IndexInfo(typeInfo, indexDef.Attributes) {
        FillFactor = indexDef.FillFactor, 
        ShortName = indexDef.Name, 
        MappingName = indexDef.MappingName
      };

      // Adding key columns
      foreach (KeyValuePair<string, Direction> pair in indexDef.KeyFields) {
        var fieldInfo = typeInfo.Fields[pair.Key];
        var columns = fieldInfo.ExtractColumns();

        if (columns.Count==0)
          throw new DomainBuilderException(
            string.Format(Resources.Strings.ExColumnXIsNotFound, pair.Key));

        foreach (var column in columns)
          result.KeyColumns.Add(column, pair.Value);
      }

      // Adding included columns
      foreach (string fieldName in indexDef.IncludedFields) {
        var fieldInfo = typeInfo.Fields[fieldName];
        var columns = fieldInfo.ExtractColumns();

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
        var root = typeInfo.Hierarchy.Root;
        var schema = typeInfo.Hierarchy.Schema;
        IEnumerable<TypeInfo> types;
        if (typeInfo.IsInterface)
          types = typeInfo.GetInterfaces().Union(new[] { typeInfo });
        else
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

        var columns = new List<ColumnInfo>();
        columns.AddRange(result.IncludedColumns);
        columns.AddRange(types.SelectMany(t => t.Columns.Find(ColumnAttributes.Inherited | ColumnAttributes.PrimaryKey, MatchType.None)));
        result.ValueColumns.AddRange(GetValueColumns(columns));
      }
      else {
        foreach (var fieldName in typeInfo.Hierarchy.KeyInfo.Fields.Select(pair => pair.Key.Name)) {
          var fieldInfo = typeInfo.Fields[fieldName];
          var columns = fieldInfo.ExtractColumns();

          if (columns.Count==0)
            throw new DomainBuilderException(
              string.Format(Strings.ExColumnXIsNotFound, fieldName));

          foreach (var column in columns)
            if (!result.KeyColumns.ContainsKey(column))
              result.ValueColumns.Add(column);
        }
        result.ValueColumns.AddRange(result.IncludedColumns.Where(ic => !result.ValueColumns.Contains(ic.Name)));
      }

      result.Name = context.NameBuilder.Build(typeInfo, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    private static IndexInfo BuildInheritedIndex(TypeInfo reflectedType, IndexInfo ancestorIndexInfo)
    {
      Log.Info(Strings.LogBuildingIndexX, ancestorIndexInfo.Name);
      var result = new IndexInfo(reflectedType, ancestorIndexInfo);
      var useFieldMap = ancestorIndexInfo.ReflectedType.IsInterface && !reflectedType.IsInterface;

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in ancestorIndexInfo.KeyColumns) {
        var field = useFieldMap ?
          reflectedType.FieldMap[pair.Key.Field] :
          reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      // Adding included columns
      foreach (var column in ancestorIndexInfo.IncludedColumns) {
        var field = useFieldMap ? 
          reflectedType.FieldMap[column.Field] : 
          reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      if (!ancestorIndexInfo.IsPrimary)
        foreach (var column in ancestorIndexInfo.ValueColumns) {
          var field = useFieldMap ?
            reflectedType.FieldMap[column.Field] :
            reflectedType.Fields[column.Field.Name];
          result.ValueColumns.Add(field.Column);
        }
      else if ((reflectedType.Attributes & TypeAttributes.Materialized) != 0)
        result.ValueColumns.AddRange(reflectedType.Columns.Find(ColumnAttributes.PrimaryKey, MatchType.None));

      if (ancestorIndexInfo.IsPrimary) {
        if (reflectedType.Hierarchy.Schema==InheritanceSchema.ClassTable) {
          foreach (var column in ancestorIndexInfo.IncludedColumns) {
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

      result.Name = BuildingContext.Current.NameBuilder.Build(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    private static IndexInfo BuildVirtualIndex(TypeInfo reflectedType, IndexAttributes indexAttributes,
      IndexInfo baseIndex,
      params IndexInfo[] baseIndexes)
    {
      Log.Info(Strings.LogBuildingIndexX, baseIndex.Name);
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var result = new IndexInfo(reflectedType, indexAttributes, baseIndex, baseIndexes);

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
      else if ((indexAttributes & IndexAttributes.Union) > 0)
      {
        var columns = new List<ColumnInfo>(baseIndex.ValueColumns);
        var columnsToAdd = baseIndexes.SelectMany(i => i.ValueColumns).ToList();
        foreach (var column in columnsToAdd)
        {
          bool exists = false;
          foreach (var existingColumn in columns)
            if (column.Field.Name == existingColumn.Field.Name && column.Field.DeclaringType == existingColumn.Field.DeclaringType)
            {
              exists = true;
              break;
            }
          if (!exists)
            columns.Add(column);
        }
        result.ValueColumns.AddRange(GetValueColumns(columns));
      }

      result.Name = nameBuilder.Build(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    #endregion

    #region Helper methods

    private static ColumnInfoCollection GetValueColumns(IEnumerable<ColumnInfo> columns)
    {
      var nameBuilder = BuildingContext.Current.NameBuilder;
      var valueColumns = new ColumnInfoCollection();
      bool assignAliases = false;
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
          if(doNotAdd)
            continue;
        }

        if (assignAliases || valueColumns.Contains(column.Name)) {
          if (column.IsSystem)
            continue;
          if (!assignAliases) {
            assignAliases = true;
            var addedValueColumns = new List<ColumnInfo>(valueColumns);
            valueColumns = new ColumnInfoCollection();
            foreach (ColumnInfo addedColumn in addedValueColumns) {
              if (!addedColumn.IsSystem) {
                var aliasedColumn = addedColumn.Clone();
                aliasedColumn.Name = nameBuilder.Build(aliasedColumn);
                valueColumns.Add(aliasedColumn);
              }
              else
                valueColumns.Add(addedColumn);
            }
          }
          var clone = column.Clone();
          clone.Name = nameBuilder.Build(column);
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
        keyColumns = new List<int>(index.ValueColumns.Take(reflectedType.Hierarchy.KeyInfo.Columns.Count).Select((_, i) => index.KeyColumns.Count + i));
        columns = new List<int>(index.KeyColumns.Select((_, i) => i));
        columns.AddRange(index.ValueColumns.Select((_, i) => index.KeyColumns.Count + i));
      }
      return new ColumnGroup(reflectedType.Hierarchy, keyColumns, columns);
    }

    public static void BuildAffectedIndexes()
    {
      var context = BuildingContext.Current;
      foreach (var typeInfo in context.Model.Types) {
        if (typeInfo.IsEntity) {
          var ancestors = new Dictionary<TypeInfo, string>();
          ProcessAncestors(typeInfo, ancestor => ancestors.Add(ancestor, string.Empty));

          var type = typeInfo;
          Action<IEnumerable<IndexInfo>> extractor = null;
          extractor = 
            delegate(IEnumerable<IndexInfo> source) {
              foreach (var indexInfo in source) {
                if (!indexInfo.IsVirtual) {
                  if ((ancestors.ContainsKey(indexInfo.ReflectedType) || indexInfo.ReflectedType == type) &&
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
