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
using Xtensive.Storage;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class IndexBuilder
  {
    public static void DefineIndexes(TypeDef typeDef)
    {
      using(Log.InfoRegion("Defining indexes.")) {

        var indexAttributes = typeDef.UnderlyingType.GetAttributes<IndexAttribute>(
          AttributeSearchOptions.Default);

        foreach (IndexAttribute attribute in indexAttributes)
          try {
            DefineIndex(typeDef, attribute);
          }
          catch (DomainBuilderException e) {
            BuildingContext.Current.RegisterError(e);
          }
      }
    }

    private static void DefineIndex(TypeDef typeDef, IndexAttribute attribute)
    {
      ArgumentValidator.EnsureArgumentNotNull(attribute, "attribute");      
      
      IndexDef index = new IndexDef();
      AttributeProcessor.Process(index, attribute);

      if (index.Name.IsNullOrEmpty() && index.KeyFields.Count > 0)
        index.Name = BuildingContext.Current.NameBuilder.Build(typeDef, index);

      if (typeDef.Indexes.Contains(index.Name))
        throw new DomainBuilderException(
          string.Format(Resources.Strings.IndexWithNameXIsAlreadyRegistered, index.Name));

      typeDef.Indexes.Add(index);
    }


    public static IndexDef DefineForeignKey(TypeDef type, FieldDef field)
    {
      var index = new IndexDef();
      index.KeyFields.Add(field.Name);
      index.Name = BuildingContext.Current.NameBuilder.Build(type, index);
      index.IsSecondary = true;
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
      BuildingContext context = BuildingContext.Current;
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface).Where(i => i.Hierarchy==hierarchy)) {
        TypeDef interfaceDef = context.Definition.Types[@interface.UnderlyingType];
        
        // Build virtual declared interface index
        foreach (IndexDef indexDescriptor in interfaceDef.Indexes)
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
      BuildingContext context = BuildingContext.Current;
      foreach (var @interface in context.Model.Types.Find(TypeAttributes.Interface).Where(i => i.Hierarchy == hierarchy)) {
        var implementors = new List<TypeInfo>(@interface.GetImplementors(false));

        if (implementors.Count==1)
          @interface.Indexes.Add(implementors[0].Indexes.PrimaryIndex);
        else {
          TypeDef rootDef = context.Definition.Types[hierarchy.Root.UnderlyingType];
          IndexDef primaryIndexDefinition = rootDef.Indexes.Where(i => i.IsPrimary).First();
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

      BuildingContext context = BuildingContext.Current;
      TypeDef typeDef = context.Definition.Types[type.UnderlyingType];

      IndexDef primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
      var indexDefinitions = typeDef.Indexes.Where(i => !i.IsPrimary).ToList();

      // Building primary index for root of the hierarchy
      if (primaryIndexDefinition != null)
        BuildHierarchyPrimaryIndex(type, primaryIndexDefinition);

      // Building declared indexes
      foreach (IndexDef indexDescriptor in indexDefinitions)
        BuildDeclaredIndex(type, indexDescriptor);

      // Building primary index for non root entities
      TypeInfo parent = type.GetAncestor();
      if (parent != null) {
        IndexInfo parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
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
      foreach (TypeInfo descendant in type.GetDescendants())
        BuildClassTableIndexes(descendant);

      // Import inherited indexes
      if (type.IsEntity) {
        var primaryIndex = type.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
        var ancestors = type.GetAncestors().ToList();
        // Build virtual primary index
        if (ancestors.Count > 0) {
          var baseIndexes = new List<IndexInfo>();
          foreach (TypeInfo ancestor in ancestors) {
            IndexInfo ancestorIndex = ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Real, MatchType.Full).First();
            IndexInfo baseIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorIndex);
            baseIndexes.Add(baseIndex);
          }
          baseIndexes.Add(primaryIndex);
          
          IndexInfo virtualPrimaryIndex = BuildVirtualIndex(type, IndexAttributes.Join, baseIndexes[0], baseIndexes.Skip(1).ToArray());
          virtualPrimaryIndex.IsPrimary = true;
          type.Indexes.Add(virtualPrimaryIndex);
        }

        // Build virtual secondary index
        foreach (TypeInfo ancestor in ancestors)
          foreach (IndexInfo ancestorSecondaryIndex in ancestor.Indexes.Find(IndexAttributes.Primary, MatchType.None)) {
            IndexInfo virtualSecondaryIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorSecondaryIndex);
            type.Indexes.Add(virtualSecondaryIndex);
          }
      }
    }

    private static void BuildDeclaredIndex(TypeInfo type, IndexDef indexDescriptor)
    {
      IndexInfo indexInfo = BuildIndex(type, indexDescriptor); 

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

      BuildingContext context = BuildingContext.Current;
      TypeDef typeDef = context.Definition.Types[type.UnderlyingType];
      TypeInfo root = type.Hierarchy.Root;

      IndexDef primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
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
      foreach (IndexDef indexDescriptor in indexDefinitions)
        using (var scope = new LogCaptureScope(context.Log)) {
          IndexInfo indexInfo = BuildIndex(type, indexDescriptor); 
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            type.Indexes.Add(indexInfo);
            context.Model.RealIndexes.Add(indexInfo);
          }
        }

      // Building primary index for non root entities
      TypeInfo parent = type.GetAncestor();
      if (parent != null) {
        IndexInfo parentPrimaryIndex = parent.Indexes.FindFirst(IndexAttributes.Primary | IndexAttributes.Real);
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
      foreach (TypeInfo descendant in type.GetDescendants())
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
        foreach (TypeInfo ancestor in ancestors)
          foreach (IndexInfo ancestorSecondaryIndex in ancestor.Indexes.Find(IndexAttributes.Primary | IndexAttributes.Virtual, MatchType.None)) {
            if (ancestorSecondaryIndex.DeclaringIndex == ancestorSecondaryIndex) {
              var secondaryIndex = BuildInheritedIndex(type, ancestorSecondaryIndex);
              type.Indexes.Add(secondaryIndex);
              context.Model.RealIndexes.Add(secondaryIndex);
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

      BuildingContext context = BuildingContext.Current;
      TypeDef typeDef = context.Definition.Types[type.UnderlyingType];
      TypeInfo root = type.Hierarchy.Root;

      IndexDef primaryIndexDefinition = typeDef.Indexes.Where(i => i.IsPrimary).FirstOrDefault();
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
      foreach (IndexDef indexDescriptor in indexDefinitions)
        using (var scope = new LogCaptureScope(context.Log)) {
          IndexInfo indexInfo = BuildIndex(type, indexDescriptor); 
          if (!scope.IsCaptured(LogEventTypes.Error)) {
            root.Indexes.Add(indexInfo);
            context.Model.RealIndexes.Add(indexInfo);
          }
        }

      TypeInfo parent = type.GetAncestor();
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
      foreach (TypeInfo descendant in type.GetDescendants())
        BuildSingleTableIndexes(descendant);

      if (type != root) {
        var ancestors = type.GetAncestors().ToList();
        ancestors.Add(type);
        foreach (var ancestorIndex in root.Indexes) {
          var interfaces = type.GetInterfaces(true);
          if ((ancestorIndex.DeclaringType.IsInterface && interfaces.Contains(ancestorIndex.DeclaringType)) || ancestors.Contains(ancestorIndex.DeclaringType)) {
            IndexInfo virtualIndex = BuildVirtualIndex(type, IndexAttributes.Filtered, ancestorIndex);
            if (!type.Indexes.Contains(virtualIndex.Name))
              type.Indexes.Add(virtualIndex);
          }
        }
      }
    }

    #endregion

    #region Build IndexInfo methods

    private static IndexInfo BuildIndex(TypeInfo typeInfo, IndexDef indexDef)
    {
      BuildingContext context = BuildingContext.Current;
      Log.Info("Building index '{0}'", indexDef.Name);
      var result = new IndexInfo(typeInfo, indexDef.Attributes);
      result.FillFactor = indexDef.FillFactor;
      result.ShortName = indexDef.Name;
      result.MappingName = indexDef.MappingName;

      Action<FieldInfo, IList<ColumnInfo>> columnsExtractor = null;
      columnsExtractor = delegate(FieldInfo field, IList<ColumnInfo> columns) {
        if (field.Column==null) {
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

        if (columns.Count==0)
          throw new DomainBuilderException(
            string.Format(Resources.Strings.ExColumnXIsNotFound, pair.Key));

        foreach (ColumnInfo column in columns)
          result.KeyColumns.Add(column, pair.Value);
      }

      // Adding included columns
      foreach (string fieldName in indexDef.IncludedFields) {
        FieldInfo fieldInfo = typeInfo.Fields[fieldName];
        IList<ColumnInfo> columns = new List<ColumnInfo>();
        columnsExtractor(fieldInfo, columns);

        if (columns.Count==0)
          throw new DomainBuilderException(
            string.Format(Resources.Strings.ExColumnXIsNotFound, fieldName));

        foreach (ColumnInfo column in columns)
          result.IncludedColumns.Add(column);
      }

      // Adding system columns as included (only if they are not primary key or index is not primary)
      foreach (ColumnInfo column in typeInfo.Columns.Find(ColumnAttributes.System).Where(c => indexDef.IsPrimary ? !c.IsPrimaryKey : true))
        result.IncludedColumns.Add(column);

      // Adding value columns
      if (indexDef.IsPrimary) {
        TypeInfo root = typeInfo.Hierarchy.Root;
        InheritanceSchema schema = typeInfo.Hierarchy.Schema;
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
        foreach (var fieldName in typeInfo.Hierarchy.Fields.Select(pair => pair.Key.Name)) {
          FieldInfo fieldInfo = typeInfo.Fields[fieldName];
          IList<ColumnInfo> columns = new List<ColumnInfo>();
          columnsExtractor(fieldInfo, columns);

          if (columns.Count==0)
            throw new DomainBuilderException(
              string.Format(Resources.Strings.ExColumnXIsNotFound, fieldName));

          foreach (ColumnInfo column in columns)
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

      if (ancestorIndexInfo.IsPrimary) {
        if (reflectedType.Hierarchy.Schema==InheritanceSchema.ClassTable) {
          foreach (ColumnInfo column in ancestorIndexInfo.IncludedColumns) {
            FieldInfo field = reflectedType.Fields[column.Field.Name];
            result.ValueColumns.Add(field.Column);
          }
          foreach (ColumnInfo column in reflectedType.Columns.Find(ColumnAttributes.Inherited | ColumnAttributes.PrimaryKey, MatchType.None))
            result.ValueColumns.Add(column);
        }
        else if (reflectedType.Hierarchy.Schema==InheritanceSchema.ConcreteTable) {
          foreach (ColumnInfo column in reflectedType.Columns.Find(ColumnAttributes.PrimaryKey, MatchType.None))
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
      Log.Info("Building index '{0}'", baseIndex.Name);
      NameBuilder nameBuilder = BuildingContext.Current.NameBuilder;
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

      result.Name = nameBuilder.Build(reflectedType, result);
      result.Group = BuildColumnGroup(result);

      return result;
    }

    #endregion

    #region Helper methods

    private static ColumnInfoCollection GetValueColumns(IEnumerable<ColumnInfo> columns)
    {
      NameBuilder nameBuilder = BuildingContext.Current.NameBuilder;
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
      var keyColumns = index.KeyColumns.Select(p => p.Key);
      return index.IsPrimary ? 
        new ColumnGroup(reflectedType.Hierarchy, keyColumns, keyColumns.Union(index.ValueColumns)) : 
        new ColumnGroup(reflectedType.Hierarchy, index.ValueColumns.Take(reflectedType.Hierarchy.Columns.Count), keyColumns.Union(index.ValueColumns));
    }

    public static void BuildAffectedIndexes()
    {
      BuildingContext context = BuildingContext.Current;
      foreach (TypeInfo typeInfo in context.Model.Types) {
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
                extractor(indexInfo.UnderlyingIndexes);
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
      BuildingContext context = BuildingContext.Current;
      TypeInfo root = typeInfo.Hierarchy.Root;

      if (root != typeInfo) {
        TypeInfo ancestorTypeInfo = context.Model.Types.FindAncestor(typeInfo);
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
