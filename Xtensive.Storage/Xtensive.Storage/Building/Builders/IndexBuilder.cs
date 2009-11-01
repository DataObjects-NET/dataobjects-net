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
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Building.Builders
{
  internal static class IndexBuilder
  {
    public static void DefineIndexes(TypeDef typeDef)
    {
      BuildingContext buildingContext = BuildingScope.Context;
      Log.Info("Defining indexes.");
      var indexes = new List<IndexDef>();

      var indexAttributes = typeDef.UnderlyingType.GetAttributes<IndexAttribute>(false);
      foreach (IndexAttribute attribute in indexAttributes) {
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          ValidationResult vr = Validator.ValidateAttribute(typeDef, attribute);
          if (!vr.Success)
            Log.Error(vr.Message);
          else {
            var index = new IndexDef();
            AttributeProcessor.Process(index, attribute);
            if (string.IsNullOrEmpty(index.Name) && index.KeyFields.Count > 0)
              index.Name = BuildingScope.Context.NameProvider.BuildName(typeDef, index);
            if (!scope.IsCaptured(LogEventTypes.Error))
              indexes.Add(index);
          }
        }
      }

      foreach (IndexDef indexDef in indexes)
        if (typeDef.Indexes.Contains(indexDef.Name))
          Log.Error("Index with name '{0}' is already registered.", indexDef.Name);
        else
          typeDef.Indexes.Add(indexDef);
    }

    public static IndexDef DefineForeignKey(TypeDef type, FieldDef field)
    {
      var index = new IndexDef();
      index.KeyFields.Add(field.Name);
      index.Name = BuildingScope.Context.NameProvider.BuildName(type, index);
      index.IsForeignKey = true;
      return index;
    }

    public static void BuildIndexes()
    {
      BuildingContext buildingContext = BuildingScope.Context;
      foreach (HierarchyInfo hierarchy in buildingContext.Model.Hierarchies) {
        CreateInterfaceIndexes(hierarchy);
        switch(hierarchy.Schema) {
        case InheritanceSchema.Default:
          BuildClassTableIndexes(hierarchy.Root);
          break;
        case InheritanceSchema.SingleTableInheritance:
          BuildSingleTableIndexes(hierarchy.Root);
          break;
        case InheritanceSchema.ConcreteTableInheritance:
          BuildConcreteTableIndexes(hierarchy.Root);
          break;
        }
        BuildInterfaceIndexes(hierarchy);
      }
    }

    #region Interface support methods

    private static void CreateInterfaceIndexes(HierarchyInfo hierarchy)
    {
      BuildingContext buildingContext = BuildingScope.Context;
      TypeDef rootDef = buildingContext.Definition.Types[hierarchy.Root.UnderlyingType];
      foreach (var @interface in buildingContext.Model.Types.Find(TypeAttributes.Interface).Where(i => i.Hierarchy == hierarchy)) {
        TypeDef interfaceDef = buildingContext.Definition.Types[@interface.UnderlyingType];
        IndexDef primaryIndexDefinition = rootDef.Indexes.Where(i => i.IsPrimary).First();

        // Build virtual primary interface index
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          var index = BuildIndex(@interface, primaryIndexDefinition);
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            @interface.Indexes.Add(index);
            if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
              buildingContext.Model.RealIndexes.Add(index);
          }
        }
        
        // Build virtual declared interface index
        foreach (IndexDef indexDescriptor in interfaceDef.Indexes)
          using (var scope = new LogCaptureScope(buildingContext.Logger)) {
            var index = BuildIndex(@interface, indexDescriptor);
            if (!scope.IsCaptured(LogEventTypes.Error)) {
              @interface.Indexes.Add(index);
              if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
                buildingContext.Model.RealIndexes.Add(index);
            }
          }
        // Build virtual inherited interface index
        foreach (var parent in @interface.GetInterfaces()) {
          foreach (var parentIndex in parent.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
            using (var scope = new LogCaptureScope(buildingContext.Logger)) {
              var index = BuildInheritedIndex(@interface, parentIndex);
              if (!scope.IsCaptured(LogEventTypes.Error)) {
                @interface.Indexes.Add(index);
                if ((@interface.Attributes & TypeAttributes.Materialized) != 0)
                  buildingContext.Model.RealIndexes.Add(index);
              }
            }
          }
        }
      }
    }

    private static void BuildInterfaceIndexes(HierarchyInfo hierarchy)
    {
      BuildingContext buildingContext = BuildingScope.Context;
      foreach (var @interface in buildingContext.Model.Types.Find(TypeAttributes.Interface).Where(i => i.Hierarchy == hierarchy)) {
        var implementors = new List<TypeInfo>(@interface.GetImplementors(false));
        
        foreach (var index in @interface.Indexes) {
          if (index.IsPrimary) {
            var implementorsIndexes = implementors.Select(t => t.Indexes.PrimaryIndex);
            if (index.IsVirtual)
              index.BaseIndexes.AddRange(implementorsIndexes);
          }
          else if (index.IsVirtual) {
            var baseIndexes = new List<IndexInfo>();
            foreach (var implementor in implementors) {
              foreach (var implementedIndex in implementor.Indexes) {
                if (implementedIndex.KeyColumns.Count==index.KeyColumns.Count) {
                  bool match = true;
                  for (int i = 0; i < index.KeyColumns.Count; i++) {
                    if (implementedIndex.KeyColumns[i].Value != index.KeyColumns[i].Value) {
                      match = false;
                      break;
                    }
                    FieldInfo field = implementor.FieldMap[index.KeyColumns[i].Key.Field];
                    if (field != implementedIndex.KeyColumns[i].Key.Field) {
                      match = false;
                      break;                       
                    }
                  }
                  if (match) {
                    baseIndexes.Add(implementedIndex);
                    break;
                  }
                }
              }
            }
            index.BaseIndexes.AddRange(baseIndexes);
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

      BuildingContext buildingContext = BuildingScope.Context;
      TypeDef typeDef = buildingContext.Definition.Types[type.UnderlyingType];
      TypeInfo root = type.Hierarchy.Root;

      IndexDef primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
      var indexDefinitions = typeDef.Indexes.Where(i => !i.IsPrimary).ToList();

      //Building primary index for root of the hierarchy
      if (primaryIndexDefinition != null)
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          var primaryIndex = BuildIndex(root, primaryIndexDefinition);
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            type.Indexes.Add(primaryIndex);
            buildingContext.Model.RealIndexes.Add(primaryIndex);
          }
        }

      //Building declared indexes
      foreach (IndexDef indexDescriptor in indexDefinitions)
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          IndexInfo indexInfo = BuildIndex(type, indexDescriptor); 
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            type.Indexes.Add(indexInfo);
            buildingContext.Model.RealIndexes.Add(indexInfo);
          }
        }

      //Building primary index for non root entities
      TypeInfo parent = type.GetAncestor();
      if (parent != null) {
        IndexInfo parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var primaryIndex = BuildInheritedIndex(type, parentPrimaryIndex);
        foreach (ColumnInfo column in parentPrimaryIndex.IncludedColumns) {
          FieldInfo field = type.Fields[column.Field.Name];
          primaryIndex.ValueColumns.Add(field.Column);
        }
        foreach (ColumnInfo column in type.Columns.Find(ColumnAttributes.Inherited | ColumnAttributes.PrimaryKey, MatchType.None))
          primaryIndex.ValueColumns.Add(column);

        //Registering built primary index
        type.Indexes.Add(primaryIndex);
        buildingContext.Model.RealIndexes.Add(primaryIndex);
      }

      //Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {

          if (parentIndex.DeclaringIndex == parentIndex)
            using (var scope = new LogCaptureScope(buildingContext.Logger)) {
              var index = BuildInheritedIndex(type, parentIndex);
              //TODO: AK: discover this check
              if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
                continue;
              if (!scope.IsCaptured(LogEventTypes.Error)) {
                type.Indexes.Add(index);
                buildingContext.Model.RealIndexes.Add(index);
              }
            }
        }
      }
      
      //Build indexes for descendants
      foreach (TypeInfo descendant in type.GetDescendants())
        BuildClassTableIndexes(descendant);


      //Process inherited indexes
      if (type.IsEntity) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var ancestors = type.GetAncestors().ToList();
        var descendants = type.GetDescendants(true).ToList();
        // Build virtual primary index
        if (ancestors.Count > 0 || descendants.Count > 0) {
          var baseIndexes = new List<IndexInfo>();
          foreach (TypeInfo ancestor in ancestors) {
            IndexInfo ancestorIndex = ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Real, MatchType.Full).First();
            IndexInfo baseIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorIndex);
            baseIndexes.Add(baseIndex);
          }
          baseIndexes.Add(primaryIndex);
          foreach (TypeInfo descendant in descendants) {
            IndexInfo descendantIndex = descendant.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Real, MatchType.Full).First();
            baseIndexes.Add(descendantIndex);
          }
          IndexInfo virtualPrimaryIndex = BuildVirtualIndex(type, IndexAttributes.Join, baseIndexes[0], baseIndexes.Skip(1).ToArray());
          type.Indexes.Add(virtualPrimaryIndex);
        }

        //Build virtual secondary index
        foreach (TypeInfo ancestor in ancestors)
          foreach (IndexInfo ancestorSecondaryIndex in ancestor.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
            IndexInfo virtualSecondaryIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorSecondaryIndex);
            type.Indexes.Add(virtualSecondaryIndex);
          }
      }
    }

    private static void BuildConcreteTableIndexes(TypeInfo type)
    {
      if (type.Indexes.Count > 0)
        return;
      
      if (type.IsStructure)
        return;

      BuildingContext buildingContext = BuildingScope.Context;
      TypeDef typeDef = buildingContext.Definition.Types[type.UnderlyingType];
      TypeInfo root = type.Hierarchy.Root;

      IndexDef primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
      var indexDefinitions = typeDef.Indexes.Where(i => !i.IsPrimary).ToList();

      //Building primary index for root of the hierarchy
      if (primaryIndexDefinition != null)
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          var primaryIndex = BuildIndex(root, primaryIndexDefinition);
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            type.Indexes.Add(primaryIndex);
            buildingContext.Model.RealIndexes.Add(primaryIndex);
          }
        }

      //Building declared indexes
      foreach (IndexDef indexDescriptor in indexDefinitions)
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          IndexInfo indexInfo = BuildIndex(type, indexDescriptor); 
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            type.Indexes.Add(indexInfo);
            buildingContext.Model.RealIndexes.Add(indexInfo);
          }
        }

      //Building primary index for non root entities
      TypeInfo parent = type.GetAncestor();
      if (parent != null) {
        IndexInfo parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var primaryIndex = BuildInheritedIndex(type, parentPrimaryIndex);
        foreach (ColumnInfo column in type.Columns.Find(ColumnAttributes.PrimaryKey, MatchType.None))
          if (!primaryIndex.ValueColumns.Contains(column.Name))
            primaryIndex.ValueColumns.Add(column);

        //Registering built primary index
        type.Indexes.Add(primaryIndex);
        buildingContext.Model.RealIndexes.Add(primaryIndex);
      }

      //Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {

          if (parentIndex.DeclaringIndex == parentIndex)
            using (var scope = new LogCaptureScope(buildingContext.Logger)) {
              var index = BuildInheritedIndex(type, parentIndex);
              //TODO: AK: discover this check
              if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
                continue;
              if (!scope.IsCaptured(LogEventTypes.Error)) {
                type.Indexes.Add(index);
                buildingContext.Model.RealIndexes.Add(index);
              }
            }
        }
      }

      //Build indexes for descendants
      foreach (TypeInfo descendant in type.GetDescendants())
        BuildConcreteTableIndexes(descendant);

      //Process inherited indexes
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
        foreach (TypeInfo ancestor in ancestors)
          foreach (IndexInfo ancestorSecondaryIndex in ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None)) {
            if (ancestorSecondaryIndex.DeclaringIndex == ancestorSecondaryIndex) {
              var secondaryIndex = BuildInheritedIndex(type, ancestorSecondaryIndex);
              type.Indexes.Add(secondaryIndex);
              buildingContext.Model.RealIndexes.Add(secondaryIndex);
            }
          }

        // Build virtual secondary indexes
        if (descendants.Count > 0)
          foreach (var index in type.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None)) {
            IndexInfo secondaryIndex = index;
            var baseIndexes = descendants.SelectMany(t => t.Indexes.Where(i => i.DeclaringIndex==secondaryIndex.DeclaringIndex)).ToList();
            IndexInfo virtualSecondaryIndex = BuildVirtualIndex(type, IndexAttributes.Union, index, baseIndexes.ToArray());
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

      BuildingContext buildingContext = BuildingScope.Context;
      TypeDef typeDef = buildingContext.Definition.Types[type.UnderlyingType];
      TypeInfo root = type.Hierarchy.Root;

      IndexDef primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
      var indexDefinitions = typeDef.Indexes.Where(i => !i.IsPrimary).ToList();

      //Building primary index for root of the hierarchy
      if (primaryIndexDefinition != null)
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          var primaryIndex = BuildIndex(root, primaryIndexDefinition);
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            root.Indexes.Add(primaryIndex);
            buildingContext.Model.RealIndexes.Add(primaryIndex);
          }
        }

      //Building declared indexes
      foreach (IndexDef indexDescriptor in indexDefinitions)
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          IndexInfo indexInfo = BuildIndex(type, indexDescriptor); 
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            root.Indexes.Add(indexInfo);
            buildingContext.Model.RealIndexes.Add(indexInfo);
          }
        }

      TypeInfo parent = type.GetAncestor();
      //Building inherited from interfaces indexes
      foreach (var @interface in type.GetInterfaces(true)) {
        foreach (var parentIndex in @interface.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
          if (parentIndex.DeclaringIndex == parentIndex)
            using (var scope = new LogCaptureScope(buildingContext.Logger)) {
              var index = BuildInheritedIndex(type, parentIndex);
              //TODO: AK: discover this check
              if ((parent != null && parent.Indexes.Contains(index.Name)) || type.Indexes.Contains(index.Name))
                continue;
              if (!scope.IsCaptured(LogEventTypes.Error)) {
                root.Indexes.Add(index);
                buildingContext.Model.RealIndexes.Add(index);
              }
            }
        }
      }
      
      //Build indexes for descendants
      foreach (TypeInfo descendant in type.GetDescendants())
        BuildSingleTableIndexes(descendant);

      if (type != root) {
        var ancestors = type.GetAncestors().ToList();
        ancestors.Add(type);
        foreach (var ancestorIndex in root.Indexes) {
          if (!ancestorIndex.DeclaringType.IsInterface && ancestors.Contains(ancestorIndex.DeclaringType)) {
            IndexInfo virtualIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorIndex);
            type.Indexes.Add(virtualIndex);
          }
        }
      }
    }

    #endregion
    
    #region Build IndexInfo methods

    private static IndexInfo BuildIndex(TypeInfo typeInfo, IndexDef indexDef)
    {
      BuildingContext buildingContext = BuildingScope.Context;
      Log.Info("Building index '{0}'", indexDef.Name);
      var result = new IndexInfo(typeInfo, indexDef.Attributes);
      result.FillFactor = indexDef.FillFactor;
      result.ShortName = indexDef.Name;
      result.MappingName = indexDef.MappingName;

      Action<FieldInfo, IList<ColumnInfo>> columnsExtractor = null;
      columnsExtractor = delegate(FieldInfo field, IList<ColumnInfo> columns) {
        if (field.Column == null) {
          if (field.IsEntity)
            foreach (FieldInfo childField in field.Fields)
              columnsExtractor(childField, columns);
        }
        else
          columns.Add(field.Column);
      };
      

      // Adding key columns
      foreach (KeyValuePair<string, Direction> pair in indexDef.KeyFields) {
        FieldInfo fieldInfo = typeInfo.Fields[pair.Key];
        IList<ColumnInfo> columns = new List<ColumnInfo>();
        columnsExtractor(fieldInfo, columns);

        if (columns.Count == 0) {
          Log.Error("Column '{0}' is not found.", pair.Key);
          continue;
        }
        foreach (ColumnInfo column in columns)
          result.KeyColumns.Add(column, pair.Value);
      }

      // Adding included columns
      foreach (string fieldName in indexDef.IncludedFields) {
        FieldInfo fieldInfo = typeInfo.Fields[fieldName];
        IList<ColumnInfo> columns = new List<ColumnInfo>();
        columnsExtractor(fieldInfo, columns);

        if (columns.Count == 0) {
          Log.Error("Column '{0}' is not found.", fieldName);
          continue;
        }
        foreach (ColumnInfo column in columns)
          result.IncludedColumns.Add(column);
      }

      // Adding system columns as included (only if they are not primary key)
      foreach (ColumnInfo column in typeInfo.Columns.Find(ColumnAttributes.System).Where(c => !c.IsPrimaryKey))
        result.IncludedColumns.Add(column);

      // Adding value columns
      if (indexDef.IsPrimary) {
        TypeInfo root = typeInfo.Hierarchy.Root;
        InheritanceSchema schema = typeInfo.Hierarchy.Schema;
        IEnumerable<TypeInfo> types;
        if (typeInfo.IsInterface)
          types = typeInfo.GetImplementors(true);
        else
          switch (schema) {
          case InheritanceSchema.SingleTableInheritance:
            types = new[] {typeInfo}.Union(root.GetDescendants(true));
            break;
          case InheritanceSchema.ConcreteTableInheritance:
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
        foreach (var fieldName in typeInfo.Hierarchy.Fields.Select(pair => pair.Key.Name)) {
          FieldInfo fieldInfo = typeInfo.Fields[fieldName];
          IList<ColumnInfo> columns = new List<ColumnInfo>();
          columnsExtractor(fieldInfo, columns);

          if (columns.Count == 0) {
            Log.Error("Field '{0}' is not found.", fieldName);
            continue;
          }
          foreach (ColumnInfo column in columns)
            if (!result.KeyColumns.ContainsKey(column))
              result.ValueColumns.Add(column);
        }
        result.ValueColumns.AddRange(result.IncludedColumns);
      }

      result.Name = buildingContext.NameProvider.BuildName(typeInfo, result);
      
      return result;
    }

    private static IndexInfo BuildInheritedIndex(TypeInfo reflectedType, IndexInfo ancestorIndexInfo)
    {
      Log.Info("Building index '{0}'", ancestorIndexInfo.Name);
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
      foreach (ColumnInfo column in ancestorIndexInfo.IncludedColumns) {
        FieldInfo field = useFieldMap ? 
          reflectedType.FieldMap[column.Field] : 
          reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      if (!ancestorIndexInfo.IsPrimary)
        foreach (ColumnInfo column in ancestorIndexInfo.ValueColumns) {
          FieldInfo field = useFieldMap ?
            reflectedType.FieldMap[column.Field] :
            reflectedType.Fields[column.Field.Name];
          result.ValueColumns.Add(field.Column);
        }
      else if ((reflectedType.Attributes & TypeAttributes.Materialized) != 0)
        result.ValueColumns.AddRange(reflectedType.Columns.Find(ColumnAttributes.PrimaryKey, MatchType.None));

      result.Name = BuildingScope.Context.NameProvider.BuildName(reflectedType, result);
      return result;
    }

    private static IndexInfo BuildVirtualIndex(TypeInfo reflectedType, IndexAttributes indexAttributes,
      IndexInfo baseIndex,
      params IndexInfo[] baseIndexes)
    {
      Log.Info("Building index '{0}'", baseIndex.Name);
      NameProvider nameProvider = BuildingScope.Context.NameProvider;
      IndexInfo result = new IndexInfo(reflectedType, indexAttributes, baseIndex, baseIndexes);

      var allBaseIndexes = new List<IndexInfo>();
      allBaseIndexes.Add(baseIndex);
      allBaseIndexes.AddRange(baseIndexes);

      // Adding key columns
      foreach (KeyValuePair<ColumnInfo, Direction> pair in baseIndex.KeyColumns) {
        FieldInfo field = reflectedType.Fields[pair.Key.Field.Name];
        result.KeyColumns.Add(field.Column, pair.Value);
      }

      var useFieldMap = baseIndex.ReflectedType.IsInterface && !reflectedType.IsInterface;

      // Adding included columns
      foreach (ColumnInfo column in baseIndex.IncludedColumns) {
        FieldInfo field = useFieldMap ?
          reflectedType.FieldMap[column.Field] :
          reflectedType.Fields[column.Field.Name];
        result.IncludedColumns.Add(field.Column);
      }

      // Adding value columns
      if ((indexAttributes & IndexAttributes.Join) > 0)
        result.ValueColumns.AddRange(GetValueColumns(allBaseIndexes.SelectMany(i => i.ValueColumns)));
      else if ((indexAttributes & IndexAttributes.Filtered) > 0) {
        var types = reflectedType.GetAncestors();
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

      result.Name = nameProvider.BuildName(reflectedType, result);
      return result;
    }

    #endregion

    #region Helper methods

    private static ColumnInfoCollection GetValueColumns(IEnumerable<ColumnInfo> columns)
    {
      NameProvider nameProvider = BuildingScope.Context.NameProvider;
      var valueColumns = new ColumnInfoCollection();
      bool assignAliases = false;
      foreach (ColumnInfo column in columns)  {
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
                aliasedColumn.Name = nameProvider.BuildName(aliasedColumn);
                valueColumns.Add(aliasedColumn);
              }
              else
                valueColumns.Add(addedColumn);
            }
          }
          var clone = column.Clone();
          clone.Name = nameProvider.BuildName(column);
          valueColumns.Add(clone);
        }
        else
          valueColumns.Add(column);
      }
      return valueColumns;
    }

    public static void BuildAffectedIndexes()
    {
      BuildingContext buildingContext = BuildingScope.Context;
      foreach (TypeInfo typeInfo in buildingContext.Model.Types) {
        if (typeInfo.IsEntity) {
          var ancestors = new Dictionary<TypeInfo, string>();
          ProcessAncestors(typeInfo, ancestor => ancestors.Add(ancestor, string.Empty));

          TypeInfo type = typeInfo;
          Action<IEnumerable<IndexInfo>> extractor = null;
          extractor = 
            delegate(IEnumerable<IndexInfo> source) {
              foreach (IndexInfo indexInfo in source) {
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
                extractor(indexInfo.BaseIndexes);
              }
            };

          extractor(typeInfo.Indexes);
        }
        else if ((typeInfo.Attributes & TypeAttributes.Materialized) != 0) {
          IndexInfo primaryIndex = typeInfo.Indexes.PrimaryIndex;
          foreach (TypeInfo descendant in typeInfo.GetDescendants(true).Where(t => t.IsEntity).Distinct()) {
            descendant.AffectedIndexes.Add(primaryIndex);
            foreach (IndexInfo indexInfo in typeInfo.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
              IndexInfo descendantIndex = descendant.Indexes.Where(i => i.DeclaringIndex == indexInfo.DeclaringIndex).FirstOrDefault();
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
      BuildingContext buildingContext = BuildingScope.Context;
      TypeInfo root = typeInfo.Hierarchy.Root;

      if (root != typeInfo) {
        TypeInfo ancestorTypeInfo = buildingContext.Model.Types.FindAncestor(typeInfo);
        if (ancestorTypeInfo != null)
          do {
            ancestorProcessor(ancestorTypeInfo);
            if (ancestorTypeInfo == root)
              break;
            ancestorTypeInfo = buildingContext.Model.Types.FindAncestor(ancestorTypeInfo);
          } while (ancestorTypeInfo != null);
      }
    }

    #endregion

  }
}
