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
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Index.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.PreCompilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.PreCompilation.Correction.ApplyProviderCorrection;
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;
using IndexInfo=Xtensive.Storage.Model.IndexInfo;
using StorageIndexInfo = Xtensive.Storage.Indexing.Model.IndexInfo;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;
using PF = Xtensive.Storage.Providers.ProviderFeatures;


namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// <see cref="Domain"/>-level handler for index storage.
  /// </summary>
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
    protected override IEnumerable<Type> GetCompilerProviderContainerTypes()
    {
      return Type.EmptyTypes;
    }

    /// <inheritdoc/>
    protected override ICompiler CreateCompiler()
    {
      return new IndexCompiler(Handlers);
    }

    /// <inheritdoc/>
    protected override IPreCompiler CreatePreCompiler()
    {
      return new CompositePreCompiler(
        new IncludeOnIndexCorrector(),
        new ApplyProviderCorrector(false),
        new OrderingCorrector(DefaultCompilationContext.ResolveOrderingDescriptor, false),
        new IndexOptimizer(Handlers.Domain.Model, new OptimizationInfoProviderResolver(this)),
//        new IndexRedundantColumnOptimizer(),
 //       new StoreRedundantColumnOptimizer(),
        new OrderingCorrector(DefaultCompilationContext.ResolveOrderingDescriptor, true)
        );
    }

    /// <inheritdoc/>
    protected override IPostCompiler CreatePostCompiler(ICompiler compiler)
    {
      return new EmptyPostCompiler();
    }

    /// <inheritdoc/>
    /// <exception cref="DomainBuilderException">Somethig went wrong.</exception>
    public override void BuildMapping()
    {
      var model = BuildingContext.Current.Model;

      // Build index transforms
      foreach (var type in model.Types) {
        foreach (var indexInfo in type.AffectedIndexes.Where(index => index.IsPrimary)) {
          var primaryIndex = type.Indexes.PrimaryIndex;
          var primaryIndexColumns = primaryIndex.Columns.ToList();
          var indexColumns = indexInfo.Columns.ToList();
          var typeColumns = type.Columns.ToList();
          var map = indexColumns
            .Select(column => 
              typeColumns.Contains(column) 
              ? primaryIndexColumns.IndexOf(column) 
              : MapTransform.NoMapping)
            .ToArray();
          primaryIndexTransforms.Add(
            new Pair<IndexInfo, TypeInfo>(indexInfo, type),
            new MapTransform(true, indexInfo.TupleDescriptor, map));
        }
      }

      // Build IndexInfo mapping
      foreach (var indexInfo in model.RealIndexes) {
        var storageIndexInfo = (
          from t in Storage.Model.Tables
          from i in t.AllIndexes
          where i.Name==indexInfo.MappingName
          select i
          ).SingleOrDefault();
        if (storageIndexInfo==null)
          throw new DomainBuilderException(string.Format(
            Strings.ExCanNotFindIndexXInStorage, indexInfo.MappingName));
        indexInfoMapping.Add(indexInfo, storageIndexInfo);
      }
    }

    /// <summary>
    /// Gets the <see cref="Indexing.Model.IndexInfo"/>
    /// by <see cref="IndexInfoRef"/>. 
    /// </summary>
    /// <param name="indexInfoRef">The index info.</param>
    /// <returns>Converted index info.</returns>
    public StorageIndexInfo GetStorageIndexInfo(IndexInfoRef indexInfoRef)
    {
      var indexInfo = indexInfoRef.Resolve(Domain.Model);
      return indexInfoMapping[indexInfo];
    }

    /// <inheritdoc/>
    protected override ProviderInfo CreateProviderInfo()
    {
      const ProviderFeatures features = PF.Batches | PF.ClusteredIndexes | PF.IncludedColumns | PF.KeyColumnSortOrder | PF.Paging | PF.FullFledgedBooleanExpressions | PF.CrossApply;
      return new ProviderInfo(new Version(0, 3), features, int.MaxValue);
    }

    #region Storage access methods

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
      // TODO: Complete this
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
      // TODO: Complete this
    }

    /// <summary>
    /// Creates the local index storage.
    /// </summary>
    /// <param name="name">The name of storage.</param>
    /// <returns>Newly created index storage.</returns>
    protected abstract IndexStorage CreateLocalStorage(string name);

    #endregion

    #region Internal \ private methods

    internal MapTransform GetTransform(IndexInfo indexInfo, TypeInfo type)
    {
      return primaryIndexTransforms[new Pair<IndexInfo, TypeInfo>(indexInfo, type)];
    }

    internal IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfoRef indexInfoRef)
    {
      // TODO: Replace with StorageView.GetIndex
      return Storage.GetRealIndex(GetStorageIndexInfo(indexInfoRef));
    }

    #endregion

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      var connectionInfo = BuildingContext.Current.Configuration.ConnectionInfo;
      var remoteUrl = connectionInfo.ToString(); // TODO: Fix this
      IndexStorage storage;
      if (!TryGetRemoteStorage(remoteUrl, out storage)) {
        storage = CreateLocalStorage(connectionInfo.Resource);
        MarshalStorage(storage, remoteUrl, connectionInfo.Port);
      }
      Storage = storage;
    }
  }
}