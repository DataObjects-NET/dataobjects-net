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
    /// Gets the storage info.
    /// </summary>
    protected virtual StorageInfo StorageInfo { get; private set; }

    /// <summary>
    /// Gets the foreign key name generator.
    /// </summary>
    protected Func<AssociationInfo, FieldInfo, string> ForeignKeyNameGenerator { get; private set; }

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
    public StorageInfo Convert(DomainModel domainModel, string storageName)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(storageName, "storageName");
      
      StorageInfo = new StorageInfo(storageName);
      Visit(domainModel);
      StorageInfo.Lock(true);
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
      // Build tables, columns and primary indexes.
      foreach (var primaryIndex in domainModel.RealIndexes.Where(i => i.IsPrimary))
        Visit(primaryIndex);

      // Build secondary indexes.
      foreach (var indexInfo in domainModel.RealIndexes.Where(i => !i.IsPrimary))
        Visit(indexInfo);

      // Build foreign keys.
      foreach (var association in domainModel.Associations)
        Visit(association);

      // ToDo: Build forign keys for hierarchy references.

      return StorageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitIndexInfo(IndexInfo index)
    {
      var table = StorageInfo.Tables[index.ReflectedType.MappingName];
      var secondaryIndex = new SecondaryIndexInfo(table, index.MappingName)
        {
          IsUnique = index.IsUnique
        };
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns)
        new KeyColumnRef(secondaryIndex, table.Columns[pair.Key.Name], pair.Value);
      foreach (var column in index.IncludedColumns)
        new IncludedColumnRef(secondaryIndex, table.Columns[column.Name]);
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

      var isNullable = column.ValueType.IsValueType
        ? column.ValueType.IsNullable()
        : column.IsNullable;
      var type = new StorageTypeInfo(columnType, isNullable, column.Length ?? 0);
      return new StorageColumnInfo(CurrentTable, column.Name, type);
    }

    /// <inheritdoc/>
    protected override IPathNode VisitAssociationInfo(AssociationInfo association)
    {
      if (!association.IsMaster)
        return null;

      var referencedTable = StorageInfo.Tables[association.ReferencedType.MappingName];
      TableInfo referencingTable;
      StorageIndexInfo referencingIndex;
      ForeignKeyInfo foreignKey;

      if (association.UnderlyingType==null) {
        referencingTable = StorageInfo.Tables[association.ReferencingType.MappingName];
        referencingIndex = FindIndex(referencingTable,
          new List<string>(association.ReferencingField.ExtractColumns().Select(ci => ci.Name)));
        var foreignKeyName = ForeignKeyNameGenerator(association, association.ReferencingField);
        foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName)
          {
            PrimaryKey = referencedTable.PrimaryIndex,
            OnRemoveAction = ConvertReferentialAction(association.OnRemove),
            OnUpdateAction = StorageReferentialAction.Default
          };
        foreignKey.ForeignKeyColumns.Set(referencingIndex);
        return foreignKey;
      }

      foreignKey = null;
      referencingTable = StorageInfo.Tables[association.UnderlyingType.MappingName];
      foreach (var field in association.UnderlyingType.Fields.Where(fieldInfo => fieldInfo.IsEntity)) {
        referencingIndex = FindIndex(referencingTable,
          new List<string>(field.ExtractColumns().Select(ci => ci.Name)));
        var foreignKeyName = ForeignKeyNameGenerator(association, field);
        foreignKey = new ForeignKeyInfo(referencingTable, foreignKeyName)
          {
            PrimaryKey = referencedTable.PrimaryIndex,
            OnRemoveAction = ConvertReferentialAction(association.OnRemove),
            OnUpdateAction = StorageReferentialAction.Default
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
    protected virtual IPathNode VisitPrimaryIndexInfo(IndexInfo index)
    {
      CurrentTable = new TableInfo(StorageInfo, index.ReflectedType.MappingName);
      foreach (var column in index.Columns)
        Visit(column);

      var primaryIndex = new PrimaryIndexInfo(CurrentTable, index.MappingName);
      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns)
        new KeyColumnRef(primaryIndex, CurrentTable.Columns[pair.Key.Name], pair.Value);
      primaryIndex.PopulateValueColumns();
      CurrentTable = null;
      return primaryIndex;
    }
    
    /// <summary>
    /// Finds the specific index by key columns.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="keyColumns">The key columns.</param>
    /// <returns>The index.</returns>
    protected virtual StorageIndexInfo FindIndex(TableInfo table, List<string> keyColumns)
    {
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
    protected virtual StorageReferentialAction ConvertReferentialAction(ReferentialAction toConvert)
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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="foreignKeyNameGenerator">The foreign key name generator.</param>
    /// <param name="hierarchyForeignKeyNameGenerator">The hierarchy foreign key name generator.</param>
    public DomainModelConverter(Func<AssociationInfo, FieldInfo, string> foreignKeyNameGenerator,
      Func<TypeInfo, TypeInfo, string> hierarchyForeignKeyNameGenerator)
    {
      ArgumentValidator.EnsureArgumentNotNull(foreignKeyNameGenerator, "foreignKeyNameGenerator");
      ArgumentValidator.EnsureArgumentNotNull(hierarchyForeignKeyNameGenerator,
        "hierarchyForeignKeyNameGenerator");
      
      ForeignKeyNameGenerator = foreignKeyNameGenerator;
      HierarchyForeignKeyNameGenerator = hierarchyForeignKeyNameGenerator;
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
    protected override IPathNode VisitGeneratorInfo(GeneratorInfo generator)
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