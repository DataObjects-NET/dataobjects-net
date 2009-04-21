// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Building;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Optimization;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;
using IndexInfo=Xtensive.Storage.Model.IndexInfo;
using StorageIndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;


namespace Xtensive.Storage.Providers.Index
{
  public abstract class DomainHandler : Providers.DomainHandler
  {
    private readonly Dictionary<Pair<IndexInfo, TypeInfo>, MapTransform> primaryIndexTransforms = 
      new Dictionary<Pair<IndexInfo, TypeInfo>, MapTransform>();

    private readonly Dictionary<IndexInfo, StorageIndexInfo> indexInfoMapping =
      new Dictionary<IndexInfo, StorageIndexInfo>();

    /// <summary>
    /// Gets the index storage.
    /// </summary>
    protected IndexStorage Storage { get; private set; }

    /// <inheritdoc/>
    protected override IEnumerable<Type> GetProviderCompilerExtensionTypes()
    {
      return Type.EmptyTypes;
    }

    /// <inheritdoc/>
    protected override ICompiler BuildCompiler(BindingCollection<object, ExecutableProvider> compiledSources)
    {
      return new IndexCompiler(Handlers, compiledSources);
    }

    /// <inheritdoc/>
    protected override IOptimizer BuildOptimizer()
    {
      return new CompositeOptimizer(
        new OrderbyOptimizer(),
        new IndexOptimizer(Handlers.Domain.Model, new OptimizationInfoProviderResolver(this)),
        new RedundantColumnOptimizer()
        );
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
        foreach (var table in Storage.Model.Tables) {
          foreach (var index in GetAllIndexes(table))
            if (index.Name == indexInfo.MappingName) {
              storageIndex = index;
              break;
            }
        }
        indexInfoMapping.Add(indexInfo, storageIndex);
      }
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      var connectionInfo = BuildingContext.Current.Configuration.ConnectionInfo;
      var remoteUrl = connectionInfo.ToString(); // ToDo: Fix this.
      IndexStorage storage;
      if (!TryGetRemoteStorage(remoteUrl, out storage)) {
        storage = CreateLocalStorage(connectionInfo.Resource);
        MarshalStorage(storage, remoteUrl, connectionInfo.Port);
      }
      Storage = storage;
    }

    /// <summary>
    /// Converts the <see cref="IndexInfoRef"/> 
    /// to <see cref="Indexing.Model.IndexInfo"/>.
    /// </summary>
    /// <param name="indexInfoRef">The index info.</param>
    /// <returns>Converted index info.</returns>
    public StorageIndexInfo ConvertIndexInfo(IndexInfoRef indexInfoRef)
    {
      var indexInfo = indexInfoRef.Resolve(Domain.Model);
      return indexInfoMapping[indexInfo];
    }

    #region Build storage methods

    /// <summary>
    /// Gets the storage of real indexes.
    /// </summary>
    /// <returns>The storage.</returns>
    internal IndexStorage GetIndexStorage()
    {
      return Storage;
    }

    /// <summary>
    /// Tries get remote storage.
    /// </summary>
    /// <param name="url">The remote URL.</param>
    /// <param name="remoteStorage">The remote storage.</param>
    /// <returns><see langword="true"/> if remote storage has been found, 
    /// otherwise <see langword="false"/>.</returns>
    protected bool TryGetRemoteStorage(string url, out IndexStorage remoteStorage)
    {
      // ToDo: Complete this
      remoteStorage = null;
      return false;
    }

    /// <summary>
    /// Shares the storage by remoting.
    /// </summary>
    /// <param name="localStorage">The local storage.</param>
    /// <param name="url">The URL.</param>
    /// <param name="port">The port.</param>
    protected void MarshalStorage(IndexStorage localStorage, string url, int port)
    {
      // ToDo: Complete this
    }

    /// <summary>
    /// Creates the local index storage.
    /// </summary>
    /// <param name="name">The name of storage.</param>
    /// <returns>Newly created index storage.</returns>
    protected abstract IndexStorage CreateLocalStorage(string name);

    #endregion

    private static IEnumerable<StorageIndexInfo> GetAllIndexes(TableInfo table)
    {
      yield return table.PrimaryIndex;
      foreach (var indexInfo in table.SecondaryIndexes)
        yield return indexInfo;
    }

    internal MapTransform GetTransform(IndexInfo indexInfo, TypeInfo type)
    {
      return primaryIndexTransforms[new Pair<IndexInfo, TypeInfo>(indexInfo, type)];
    }

    internal IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfoRef indexInfoRef)
    {
      return Storage.GetRealIndex(ConvertIndexInfo(indexInfoRef));
    }
    
  }
}