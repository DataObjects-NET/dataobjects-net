// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Modelling;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers;
using DomainIndexInfo=Xtensive.Storage.Model.IndexInfo;
using DomainTypeInfo = Xtensive.Storage.Model.TypeInfo;
using AssociationInfo = Xtensive.Storage.Model.AssociationInfo;
using GeneratorInfo = Xtensive.Storage.Model.GeneratorInfo;
using DomainModel = Xtensive.Storage.Model.DomainModel;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;
using DomainColumnInfo = Xtensive.Storage.Model.ColumnInfo;
using KeyField = Xtensive.Storage.Model.KeyField;
using HierarchyInfo = Xtensive.Storage.Model.HierarchyInfo;
using KeyInfo = Xtensive.Storage.Model.KeyInfo;
using InheritanceSchema = Xtensive.Storage.Model.InheritanceSchema;
using Node = Xtensive.Storage.Model.Node;
using IndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;
using ReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Converts <see cref="Storage.Model.DomainModel"/> to indexing storage model.
  /// </summary>
  internal sealed class DomainModelConverter : Model.ModelVisitor<IPathNode>
  {
    /// <summary>
    /// Gets the persistent generator filter.
    /// </summary>
    private Func<GeneratorInfo, bool> PersistentGeneratorFilter { get; set; }

    /// <summary>
    /// Gets the provider info.
    /// </summary>
    private ProviderInfo ProviderInfo { get; set; }

    /// <summary>
    /// Gets the type builder.
    /// </summary>
    private Func<Type, int?, TypeInfo> TypeBuilder { get; set; }

    /// <summary>
    /// Gets the storage info.
    /// </summary>
    private StorageInfo StorageInfo { get; set; }

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
    private Func<DomainTypeInfo, DomainTypeInfo, string> HierarchyForeignKeyNameGenerator { get; set; }

    /// <summary>
    /// Gets or sets the currently visiting table.
    /// </summary>
    private TableInfo CurrentTable { get; set; }

    /// <summary>
    /// Converts the specified <see cref="DomainModel"/> to
    /// <see cref="StorageInfo"/>.
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <returns>The storage model.</returns>
    public StorageInfo Convert(DomainModel domainModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      
      StorageInfo = new StorageInfo();
      Model = domainModel;
      Visit(domainModel);
      return StorageInfo;
    }
    
    /// <inheritdoc/>
    protected override IPathNode Visit(Node node)
    {
      var indexInfo = node as DomainIndexInfo;
      if (indexInfo != null && indexInfo.IsPrimary)
        return VisitPrimaryIndexInfo(indexInfo);

      return base.Visit(node);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitDomainModel(DomainModel domainModel)
    {
      // Build tables, columns and primary indexes
      foreach (var primaryIndex in domainModel.RealIndexes.Where(i => i.IsPrimary))
        Visit(primaryIndex);

     // Build foreign keys
      if (BuildForeignKeys && ProviderInfo.SupportsForeignKeyConstraints)
        foreach (var association in domainModel.Associations)
          Visit(association);

      // Build sequnces
      var persistentGenerators = domainModel.Generators
        .Where(g => PersistentGeneratorFilter.Invoke(g)).ToArray();
      foreach (var generator in persistentGenerators)
        Visit(generator);

      if (!BuildHierarchyForeignKeys || !ProviderInfo.SupportsForeignKeyConstraints)
        return StorageInfo;

      // Build hierarchy foreign keys
      var indexPairs = new Dictionary<Pair<DomainIndexInfo>, object>();
      foreach (var type in domainModel.Types.Entities) {
        if (type.Indexes.PrimaryIndex.IsVirtual) {
          var realPrimaryIndexes = type.Indexes.RealPrimaryIndexes;
          for (var i = 0; i < realPrimaryIndexes.Count - 1; i++) {
            if (realPrimaryIndexes[i]!=realPrimaryIndexes[i + 1]) {
              var pair = new Pair<DomainIndexInfo>(realPrimaryIndexes[i], realPrimaryIndexes[i + 1]);
              indexPairs[pair] = null;
            }
          }
        }
      }
      foreach (var indexPair in indexPairs.Keys) {
        var referencedIndex = indexPair.First;
        if (referencedIndex.ReflectedType.Hierarchy.Schema == InheritanceSchema.ConcreteTable)
          continue;
        var referencingIndex = indexPair.Second;
        var referencingTable = StorageInfo.Tables[referencingIndex.ReflectedType.MappingName];
        var referencedTable = StorageInfo.Tables[referencedIndex.ReflectedType.MappingName];
        var storageReferencingIndex = FindIndex(referencingTable,
          new List<string>(referencingIndex.KeyColumns.Select(ci => ci.Key.Name)));
        
        var foreignKeyName = HierarchyForeignKeyNameGenerator.Invoke(referencingIndex.ReflectedType, referencedIndex.ReflectedType);
        CreateForeignKey(referencingTable, foreignKeyName, referencedTable, storageReferencingIndex);
      }
      
      return StorageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndexInfo(DomainIndexInfo index)
    {
      var table = CurrentTable;
      var primaryIndex = Model.RealIndexes.First(i => i.MappingName==table.PrimaryIndex.Name);
      var secondaryIndex = new SecondaryIndexInfo(table, index.MappingName) {
        IsUnique = index.IsUnique
      };
      foreach (KeyValuePair<DomainColumnInfo, Direction> pair in index.KeyColumns) {
        var columName = GetPrimaryIndexColumnName(primaryIndex, pair.Key, index);
        var column = table.Columns[columName];
        new KeyColumnRef(secondaryIndex, column,
          ProviderInfo.SupportKeyColumnSortOrder
            ? pair.Value
            : Direction.Positive);
      }
      if (ProviderInfo.SupportsIncludedColumns)
        foreach (var includedColumn in index.IncludedColumns) {
          var columName = GetPrimaryIndexColumnName(primaryIndex, includedColumn, index);
          var column = table.Columns[columName];
          new IncludedColumnRef(secondaryIndex, column);
        }
      secondaryIndex.PopulatePrimaryKeyColumns();
      return secondaryIndex;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitColumnInfo(DomainColumnInfo column)
    {
      var originalType = column.ValueType;
      var originalTypeInfo = new TypeInfo(GetType(originalType, column.IsNullable), column.IsNullable, column.Length);
      var storageTypeInfo = TypeBuilder.Invoke(originalType, column.Length);
      storageTypeInfo = new TypeInfo(GetType(storageTypeInfo.Type, column.IsNullable), column.IsNullable, storageTypeInfo.Length);

      var defaultValue = !column.IsNullable && originalType.IsValueType
        ? Activator.CreateInstance(originalType)
        : null;
      if (defaultValue is char)
        defaultValue = '0';

      return new ColumnInfo(CurrentTable, column.Name, storageTypeInfo) {
        DefaultValue = defaultValue,
        OriginalType = originalTypeInfo
      };
    }

    /// <inheritdoc/>
    protected override IPathNode VisitAssociationInfo(AssociationInfo association)
    {
      if (!association.IsMaster 
        || association.TargetType.Hierarchy.Schema == InheritanceSchema.ConcreteTable)
        return null;

      TableInfo referencedTable;
      TableInfo referencingTable;
      IndexInfo referencingIndex;
      ForeignKeyInfo foreignKey;

      if (association.AuxiliaryType==null) {
        if (association.OwnerType.Indexes.PrimaryIndex==null)
          return null;
        referencedTable = GetTable(association.TargetType);
        referencingTable = GetTable(association.OwnerType);
        referencingIndex = FindIndex(referencingTable,
          new List<string>(association.OwnerField.ExtractColumns().Select(ci => ci.Name)));
        var foreignKeyName = ForeignKeyNameGenerator(association, association.OwnerField);
        foreignKey = CreateForeignKey(referencingTable, foreignKeyName, referencedTable, referencingIndex);
        return foreignKey;
      }

      foreignKey = null;
      referencingTable = GetTable(association.AuxiliaryType);
      foreach (var field in association.AuxiliaryType.Fields.Where(fieldInfo => fieldInfo.IsEntity)) {
        var referencedIndex = FindIndex(Model.Types[field.ValueType].Indexes.PrimaryIndex, null);
        referencedTable = GetTable(referencedIndex.DeclaringType);
        referencingIndex = FindIndex(referencingTable,
          new List<string>(field.ExtractColumns().Select(ci => ci.Name)));
        var foreignKeyName = ForeignKeyNameGenerator(association, field);
        foreignKey = CreateForeignKey(referencingTable, foreignKeyName, referencedTable, referencingIndex);
      }
      return foreignKey;
    }

    /// <summary>
    /// Visits primary index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    private IPathNode VisitPrimaryIndexInfo(DomainIndexInfo index)
    {
      var tableName = index.ReflectedType.MappingName;
      CurrentTable = new TableInfo(StorageInfo, tableName);
      foreach (var column in index.Columns)
        Visit(column);

      var primaryIndex = new PrimaryIndexInfo(CurrentTable, index.MappingName);
      foreach (KeyValuePair<DomainColumnInfo, Direction> pair in index.KeyColumns) {
        var columName = GetPrimaryIndexColumnName(index, pair.Key, index);
        var column = CurrentTable.Columns[columName];
        new KeyColumnRef(primaryIndex, column,
          ProviderInfo.SupportKeyColumnSortOrder
            ? pair.Value
            : Direction.Positive);
      }
      primaryIndex.PopulateValueColumns();

      foreach (var indexInfo in index.ReflectedType.Indexes.Where(i=>i.IsSecondary && !i.IsVirtual))
        Visit(indexInfo);

      CurrentTable = null;
      return primaryIndex;
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitGeneratorInfo(GeneratorInfo generator)
    {
      var sequence = new SequenceInfo(StorageInfo, generator.MappingName) {
        StartValue = generator.CacheSize,
        Increment = generator.CacheSize,
        Type = TypeBuilder.Invoke(generator.TupleDescriptor[0], null)
      };
      return sequence;
    }

    /// <summary>
    /// Finds the specific index by key columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>The index.</returns>
    private static IndexInfo FindIndex(TableInfo table, List<string> keyColumns)
    {
      var primaryKeyColumns = table.PrimaryIndex.KeyColumns.Select(cr => cr.Value.Name);
      if (primaryKeyColumns.Except(keyColumns)
        .Union(keyColumns.Except(primaryKeyColumns)).Count()==0)
        return table.PrimaryIndex;

      foreach (var index in table.SecondaryIndexes) {
        var secondaryKeyColumns = index.KeyColumns.Select(cr => cr.Value.Name);
        if (secondaryKeyColumns.Except(keyColumns)
          .Union(keyColumns.Except(secondaryKeyColumns)).Count()==0)
          return index;
      }
      return null;
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
      case OnRemoveAction.None:
        return ReferentialAction.None;
      case OnRemoveAction.Cascade:
        return ReferentialAction.Cascade;
      case OnRemoveAction.Clear:
        return ReferentialAction.Clear;
      default:
        return ReferentialAction.Default;
      }
    }

    /// <summary>
    /// Finds the index of the real.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="field">The field.</param>
    /// <returns></returns>
    private static DomainIndexInfo FindIndex(DomainIndexInfo index, FieldInfo field)
    {
      if (index.IsVirtual)
        foreach (var underlyingIndex in index.UnderlyingIndexes) {
          var result = FindIndex(underlyingIndex, field);
          if (result!=null)
            return result;
        }
      else if (field==null || index.Columns.ContainsAny(field.ExtractColumns()))
        return index;
      return null;
    }

    /// <summary>
    /// Finds the non virtual primary index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Primary index.</returns>
    private static DomainIndexInfo FindNonVirtualPrimaryIndex(DomainIndexInfo index)
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
    /// Gets the name of the primary index column.
    /// </summary>
    /// <param name="primaryIndex">Index of the primary.</param>
    /// <param name="secondaryIndexColumn">The secondary index column.</param>
    /// <param name="secondaryIndex">Index of the secondary.</param>
    /// <returns>Columns name.</returns>
    private static string GetPrimaryIndexColumnName(DomainIndexInfo primaryIndex, 
      DomainColumnInfo secondaryIndexColumn, DomainIndexInfo secondaryIndex)
    {
      string primaryIndexColumnName = null;
      foreach (var primaryColumn in primaryIndex.Columns)
        if (primaryColumn.Field.Equals(secondaryIndexColumn.Field)) {
          primaryIndexColumnName = primaryColumn.Name;
          break;
        }
      return primaryIndexColumnName;
    }

    /// <summary>
    /// Gets the table.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Table.</returns>
    private TableInfo GetTable(DomainTypeInfo type)
    {
      if (type.Hierarchy==null 
        || type.Hierarchy.Schema!=InheritanceSchema.SingleTable)
        return StorageInfo.Tables[type.MappingName];
      return StorageInfo.Tables[type.Hierarchy.Root.MappingName];
    }

    private static ForeignKeyInfo CreateForeignKey(TableInfo referencingTable, string foreignKeyName, 
      TableInfo referencedTable, IndexInfo referencingIndex)
    {
      var foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName) {
        PrimaryKey = referencedTable.PrimaryIndex,
        OnRemoveAction = ReferentialAction.None,
        OnUpdateAction = ReferentialAction.None
      };
      foreignKey.ForeignKeyColumns.Set(referencingIndex);
      return foreignKey;
    }

    private static Type GetType(Type type, bool isNullable)
    {
      return isNullable && type.IsValueType && !type.IsNullable()
        ? type.ToNullable()
        : type;
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="buildForeignKeys">If set to <see langword="true"/> foreign keys
    /// will be created for associations.</param>
    /// <param name="foreignKeyNameGenerator">The foreign key name generator.</param>
    /// <param name="buildHierarchyForeignKeys">If set to <see langword="true"/> foreign keys
    /// will be created for hierarchies.</param>
    /// <param name="hierarchyForeignKeyNameGenerator">The hierarchy foreign key name generator.</param>
    /// <param name="persistentGeneratorFilter">The persistent generator filter.</param>
    /// <param name="providerInfo">The provider info.</param>
    /// <param name="typeBuilder">The type builder.</param>
    public DomainModelConverter(bool buildForeignKeys, Func<AssociationInfo, FieldInfo, string> foreignKeyNameGenerator,
      bool buildHierarchyForeignKeys, Func<DomainTypeInfo, DomainTypeInfo, string> hierarchyForeignKeyNameGenerator,
      Func<GeneratorInfo, bool> persistentGeneratorFilter, ProviderInfo providerInfo, Func<Type, int?, TypeInfo> typeBuilder)
    {
      if (buildForeignKeys)
        ArgumentValidator.EnsureArgumentNotNull(foreignKeyNameGenerator, "foreignKeyNameGenerator");
      if (buildHierarchyForeignKeys)
        ArgumentValidator.EnsureArgumentNotNull(hierarchyForeignKeyNameGenerator,
        "hierarchyForeignKeyNameGenerator");
      ArgumentValidator.EnsureArgumentNotNull(persistentGeneratorFilter, "persistentGeneratorFilter");
      ArgumentValidator.EnsureArgumentNotNull(providerInfo, "providerInfo");

      BuildForeignKeys = buildForeignKeys;
      ForeignKeyNameGenerator = foreignKeyNameGenerator;
      BuildHierarchyForeignKeys = buildHierarchyForeignKeys;
      HierarchyForeignKeyNameGenerator = hierarchyForeignKeyNameGenerator;
      PersistentGeneratorFilter = persistentGeneratorFilter;
      ProviderInfo = providerInfo;
      TypeBuilder = typeBuilder;
    }
    
    #region Not supported

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitKeyField(KeyField keyField)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitFieldInfo(FieldInfo field)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitKeyInfo(KeyInfo key)
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
    protected override IPathNode VisitTypeInfo(DomainTypeInfo type)
    {
      throw new NotSupportedException();
    }
    
    #endregion
  }
}