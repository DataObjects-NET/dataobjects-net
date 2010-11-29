// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.02

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Sorting;
using Xtensive.Modelling;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;
using FullTextIndexInfo = Xtensive.Storage.Model.FullTextIndexInfo;
using IndexInfo = Xtensive.Storage.Model.IndexInfo;
using IndexingModel = Xtensive.Storage.Indexing.Model;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;
using SequenceInfo = Xtensive.Storage.Model.SequenceInfo;
using TypeInfo = Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Converts <see cref="Storage.Model.DomainModel"/> to indexing storage model.
  /// </summary>
  internal sealed class DomainModelConverter : ModelVisitor<IPathNode>
  {
    /// <summary>
    /// Gets the persistent generator filter.
    /// </summary>
    private Func<KeyInfo, KeyGenerator> GeneratorResolver { get; set; }

    /// <summary>
    /// Gets the provider info.
    /// </summary>
    private ProviderInfo ProviderInfo { get; set; }

    /// <summary>
    /// Gets the type builder.
    /// </summary>
    private Func<Type, int?, int?, int?, IndexingModel.TypeInfo> TypeBuilder { get; set; }

    /// <summary>
    /// Gets the storage info.
    /// </summary>
    private IndexingModel.StorageInfo StorageInfo { get; set; }

    /// <summary>
    /// Gets the currently converting model.
    /// </summary>
    private DomainModel Model { get; set; }

    /// <summary>
    /// Gets a value indicating whether 
    /// build foreign keys for associations.
    /// </summary>
    private bool BuildForeignKeys { get; set; }

    /// <summary>
    /// Gets the foreign key name generator.
    /// </summary>
    private Func<AssociationInfo, FieldInfo, string> ForeignKeyNameGenerator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 
    /// build foreign keys for hierarchies.
    /// </summary>
    private bool BuildHierarchyForeignKeys { get; set; }

    /// <summary>
    /// Gets the hierarchy foreign key name generator.
    /// </summary>
    private Func<Model.TypeInfo, Model.TypeInfo, string> HierarchyForeignKeyNameGenerator { get; set; }

    /// <summary>
    /// Gets or sets the currently visiting table.
    /// </summary>
    private IndexingModel.TableInfo CurrentTable { get; set; }

    /// <summary>
    /// Converts the specified <see cref="Storage.Model.DomainModel"/> to
    /// <see cref="StorageInfo"/>.
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <returns>The storage model.</returns>
    public IndexingModel.StorageInfo Convert(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");

      StorageInfo = new IndexingModel.StorageInfo();
      Model = domainModel;
      Visit(domainModel);
      return StorageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode Visit(Model.Node node)
    {
      var indexInfo = node as IndexInfo;
      if (indexInfo!=null && indexInfo.IsPrimary)
        return VisitPrimaryIndexInfo(indexInfo);

      return base.Visit(node);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitDomainModel(DomainModel domainModel)
    {
      // Build tables, columns and primary indexes
      foreach (var primaryIndex in domainModel.RealIndexes.Where(i => i.IsPrimary))
        Visit(primaryIndex);

      // Build full-text indexes
      foreach (var fullTextIndex in domainModel.FullTextIndexes)
        Visit(fullTextIndex);

      // Build foreign keys
      if (BuildForeignKeys && ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints)) {
        foreach (var group in domainModel.Associations.GroupBy(a => a.OwnerField.DeclaringField)) {
          var associations = group.ToList();
          if (associations.Count == 1)
            Visit(associations.Single());
          else {
            var mostCommonAssociation = associations.Reorder().Last();
            Visit(mostCommonAssociation);
          }
        }
      }

      // Build keys and sequences
      foreach (KeyInfo keyInfo in domainModel.Hierarchies.Select(h => h.Key))
        Visit(keyInfo);

      if (!BuildHierarchyForeignKeys || !ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints))
        return StorageInfo;

      // Build hierarchy foreign keys
      var indexPairs = new Dictionary<Pair<IndexInfo>, object>();
      foreach (TypeInfo type in domainModel.Types.Entities) {
        if (type.Hierarchy==null || type.Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable)
          continue;
        if (type.Indexes.PrimaryIndex.IsVirtual) {
          Dictionary<TypeInfo, int> typeOrder = type.GetAncestors()
            .AddOne(type)
            .Select((t, i) => new {Type = t, Index = i})
            .ToDictionary(a => a.Type, a => a.Index);
          List<IndexInfo> realPrimaryIndexes = type.Indexes.RealPrimaryIndexes
            .OrderBy(index => typeOrder[index.ReflectedType])
            .ToList();
          for (int i = 0; i < realPrimaryIndexes.Count - 1; i++) {
            if (realPrimaryIndexes[i]!=realPrimaryIndexes[i + 1]) {
              var pair = new Pair<IndexInfo>(realPrimaryIndexes[i], realPrimaryIndexes[i + 1]);
              indexPairs[pair] = null;
            }
          }
        }
      }
      foreach (var indexPair in indexPairs.Keys) {
        var referencedIndex = indexPair.First;
        var referencingIndex = indexPair.Second;
        var referencingTable = StorageInfo.Tables[referencingIndex.ReflectedType.MappingName];
        var referencedTable = StorageInfo.Tables[referencedIndex.ReflectedType.MappingName];
        var storageReferencingIndex = FindIndex(referencingTable,
          new List<string>(referencingIndex.KeyColumns.Select(ci => ci.Key.Name)));

        string foreignKeyName = HierarchyForeignKeyNameGenerator.Invoke(referencingIndex.ReflectedType, referencedIndex.ReflectedType);
        CreateForeignKey(referencingTable, foreignKeyName, referencedTable, storageReferencingIndex);
      }

      return StorageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndexInfo(IndexInfo index)
    {
      IndexingModel.TableInfo table = CurrentTable;
      IndexInfo primaryIndex = Model.RealIndexes.First(i => i.MappingName==table.PrimaryIndex.Name);
      var secondaryIndex = new IndexingModel.SecondaryIndexInfo(table, index.MappingName) {
        IsUnique = index.IsUnique
      };
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns) {
        string columName = GetPrimaryIndexColumnName(primaryIndex, pair.Key, index);
        IndexingModel.ColumnInfo column = table.Columns[columName];
        new IndexingModel.KeyColumnRef(secondaryIndex, column,
          ProviderInfo.Supports(ProviderFeatures.KeyColumnSortOrder)
            ? pair.Value
            : Direction.Positive);
      }
      if (ProviderInfo.Supports(ProviderFeatures.IncludedColumns)) {
        foreach (var includedColumn in index.IncludedColumns) {
          string columName = GetPrimaryIndexColumnName(primaryIndex, includedColumn, index);
          IndexingModel.ColumnInfo column = table.Columns[columName];
          new IndexingModel.IncludedColumnRef(secondaryIndex, column);
        }
      }
      secondaryIndex.PopulatePrimaryKeyColumns();
      return secondaryIndex;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitColumnInfo(ColumnInfo column)
    {
      var nonNullableType = column.ValueType;
      var nullableType    = ToNullable(nonNullableType, column.IsNullable);

      var typeInfoPrototype = new IndexingModel.TypeInfo(nullableType, column.IsNullable, 
        column.Length, column.Scale, column.Precision, null);
      var nativeTypeInfo = TypeBuilder.Invoke(nonNullableType, column.Length, column.Precision, column.Scale);

      // We need the same type as in SQL database here (i.e. the same as native)
      var typeInfo = new IndexingModel.TypeInfo(ToNullable(nativeTypeInfo.Type, column.IsNullable), column.IsNullable, 
        nativeTypeInfo.Length, nativeTypeInfo.Scale, nativeTypeInfo.Precision, nativeTypeInfo.NativeType);

      var defaultValue = GetColumnDefaultValue(column, typeInfo);
      if (column.IsSystem && column.Field.IsTypeId) {
        var type = column.Field.ReflectedType;
        if (type.IsEntity && type==type.Hierarchy.Root) {
          var buildingContext = BuildingContext.Demand();
          defaultValue = buildingContext.BuilderConfiguration.TypeIdProvider(type.UnderlyingType);
        }
      }

      return new IndexingModel.ColumnInfo(CurrentTable, column.Name, typeInfo) {
        DefaultValue = defaultValue
      };
    }

    /// <inheritdoc/>
    protected override IPathNode VisitAssociationInfo(AssociationInfo association)
    {
      if (association.TargetType.Hierarchy==null)
        return null;
      if (association.OwnerType.Hierarchy==null)
        return null;
      if (association.TargetType.Hierarchy.InheritanceSchema==InheritanceSchema.ConcreteTable && !association.TargetType.IsLeaf)
        return null;
      if (association.OwnerType.Indexes.PrimaryIndex==null)
        return null;
//      if (association.OnTargetRemove.HasValue && association.OnTargetRemove.Value == OnRemoveAction.None)
//        return null;
      if (association.OnTargetRemove==OnRemoveAction.None)
        return null;

      // Auxiliary == null
      if (association.AuxiliaryType==null) {
        if (association.OwnerField.Columns.Count==0)
          return null;

        IndexingModel.TableInfo referencingTable = GetTable(association.OwnerField.DeclaringType);
        IndexingModel.TableInfo referencedTable = GetTable(association.TargetType);
        if (referencedTable==null || referencingTable==null)
          return null;
        var foreignColumns = association.OwnerField.Columns
          .Select(ci => referencingTable.Columns[ci.Name]).ToList();
        string foreignKeyName = ForeignKeyNameGenerator.Invoke(association, association.OwnerField);
        var foreignKey = new IndexingModel.ForeignKeyInfo(referencingTable, foreignKeyName) {
          PrimaryKey = referencedTable.PrimaryIndex,
          OnRemoveAction = ReferentialAction.None,
          OnUpdateAction = ReferentialAction.None
        };
        foreach (var foreignColumn in foreignColumns)
          new IndexingModel.ForeignKeyColumnRef(foreignKey, foreignColumn);

        return foreignKey;
      }

      // Auxiliary != null
      if (association.AuxiliaryType!=null) {
        if (!association.IsMaster)
          return null;
        IndexingModel.TableInfo referencingTable = GetTable(association.AuxiliaryType);
        foreach (FieldInfo field in association.AuxiliaryType.Fields.Where(fieldInfo => fieldInfo.IsEntity)) {
          IndexingModel.TableInfo referencedTable = GetTable(Model.Types[field.ValueType]);
          if (referencedTable==null || referencingTable==null)
            continue;
          var foreignColumns = field.Columns.Select(ci => referencingTable.Columns[ci.Name]).ToList();
          string foreignKeyName = ForeignKeyNameGenerator.Invoke(association, field);
          var foreignKey = new IndexingModel.ForeignKeyInfo(referencingTable, foreignKeyName) {
            PrimaryKey = referencedTable.PrimaryIndex,
            OnRemoveAction = ReferentialAction.None,
            OnUpdateAction = ReferentialAction.None
          };
          foreach (var foreignColumn in foreignColumns)
            new IndexingModel.ForeignKeyColumnRef(foreignKey, foreignColumn);
        }
      }
      return null;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitKeyInfo(KeyInfo keyInfo)
    {
      if (keyInfo.Sequence==null || !keyInfo.IsFirstAmongSimilarKeys)
        return null;
      var sequenceInfo = keyInfo.Sequence;
      var sequence = new IndexingModel.SequenceInfo(StorageInfo, sequenceInfo.MappingName) {
        Seed = sequenceInfo.Seed,
        Increment = sequenceInfo.Increment,
        Type = TypeBuilder.Invoke(keyInfo.TupleDescriptor[0], null, null, null),
      };
      return sequence;
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Thrown always by this method.</exception>
    protected override IPathNode VisitSequenceInfo(SequenceInfo sequenceInfo)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override IPathNode VisitFullTextIndexInfo(FullTextIndexInfo fullTextIndex)
    {
      var table = GetTable(fullTextIndex.PrimaryIndex.ReflectedType);
      var primaryIndex = table.PrimaryIndex;
      var ftIndex = new IndexingModel.FullTextIndexInfo(table, fullTextIndex.Name);
      foreach (var fullTextColumn in fullTextIndex.Columns) {
        var column = table.Columns[fullTextColumn.Name];
        var typeColumn = fullTextColumn.TypeColumn == null
          ? null
          : primaryIndex.ValueColumns[fullTextColumn.TypeColumn.Name];
        var ftColumn = new IndexingModel.FullTextColumnRef(ftIndex, column, fullTextColumn.Configuration, typeColumn);
      }
      return ftIndex;
    }

    /// <summary>
    /// Visits primary index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    private IPathNode VisitPrimaryIndexInfo(IndexInfo index)
    {
      var tableName = index.ReflectedType.MappingName;
      CurrentTable = new IndexingModel.TableInfo(StorageInfo, tableName);
      foreach (var column in index.Columns)
        Visit(column);

      var primaryIndex = new IndexingModel.PrimaryIndexInfo(CurrentTable, index.MappingName);
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns) {
        string columName = GetPrimaryIndexColumnName(index, pair.Key, index);
        var column = CurrentTable.Columns[columName];
        new IndexingModel.KeyColumnRef(primaryIndex, column,
          ProviderInfo.Supports(ProviderFeatures.KeyColumnSortOrder)
            ? pair.Value
            : Direction.Positive);
      }
      primaryIndex.PopulateValueColumns();

      foreach (var indexInfo in index.ReflectedType.Indexes.Where(i => i.IsSecondary && !i.IsVirtual))
        Visit(indexInfo);

      CurrentTable = null;
      return primaryIndex;
    }

    #region Not supported

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitKeyField(KeyField keyField)
    {
      throw new NotSupportedException(String.Format(Strings.ExVisitKeyFieldIsNotSupportedByX, typeof (DomainModelConverter)));
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitFieldInfo(FieldInfo field)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitHierarchyInfo(HierarchyInfo hierarchy)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitTypeInfo(TypeInfo type)
    {
      throw new NotSupportedException();
    }

    #endregion

    #region Helper methods

    private object GetColumnDefaultValue(ColumnInfo column, Indexing.Model.TypeInfo typeInfo)
    {
      if (column.DefaultValue!=null)
        return column.DefaultValue;

      if (column.IsNullable)
        return null;
      
      var type = typeInfo.Type;
      if (type==typeof(string))
        return string.Empty;
      if (type==typeof(byte[]))
        return ArrayUtils<byte>.EmptyArray;
      return Activator.CreateInstance(column.ValueType);
    }

    /// <summary>
    /// Converts the <see cref="Xtensive.Storage.OnRemoveAction"/> to 
    /// <see cref="Xtensive.Storage.Indexing.Model.ReferentialAction"/>.
    /// </summary>
    /// <param name="toConvert">The action to convert.</param>
    /// <returns>Converted action.</returns>
    private static ReferentialAction ConvertReferentialAction(OnRemoveAction toConvert)
    {
      switch (toConvert) {
      case OnRemoveAction.Deny:
        return ReferentialAction.Restrict;
      case OnRemoveAction.Cascade:
        return ReferentialAction.Cascade;
      case OnRemoveAction.Clear:
        return ReferentialAction.Clear;
      default:
        return ReferentialAction.Default;
      }
    }

    /// <summary>
    /// Finds the specific index by key columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>The index.</returns>
    private static Indexing.Model.IndexInfo FindIndex(TableInfo table, IEnumerable<string> keyColumns)
    {
      IEnumerable<string> primaryKeyColumns = table.PrimaryIndex.KeyColumns.Select(cr => cr.Value.Name);
      if (primaryKeyColumns.Except(keyColumns)
        .Union(keyColumns.Except(primaryKeyColumns)).Count()==0)
        return table.PrimaryIndex;

      foreach (SecondaryIndexInfo index in table.SecondaryIndexes) {
        IEnumerable<string> secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value.Name);
        if (secondaryKeyColumns.Except(keyColumns)
          .Union(keyColumns.Except(secondaryKeyColumns)).Count()==0)
          return index;
      }
      return null;
    }

    /// <summary>
    /// Finds the index of the real.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="field">The field.</param>
    /// <returns></returns>
    private static IndexInfo FindIndex(IndexInfo index, FieldInfo field)
    {
      if (index.IsVirtual) {
        foreach (var underlyingIndex in index.UnderlyingIndexes) {
          var result = FindIndex(underlyingIndex, field);
          if (result!=null)
            return result;
        }
      }
      else if (field==null || index.Columns.ContainsAny(field.Columns))
        return index;
      return null;
    }

    /// <summary>
    /// Finds the non virtual primary index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Primary index.</returns>
    private static IndexInfo FindNonVirtualPrimaryIndex(IndexInfo index)
    {
      if (index.IsPrimary && !index.IsVirtual)
        return index;
      var primaryIndex = index.ReflectedType.Indexes.PrimaryIndex;

      return
        primaryIndex.IsVirtual
          ? primaryIndex.DeclaringIndex
          : primaryIndex;
    }

    /// <summary>
    /// Gets the table.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Table.</returns>
    private TableInfo GetTable(TypeInfo type)
    {
      if (type.Hierarchy==null || type.Hierarchy.InheritanceSchema!=InheritanceSchema.SingleTable)
        return StorageInfo.Tables.FirstOrDefault(table => table.Name==type.MappingName);
      if (type.IsInterface)
        return null;
      return StorageInfo.Tables.FirstOrDefault(table => table.Name==type.Hierarchy.Root.MappingName);
    }

    /// <summary>
    /// Gets the name of the primary index column.
    /// </summary>
    /// <param name="primaryIndex">Index of the primary.</param>
    /// <param name="secondaryIndexColumn">The secondary index column.</param>
    /// <param name="secondaryIndex">Index of the secondary.</param>
    /// <returns>Columns name.</returns>
    private static string GetPrimaryIndexColumnName(IndexInfo primaryIndex, ColumnInfo secondaryIndexColumn, IndexInfo secondaryIndex)
    {
      string primaryIndexColumnName = null;
      foreach (var primaryColumn in primaryIndex.Columns)
        if (primaryColumn.Field.Equals(secondaryIndexColumn.Field)) {
          primaryIndexColumnName = primaryColumn.Name;
          break;
        }
      return primaryIndexColumnName;
    }

    private static void CreateForeignKey(TableInfo referencingTable, string foreignKeyName, TableInfo referencedTable, Indexing.Model.IndexInfo referencingIndex)
    {
      var foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName) {
        PrimaryKey = referencedTable.PrimaryIndex,
        OnRemoveAction = ReferentialAction.None,
        OnUpdateAction = ReferentialAction.None
      };
      foreignKey.ForeignKeyColumns.Set(referencingIndex);
      return;
    }

    private static Type ToNullable(Type type, bool isNullable)
    {
      return isNullable && type.IsValueType && !type.IsNullable()
        ? type.ToNullable()
        : type;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="providerInfo">The provider info.</param>
    /// <param name="buildForeignKeys">If set to <see langword="true"/>, foreign keys
    /// will be created for associations.</param>
    /// <param name="foreignKeyNameGenerator">The foreign key name generator.</param>
    /// <param name="buildHierarchyForeignKeys">If set to <see langword="true"/>, foreign keys
    /// will be created for hierarchies.</param>
    /// <param name="hierarchyForeignKeyNameGenerator">The hierarchy foreign key name generator.</param>
    /// <param name="typeBuilder">The type builder.</param>
    public DomainModelConverter(
      ProviderInfo providerInfo, 
      bool buildForeignKeys, 
      Func<AssociationInfo, FieldInfo, string> foreignKeyNameGenerator, 
      bool buildHierarchyForeignKeys, 
      Func<TypeInfo, TypeInfo, string> hierarchyForeignKeyNameGenerator, 
      Func<Type, int?, int?, int?, Indexing.Model.TypeInfo> typeBuilder)
    {
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");
      if (buildForeignKeys)
        ArgumentValidator.EnsureArgumentNotNull(foreignKeyNameGenerator, 
          "foreignKeyNameGenerator");
      if (buildHierarchyForeignKeys) {
        ArgumentValidator.EnsureArgumentNotNull(hierarchyForeignKeyNameGenerator,
          "hierarchyForeignKeyNameGenerator");
      }

      BuildForeignKeys = buildForeignKeys;
      ForeignKeyNameGenerator = foreignKeyNameGenerator;
      BuildHierarchyForeignKeys = buildHierarchyForeignKeys;
      HierarchyForeignKeyNameGenerator = hierarchyForeignKeyNameGenerator;
      ProviderInfo = providerInfo;
      TypeBuilder = typeBuilder;
    }
  }
}