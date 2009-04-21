// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using IndexInfo=Xtensive.Storage.Model.IndexInfo;
using StorageIndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Providers.Memory
{
  public class DomainHandler : Index.DomainHandler
  {
    private readonly Dictionary<Pair<IndexInfo, TypeInfo>, MapTransform> primaryIndexTransforms = 
      new Dictionary<Pair<IndexInfo, TypeInfo>, MapTransform>();

    private readonly Dictionary<IndexInfo, StorageIndexInfo> mappings =
      new Dictionary<IndexInfo, StorageIndexInfo>();
    
    /// <inheritdoc/>
    protected override Index.IndexStorage CreateLocalStorage(string name)
    {
      return new IndexStorage(name);
    }

    /// <inheritdoc/>
    public override void BuildMappingSchema()
    {
      var domainModel = Building.BuildingContext.Current.Model;
      foreach (var type in domainModel.Types) {
        foreach (var indexInfo in type.AffectedIndexes.Where(index => index.IsPrimary)) {
          var primaryIndex = type.Indexes.PrimaryIndex;
          var primaryIndexColumns = primaryIndex.Columns.ToList();
          var indexColumns = indexInfo.Columns.ToList();
          var map = indexColumns.Select(column => primaryIndexColumns.IndexOf(column)).ToArray();
          primaryIndexTransforms.Add(
            new Pair<IndexInfo, TypeInfo>(indexInfo, type),
            new MapTransform(true, indexInfo.TupleDescriptor, map));
        }
      }

      foreach (var indexInfo in domainModel.RealIndexes) {
        StorageIndexInfo storageIndex = null;
        foreach (var table in Storage.Model.Tables)
          foreach (var index in GetAllIndexes(table))
            if (index.Name==indexInfo.MappingName)
              storageIndex = index;
        mappings.Add(indexInfo, storageIndex);
      }
    }

    internal MapTransform GetTransform(IndexInfo indexInfo, TypeInfo type)
    {
      return GetIndexTransform(indexInfo, type);
    }

    /// <inheritdoc/>
    protected override IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfo indexInfo)
    {
      var storageIndex = ConvertIndexInfo(indexInfo);
      return Storage.GetRealIndex(storageIndex);
    }

    /// <inheritdoc/>
    protected override MapTransform GetIndexTransform(IndexInfo indexInfo, TypeInfo type)
    {
      return primaryIndexTransforms[new Pair<IndexInfo, TypeInfo>(indexInfo, type)];
    }

    /// <inheritdoc/>
    public override StorageIndexInfo ConvertIndexInfo(IndexInfoRef indexInfo)
    {
      return mappings[indexInfo.Resolve(Domain.Model)];
    }

    private StorageIndexInfo ConvertIndexInfo(IndexInfo indexInfo)
    {
      return mappings[indexInfo];
    }

    private static IEnumerable<StorageIndexInfo> GetAllIndexes(TableInfo table)
    {
      yield return table.PrimaryIndex;
      foreach (var indexInfo in table.SecondaryIndexes)
        yield return indexInfo;
    }

  }
}