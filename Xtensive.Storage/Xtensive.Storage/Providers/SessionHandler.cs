// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : InitializableHandlerBase,
    IDisposable, IHasServices
  {
    private static readonly object CachingRegion = new object();

    private PrefetchManager prefetchManager;

    /// <summary>
    /// The <see cref="object"/> to synchronize access to a connection.
    /// </summary>
    protected readonly object connectionSyncRoot = new object();

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    /// <summary>
    /// Gets the query provider.
    /// </summary>
    public virtual QueryProvider Provider {get { return QueryProvider.Instance; }}

    /// <summary>
    /// Gets a value indicating whether transaction is actually started.
    /// </summary>
    public abstract bool TransactionIsStarted { get; }
    
    internal virtual int PrefetchTaskExecutionCount { get { return prefetchManager.TaskExecutionCount;} }

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction();

    /// <summary>
    /// Commits the transaction.
    /// </summary>    
    public virtual void CommitTransaction()
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>    
    public virtual void RollbackTransaction()
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Acquires the connection lock.
    /// </summary>
    /// <returns>An implementation of <see cref="IDisposable"/> which should be disposed 
    /// to release the connection lock.</returns>
    public IDisposable AcquireConnectionLock()
    {
      Monitor.Enter(connectionSyncRoot);
      return new Disposable<object>(connectionSyncRoot, (disposing, syncRoot) => Monitor.Exit(syncRoot));
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      prefetchManager = new PrefetchManager(Session);

      PersistRequiresTopologicalSort =
        (Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0 &&
         Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints) &&
        !Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.DeferrableConstraints);
    }

    /// <inheritdoc/>
    public abstract void Dispose();
    
    public Dictionary<Key, VersionInfo> GetActualVersions(IEnumerable<Key> keys)
    {
      var versions = new Dictionary<Key, VersionInfo>();
      foreach (var key in keys) {
        var type = key.TypeRef.Type;
        if (type.VersionExtractor==null)
          versions[key] = new VersionInfo();
        else {
          var tuple = type.Indexes.PrimaryIndex
            .ToRecordSet()
            .Seek(key.Value)
            .FirstOrDefault();
          if (tuple!=null) {
            var versionTuple = type.VersionExtractor.Apply(TupleTransformType.Tuple, tuple);
            versions[key] = new VersionInfo(versionTuple);
          }
        }
      }
      return versions;
    }

    /// <summary>
    /// Gets the key generator.
    /// </summary>
    /// <param name="keyProviderInfo">The key provider info.</param>
    public virtual KeyGenerator GetKeyGenerator(KeyProviderInfo keyProviderInfo)
    {
      return null;
    }

    /// <summary>
    /// Creates enumeration context.
    /// </summary>
    /// <returns>Created context.</returns>
    public virtual Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      return new EnumerationContext(GetEnumerationContextOptions());
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public virtual void ExecuteQueryTasks(IList<QueryTask> queryTasks, bool allowPartialExecution)
    {
      foreach (var task in queryTasks) {
        using (EnumerationScope.Open())
        using (task.ParameterContext.ActivateSafely())
          task.Result = task.DataSource.ToList();
      }
    }

    /// <summary>
    /// Register the task prefetching fields' values of the <see cref="Entity"/> with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="type">The type of the <see cref="Entity"/>.</param>
    /// <param name="descriptors">The descriptors of fields which values will be loaded.</param>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public virtual StrongReferenceContainer Prefetch(Key key, TypeInfo type,
      params PrefetchFieldDescriptor[] descriptors)
    {
      return prefetchManager.Prefetch(key, type, descriptors);
    }

    /// <summary>
    /// Executes registered prefetch tasks.
    /// </summary>
    /// <returns>A <see cref="StrongReferenceContainer"/> which can be used to save 
    /// a strong reference to a fetched <see cref="Entity"/>.</returns>
    public virtual StrongReferenceContainer ExecutePrefetchTasks()
    {
      return prefetchManager.ExecuteTasks();
    }

    /// <summary>
    /// Gets the enumeration context options.
    /// </summary>
    /// <returns>Options for new enumeration context.</returns>
    protected virtual EnumerationContextOptions GetEnumerationContextOptions()
    {
      var options = EnumerationContextOptions.Default;
      if (!Handlers.DomainHandler.ProviderInfo.Supports(ProviderFeatures.MultipleActiveResultSets))
        options |= EnumerationContextOptions.PreloadEnumerator;
      return options;
    }

    /// <summary>
    /// Updates the state of the <see cref="EntitySet{TItem}"/>.
    /// </summary>
    /// <param name="key">The owner's key.</param>
    /// <param name="fieldInfo">The referencing field.</param>
    /// <param name="items">The items.</param>
    /// <param name="isFullyLoaded">if set to <see langword="true"/> then <paramref name="items"/> 
    /// contains all elements of an <see cref="EntitySet{TItem}"/>.</param>
    /// <returns>
    /// The updated <see cref="EntitySetState"/>, or <see langword="null"/>
    /// if a state was not found.
    /// </returns>
    protected EntitySetState UpdateEntitySetState(Key key, FieldInfo fieldInfo, IEnumerable<Key> items,
      bool isFullyLoaded)
    {
      var entityState = Session.EntityStateCache[key, true];
      if (entityState==null)
        return null;
      var entity = entityState.Entity;
      if (entity==null)
        return null;
      var entitySet = entity.GetFieldValue<EntitySetBase>(fieldInfo);
      return entitySet.UpdateState(items, isFullyLoaded);
    }

    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    protected internal virtual EntityState FetchInstance(Key key)
    {
      var type = key.TypeRef.Type;
      prefetchManager.Prefetch(key, type,
        PrefetchHelper.GetCachedDescriptorsForFieldsLoadedByDefault(Session.Domain, type));
      prefetchManager.ExecuteTasks();
      EntityState result;
      return TryGetEntityState(key, out result) ? result : null;
    }

    /// <summary>
    /// Fetches the field of an <see cref="Entity"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field to fetch.</param>
    protected internal virtual void FetchField(Key key, FieldInfo field)
    {
      var type = key.TypeRef.Type;
      prefetchManager.Prefetch(key, type, new PrefetchFieldDescriptor(field, false, false));
      prefetchManager.ExecuteTasks();
    }

    internal virtual EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      return Session.UpdateEntityState(key, tuple);
    }

    internal virtual bool TryGetEntityState(Key key, out EntityState entityState)
    {
      return Session.EntityStateCache.TryGetItem(key, true, out entityState);
    }

    internal virtual EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo, bool isFullyLoaded, 
      List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      if (Session.EntityStateCache[key, false]==null)
        return null;
      return UpdateEntitySetState(key, fieldInfo, entityKeys, isFullyLoaded);
    }

    internal virtual bool TryGetEntitySetState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      var entityState = Session.EntityStateCache[key, false];
      if (entityState!=null) {
        var entity = entityState.Entity;
        if (entity!=null) {
          var entitySet = entity.GetFieldValue<EntitySetBase>(fieldInfo);
          entitySetState = entitySet.GetState();
          return entitySetState!=null;
        }
      }
      entitySetState = null;
      return false;
    }

    /// <summary>
    /// Gets the references to specified entity.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="association">The association.</param>
    /// <returns>References.</returns>
    protected internal virtual IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      var result = new List<ReferenceInfo>();
      using (new ParameterContext().Activate()) {
        var pair = (Pair<RecordSet, Parameter<Tuple>>)Session.Domain.GetCachedItem(
          new Pair<object, AssociationInfo>(CachingRegion, association),
          p => BuildReferencingQuery(((Pair<object, AssociationInfo>)p).Second));
        var recordSet = pair.First;
        var parameter = pair.Second;
        parameter.Value = target.Key.Value;
        foreach (var item in recordSet.ToEntities(0))
          result.Add(new ReferenceInfo(item, target, association));
      }
      return result;

    }

    private static Pair<RecordSet,Parameter<Tuple>> BuildReferencingQuery(AssociationInfo association)
    {
      var recordSet = (RecordSet)null;
      var parameter = new Parameter<Tuple>();
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.ManyToOne: {
          var index = association.OwnerType.Indexes.PrimaryIndex;
          recordSet = index.ToRecordSet()
            .Filter(QueryHelper.BuildFilterLambda(
              association.OwnerField.MappingInfo.Offset,
              association.OwnerField.Columns.Select(c => c.ValueType).ToList(),
              parameter));
          break;
        }
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany: {
          var index = association.OwnerType.Indexes.PrimaryIndex;
          var targetIndex = association.TargetType.Indexes.PrimaryIndex;
          recordSet = targetIndex.ToRecordSet()
            .Filter(QueryHelper.BuildFilterLambda(0,
              association.TargetType.KeyProviderInfo.TupleDescriptor,
              parameter))
            .Alias("a")
            .Join(
              index.ToRecordSet(), 
              JoinAlgorithm.Loop, 
              association.Reversed.OwnerField.MappingInfo
                .GetItems()
                .Select((l,r) => new Pair<int>(l,r))
                .ToArray())
            .Select(Enumerable.Range(targetIndex.Columns.Count, index.Columns.Count).ToArray());
          break;
        }
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany: {
          var referencedType = association.IsMaster
            ? association.OwnerType
            : association.TargetType;
          var referencingType = association.IsMaster
            ? association.TargetType
            : association.OwnerType;
          var index = referencedType.Indexes.PrimaryIndex;
          var targetIndex = association.AuxiliaryType.Indexes.PrimaryIndex;
          var referencingField = association.IsMaster
            ? association.AuxiliaryType.Fields[WellKnown.SlaveFieldName]
            : association.AuxiliaryType.Fields[WellKnown.MasterFieldName];
          var referencedField = association.IsMaster
            ? association.AuxiliaryType.Fields[WellKnown.MasterFieldName]
            : association.AuxiliaryType.Fields[WellKnown.SlaveFieldName];
          recordSet = targetIndex.ToRecordSet()
            .Filter(QueryHelper.BuildFilterLambda(
              referencingField.MappingInfo.Offset,
              referencingType.KeyProviderInfo.TupleDescriptor,
              parameter))
            .Alias("a")
            .Join(
              index.ToRecordSet(),
              JoinAlgorithm.Loop,
              referencedField.MappingInfo
                .GetItems()
                .Select((l, r) => new Pair<int>(l, r))
                .ToArray())
            .Select(Enumerable.Range(targetIndex.Columns.Count, index.Columns.Count).ToArray());
          break;
        }
      }
      return new Pair<RecordSet, Parameter<Tuple>>(recordSet, parameter);
    }

    /// <summary>
    /// Gets the references from specified entity.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="association">The association.</param>
    /// <returns>References.</returns>
    protected internal virtual IEnumerable<ReferenceInfo> GetReferencesFrom(Entity owner, AssociationInfo association)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.OneToOne:
        case Multiplicity.ManyToOne:
          var target = owner.GetFieldValue<IEntity>(association.OwnerField);
          if (target != null)
            yield return new ReferenceInfo(owner, (Entity) target, association);
          break;
        case Multiplicity.ZeroToMany:
        case Multiplicity.OneToMany:
        case Multiplicity.ManyToMany:
          var targets = owner.GetFieldValue<EntitySetBase>(association.OwnerField);
          foreach (var item in targets.Entities)
            yield return new ReferenceInfo(owner, (Entity) item, association);
          break;
      }
    }

    #region IHasServices members

    public virtual T GetService<T>()
      where T : class
    {
      var result = this as T;
      if (result==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExServiceXIsNotSupported, typeof (T).GetFullName()));
      return result;
    }

    #endregion
  }
}