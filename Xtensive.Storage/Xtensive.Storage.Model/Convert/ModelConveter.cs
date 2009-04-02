// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.02

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Storage.Indexing.Model;
using StorageColumnInfo = Xtensive.Storage.Indexing.Model.ColumnInfo;
using StorageTypeInfo = Xtensive.Storage.Indexing.Model.TypeInfo;
using StorageIndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;

namespace Xtensive.Storage.Model.Convert
{
  /// <summary>
  /// Converts <see cref="DomainModel"/> to indexing storage model.
  /// </summary>
  [Serializable]
  public class ModelConverter: ModelVisitor<IPathNode>
  {
    /// <summary>
    /// Gets the storage info.
    /// </summary>
    protected virtual StorageInfo StorageInfo { get; private set; }

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
      return StorageInfo;
    }

    /// <inheritdoc/>
    protected override IPathNode VisitDomainModel(DomainModel domainModel)
    {
      foreach (var primaryIndex in domainModel.RealIndexes.Where(i => i.IsPrimary))
        VisitPrimaryIndex(primaryIndex);

      foreach (var indexInfo in domainModel.RealIndexes.Where(i => !i.IsPrimary))
        Visit(indexInfo);

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
      CreateIndexColumnRefs(index, secondaryIndex);
      return secondaryIndex;
    }
    
    /// <inheritdoc/>
    protected override IPathNode VisitColumnInfo(ColumnInfo column)
    {
      var table = StorageInfo.Tables[column.Field.ReflectedType.MappingName];
      var type = new StorageTypeInfo(column.ValueType);
      return new StorageColumnInfo(table, column.Name, type);
    }

    /// <summary>
    /// Visits primary index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    protected virtual IPathNode VisitPrimaryIndex(IndexInfo index)
    {
      var table = new TableInfo(StorageInfo, index.ReflectedType.MappingName);
      foreach (var column in index.Columns)
        Visit(column);

      var primaryIndex = new PrimaryIndexInfo(table, index.MappingName);
      CreateIndexColumnRefs(index, primaryIndex);
      return primaryIndex;
    }

    /// <summary>
    /// Creates column references for index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storageIndex">Index of the storage model.</param>
    protected virtual void CreateIndexColumnRefs(IndexInfo index, StorageIndexInfo storageIndex)
    {
      var table = StorageInfo.Tables[index.ReflectedType.MappingName];

      foreach (KeyValuePair<ColumnInfo, Direction> pair in index.KeyColumns)
        new KeyColumnRef(storageIndex, table.Columns[pair.Key.Name],
          storageIndex.KeyColumns.Count, pair.Value);
      foreach (var column in index.ValueColumns)
        new ValueColumnRef(storageIndex, table.Columns[column.Name],
          storageIndex.ValueColumns.Count);
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
    protected override IPathNode VisitAssociationInfo(AssociationInfo association)
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