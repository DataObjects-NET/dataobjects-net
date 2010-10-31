// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Model;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Orm.Building;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Indexing.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.PreCompilation;
using Xtensive.Storage.Rse.PreCompilation.Correction;
using Xtensive.Storage.Rse.PreCompilation.Correction.ApplyProviderCorrection;
using Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection;
using IndexInfo = Xtensive.Orm.Model.IndexInfo;
using PF = Xtensive.Storage.Providers.ProviderFeatures;
using StorageIndexInfo = Xtensive.Storage.Model.IndexInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;


namespace Xtensive.Storage.Providers.Indexing
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
    public IndexStorage Storage { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    public override void BuildMapping()
    {
      var model = BuildingContext.Demand().Model;

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
    /// Gets the <see cref="Xtensive.Orm.Model.IndexInfo"/>
    /// by <see cref="IndexInfoRef"/>. 
    /// </summary>
    /// <param name="indexInfoRef">The index info reference.</param>
    /// <returns>Converted index info.</returns>
    public StorageIndexInfo GetStorageIndexInfo(IndexInfoRef indexInfoRef)
    {
      var indexInfo = indexInfoRef.Resolve(Domain.Model);
      return indexInfoMapping[indexInfo];
    }

    /// <summary>
    /// Gets the <see cref="StorageIndexInfo"/> by <see cref="IndexInfo"/>. 
    /// </summary>
    /// <param name="indexInfo">The index info.</param>
    /// <returns>Converted index info.</returns>
    public StorageIndexInfo GetStorageIndexInfo(IndexInfo indexInfo)
    {
      return indexInfoMapping[indexInfo];
    }

    /// <inheritdoc/>
    protected override ProviderInfo CreateProviderInfo()
    {
      // We extracted this method to a separate class for tests.
      // It's very desirable to have a way of getting ProviderInfo without building a domain.
      return ProviderInfoBuilder.Build();
    }

    #region Local & remote storage initialization methods

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

    #region Compilation-related methods

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
        new ApplyProviderCorrector(false),
        new OrderingCorrector(DefaultCompilationService.ResolveOrderingDescriptor, false),
        new IndexOptimizer(Handlers.Domain.Model, new OptimizationInfoProviderResolver(this)),
        new OrderingCorrector(DefaultCompilationService.ResolveOrderingDescriptor, true)
        );
    }

    /// <inheritdoc/>
    protected override IPostCompiler CreatePostCompiler(ICompiler compiler)
    {
      return new EmptyPostCompiler();
    }

    #endregion

    #region Internal \ private methods

    internal MapTransform GetTransform(IndexInfo indexInfo, TypeInfo type)
    {
      return primaryIndexTransforms[new Pair<IndexInfo, TypeInfo>(indexInfo, type)];
    }

    #endregion

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      var connectionInfo = BuildingContext.Demand().Configuration.ConnectionInfo;
      if (connectionInfo.ConnectionUrl==null)
        throw new NotImplementedException(Strings.ExIndexingStoragesSupportOnlyConnectionUrls);
      var remoteUrl = connectionInfo.ConnectionUrl;
      var remoteUrlString = remoteUrl.ToString(); // TODO: Fix this
      IndexStorage storage;
      if (!TryGetRemoteStorage(remoteUrlString, out storage)) {
        storage = CreateLocalStorage(remoteUrl.Resource);
        MarshalStorage(storage, remoteUrlString, remoteUrl.Port);
      }
      Storage = storage;
      StorageLocation = Location.FromUrl(remoteUrl);
    }
  }
}