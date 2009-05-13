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
using StorageColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using StorageTypeInfo = Xtensive.Storage.Indexing.Model.TypeInfo;
using StorageIndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;
using StorageReferentialAction = Xtensive.Storage.Indexing.Model.ReferentialAction;

namespace Xtensive.Storage.Model.Conversion
{
  /// <summary>
  /// Converts <see cref="DomainModel"/> to indexing storage model.
  /// </summary>
  [Serializable]
  public class DomainModelConverter : ModelVisitor<IPathNode>
  {
    /// <summary>
    /// Gets the persistent generator filter.
    /// </summary>
    protected Func<GeneratorInfo, bool> PersistentGeneratorFilter { get; private set; }

    /// <summary>
    /// Gets the storage info.
    /// </summary>
    protected StorageInfo StorageInfo { get; private set; }

    /// <summary>
    /// Gets the currently converting model.
    /// </summary>
    protected DomainModel Model { get; private set; }

    /// <summary>
    /// Gets a value indicating whether 
    /// build foreign keys for associations.
    /// </summary>
    public bool BuildForeignKeys { get; private set; }

    /// <summary>
    /// Gets the foreign key name generator.
    /// </summary>
    protected Func<AssociationInfo, FieldInfo, string> ForeignKeyNameGenerator { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether 
    /// build foreign keys for hierarchies.
    /// </summary>
    public bool BuildHierarchyForeignKeys { get; private set; }

    /// <summary>
    /// Gets the hierarchy foreign key name generator.
    /// </summary>
    protected Func<TypeInfo, TypeInfo, string> HierarchyForeignKeyNameGenerator { get; private set; }

    /// <summary>
    /// Gets or sets the currently visiting table.
    /// </summary>
    protected TableInfo CurrentTable { get; set; }

    /// <summary>
    /// Converts the specified <see cref="DomainModel"/> to
    /// <see cref="StorageInfo"/>.
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <param name="storageName">Name of the storage.</param>
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
      var indexInfo = node as IndexInfo;
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

      // Build secondary indexes
      // foreach (var indexInfo in domainModel.RealIndexes.Where(i => !i.IsPrimary))
      //  Visit(indexInfo);

      // Build foreign keys
      if (BuildForeignKeys)
        foreach (var association in domainModel.Associations)
          Visit(association);

      // TODO: Build foreign keys for hierarchy references

      // Build sequnces
      var persistentGenerators = domainModel.Generators
        .Where(g => PersistentGeneratorFilter.Invoke(g)).ToArray();
      foreach (var generator in persistentGenerators)
        Visit(generator);

      return StorageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndexInfo(IndexInfo index)
    {
      var table = CurrentTable;
      var primaryIndex = Model.RealIndexes.First(i => i.MappingName == table.PrimaryIndex.Name);
      var secondaryIndex = new SecondaryIndexInfo(table, index.MappingName)
        {
          IsUnique = index.IsUnique
        };
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns)
      {
        var columName = GetPrimaryIndexColumnName(primaryIndex, pair.Key, index);
        var column = table.Columns[columName];
        new KeyColumnRef(secondaryIndex, column, pair.Value);
      }
      foreach (var includedColumn in index.IncludedColumns) {
        var columName = GetPrimaryIndexColumnName(primaryIndex, includedColumn, index);
        var column = table.Columns[columName];
        new IncludedColumnRef(secondaryIndex, column);
      }
      secondaryIndex.PopulatePrimaryKeyColumns();
      return secondaryIndex;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitColumnInfo(ColumnInfo column)
    {
      var columnType = column.IsNullable
        && column.ValueType.IsValueType
          && !column.ValueType.IsNullable()
        ? column.ValueType.ToNullable()
        : column.ValueType;

      var type = new StorageTypeInfo(columnType, column.IsNullable, column.Length);
      return new StorageColumnInfo(CurrentTable, column.Name, type);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitAssociationInfo(AssociationInfo association)
    {
      if (!association.IsMaster)
        return null;

      TableInfo referencedTable;
      TableInfo referencingTable;
      StorageIndexInfo referencingIndex;
      ForeignKeyInfo foreignKey;

      if (association.UnderlyingType==null) {
        if (association.ReferencingType.Indexes.PrimaryIndex==null)
            return null;
        referencedTable = StorageInfo.Tables[association.ReferencedType.MappingName];
        referencingTable = StorageInfo.Tables[association.ReferencingType.MappingName];
        referencingIndex = FindIndex(referencingTable,
          new List<string>(association.ReferencingField.ExtractColumns().Select(ci => ci.Name)));
        var foreignKeyName = ForeignKeyNameGenerator(association, association.ReferencingField);
        foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName)
          {
            PrimaryKey = referencedTable.PrimaryIndex,
            OnRemoveAction = StorageReferentialAction.None,
            OnUpdateAction = StorageReferentialAction.None
          };
        foreignKey.ForeignKeyColumns.Set(referencingIndex);
        return foreignKey;
      }

      foreignKey = null;
      referencingTable = StorageInfo.Tables[association.UnderlyingType.MappingName];
      foreach (var field in association.UnderlyingType.Fields.Where(fieldInfo => fieldInfo.IsEntity)) {
        var referencedIndex = FindIndex(Model.Types[field.ValueType].Indexes.PrimaryIndex, null);
        referencedTable = StorageInfo.Tables[referencedIndex.DeclaringType.MappingName];
        referencingIndex = FindIndex(referencingTable,
          new List<string>(field.ExtractColumns().Select(ci => ci.Name)));
        var foreignKeyName = ForeignKeyNameGenerator(association, field);
        foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName)
          {
            PrimaryKey = referencedTable.PrimaryIndex,
            OnRemoveAction = StorageReferentialAction.None,
            OnUpdateAction = StorageReferentialAction.None
          };
        foreignKey.ForeignKeyColumns.Set(referencingIndex);
      }
      return foreignKey;
    }

    /// <summary>
    /// Visits primary index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    protected IPathNode VisitPrimaryIndexInfo(IndexInfo index)
    {
      var tableName = index.ReflectedType.MappingName;
      CurrentTable = new TableInfo(StorageInfo, tableName);
      foreach (var column in index.Columns)
        Visit(column);

      var primaryIndex = new PrimaryIndexInfo(CurrentTable, index.MappingName);
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns)
        new KeyColumnRef(primaryIndex, CurrentTable.Columns[pair.Key.Name], pair.Value);
      primaryIndex.PopulateValueColumns();

      foreach (var indexInfo in index.ReflectedType.Indexes.Where(i=>i.IsSecondary && !i.IsVirtual)) {
        Visit(indexInfo);
      }
      CurrentTable = null;
      return primaryIndex;
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitGeneratorInfo(GeneratorInfo generator)
    {
      var sequence = new SequenceInfo(StorageInfo, generator.MappingName) {
        StartValue = generator.CacheSize,
        Increment = generator.CacheSize,
        Type = new StorageTypeInfo(generator.TupleDescriptor[0])
      };
      return sequence;
    }

    /// <summary>
    /// Finds the specific index by key columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>The index.</returns>
    protected StorageIndexInfo FindIndex(TableInfo table, List<string> keyColumns)
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
    /// Converts the <see cref="Xtensive.Storage.Model.ReferentialAction"/> to 
    /// <see cref="Xtensive.Storage.Indexing.Model.ReferentialAction"/>.
    /// </summary>
    /// <param name="toConvert">The action to convert.</param>
    /// <returns>Converted action.</returns>
    protected StorageReferentialAction ConvertReferentialAction(ReferentialAction toConvert)
    {
      switch (toConvert) {
      case ReferentialAction.Restrict:
        return StorageReferentialAction.Restrict;
      case ReferentialAction.NoAction:
        return StorageReferentialAction.None;
      case ReferentialAction.Cascade:
        return StorageReferentialAction.Cascade;
      case ReferentialAction.Clear:
        return StorageReferentialAction.Clear;
      default:
        return StorageReferentialAction.Default;
      }
    }

    /// <summary>
    /// Finds the index of the real.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="field">The field.</param>
    /// <returns></returns>
    protected static IndexInfo FindIndex(IndexInfo index, FieldInfo field)
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
    protected static IndexInfo FindNonVirtualPrimaryIndex(IndexInfo index)
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
    protected static string GetPrimaryIndexColumnName(IndexInfo primaryIndex, 
      ColumnInfo secondaryIndexColumn, IndexInfo secondaryIndex)
    {
      string primaryIndexColumnName = null;
      foreach (var primaryColumn in primaryIndex.Columns)
        if (primaryColumn.Field.Equals(secondaryIndexColumn.Field)) {
          primaryIndexColumnName = primaryColumn.Name;
          break;
        }
      return primaryIndexColumnName;
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
    public DomainModelConverter(bool buildForeignKeys, Func<AssociationInfo, FieldInfo, string> foreignKeyNameGenerator,
      bool buildHierarchyForeignKeys, Func<TypeInfo, TypeInfo, string> hierarchyForeignKeyNameGenerator,
      Func<GeneratorInfo, bool> persistentGeneratorFilter)
    {
      if (buildForeignKeys)
        ArgumentValidator.EnsureArgumentNotNull(foreignKeyNameGenerator, "foreignKeyNameGenerator");
      if (buildHierarchyForeignKeys)
        ArgumentValidator.EnsureArgumentNotNull(hierarchyForeignKeyNameGenerator,
        "hierarchyForeignKeyNameGenerator");
      ArgumentValidator.EnsureArgumentNotNull(persistentGeneratorFilter, "persistentGeneratorFilter");

      BuildForeignKeys = buildForeignKeys;
      ForeignKeyNameGenerator = foreignKeyNameGenerator;
      BuildHierarchyForeignKeys = buildHierarchyForeignKeys;
      HierarchyForeignKeyNameGenerator = hierarchyForeignKeyNameGenerator;
      PersistentGeneratorFilter = persistentGeneratorFilter;
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
    protected override IPathNode VisitTypeInfo(TypeInfo type)
    {
      throw new NotSupportedException();
    }
    
    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Method is not supported.</exception>
    protected override IPathNode VisitServiceInfo(ServiceInfo serviceInfo)
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}