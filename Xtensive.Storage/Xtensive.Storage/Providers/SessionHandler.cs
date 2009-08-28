// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Transactions;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Parameters;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
    private static ThreadSafeDictionary<FetchTask, RecordSet> recordSetCache;
    private static readonly Parameter<Tuple> fetchParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);

    /// <summary>
    /// The <see cref="object"/> to synchronize access to a connection.
    /// </summary>
    protected readonly object ConnectionSyncRoot = new object();

    /// <summary>
    /// Determines whether an auto-shortened transaction is activated.
    /// </summary>
    protected bool IsAutoshortenTransactionActivated;

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    ///<summary>
    /// Gets the specified <see cref="IsolationLevel"/>.
    ///</summary>
    public IsolationLevel DefaultIsolationLevel { get; internal set; }

    /// <summary>
    /// Gets the query provider.
    /// </summary>
    public virtual QueryProvider Provider {get { return QueryProvider.Instance; }}

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction();

    /// <summary>
    /// Commits the transaction.
    /// </summary>    
    public abstract void CommitTransaction();

    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>    
    public abstract void RollbackTransaction();

    /// <summary>
    /// Acquires the connection lock.
    /// </summary>
    /// <returns>An implementation of <see cref="IDisposable"/> which should be disposed 
    /// to release the connection lock.</returns>
    public IDisposable AcquireConnectionLock()
    {
      Monitor.Enter(ConnectionSyncRoot);
      return new Disposable<object>(ConnectionSyncRoot, (disposing, syncRoot) => Monitor.Exit(syncRoot));
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      PersistRequiresTopologicalSort =
        (Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0 &&
         Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints) &&
        !Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.DeferrableConstraints);
    }

    /// <inheritdoc/>
    public abstract void Dispose();
    
    /// <summary>
    /// Executes the specified compiled RSE query.
    /// This method is used only for non-index storages.
    /// </summary>
    /// <param name="provider">The provider to execute.</param>
    /// <returns>Result of query execution.</returns>
    public virtual IEnumerator<Tuple> Execute(ExecutableProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="dirty">if set to <see langword="true"/> dirty execution is allowed.</param>
    public virtual void Execute(IList<QueryTask> queryTasks, bool dirty)
    {
      foreach (var task in queryTasks) {
        using (task.ParameterContext.ActivateSafely())
        using (EnumerationScope.Open())
          task.Result = task.DataSource.ToList();
      }
    }
    
    #region Fetch methods
    
    /// <summary>
    /// Fetches an <see cref="EntityState"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The key of fetched <see cref="EntityState"/>.</returns>
    protected internal EntityState FetchInstance(Key key)
    {
      // We should fetch all non-lazyload columns
      var index = GetPrimaryIndex(key);
      var request = new FetchTask(index, index.GetDefaultFetchColumnsIndexes());

      // Key could be with or without exact TypeId
      return Execute(request, key);
    }

    /// <summary>
    /// Fetches the field of an <see cref="Entity"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="field">The field to be fetched.</param>
    protected internal void FetchField(Key key, FieldInfo field)
    {
      // This method is being called only for fetching lazyload field. All non-lazyload fields are already fetched.
      // We should combine Key+TypeId columns with requested field column(s)
      var index = GetPrimaryIndex(key);
      int[] columns = index.GetKeyFetchColumnsIndexes();

      // Choosing most optimal array construction method
      if (field.MappingInfo.Length == 1)
        columns = columns.Append(index.Columns.IndexOf(field.Column));
      else
        columns = columns.Combine(field.ExtractColumns().Select(c => index.Columns.IndexOf(c)).ToArray());

      var request = new FetchTask(index, columns);

      // Key always contains exact TypeId
      Execute(request, key);
    }

    protected bool IsAutoshortenTransactionsEnabled()
    {
      return (Session.Configuration.Options & SessionOptions.AutoShortenTransactions)
        ==SessionOptions.AutoShortenTransactions;
    }

    private EntityState Execute(FetchTask task, Key key)
    {
      var recordSet = GetRecordSet(task);

      using (new ParameterContext().Activate()) {
        fetchParameter.Value = key.Value;
        var reader = Session.Domain.RecordSetReader;
        var record = reader.ReadSingleRow(recordSet, key);
        if (record == null) {
          // Ensures there will be "removed" EntityState associated with this key
          Session.UpdateEntityState(key, null);
          return null;
        }
        var fetchedKey = record.GetKey();
        var tuple = record.GetTuple();
        if (tuple != null)
          return Session.UpdateEntityState(fetchedKey, tuple);
        return null;
      }
    }

    private static IndexInfo GetPrimaryIndex(Key key)
    {
      return (key.IsTypeCached ? key.Type : key.Hierarchy.Root).Indexes.PrimaryIndex;
    }

    private static RecordSet GetRecordSet(FetchTask task)
    {
      return recordSetCache.GetValue(task, delegate {
        return IndexProvider.Get(task.Index).Result
          .Seek(() => fetchParameter.Value)
          .Select(task.Columns);
      });
    }

    #endregion
    
    static SessionHandler()
    {
      recordSetCache = ThreadSafeDictionary<FetchTask, RecordSet>.Create(new object());
    }
  }
}