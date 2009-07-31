// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
    private static ThreadSafeDictionary<FetchRequest, RecordSet> recordSetCache;
    private static readonly Parameter<Tuple> fetchParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    ///<summary>
    /// Gets the specified <see cref="IsolationLevel"/>.
    ///</summary>
    public IsolationLevel DefaultIsolationLevel { get; internal set; }

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
    /// Persists changed entities.
    /// </summary>
    /// <param name="persistActions">The entity states and the corresponding actions.</param>
    public abstract void Persist(IEnumerable<PersistAction> persistActions);

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    /// <inheritdoc/>
    public abstract void Dispose();

    #region Fetch methods

    protected internal EntityState FetchInstance(Key key)
    {
      // We should fetch all non-lazyload columns
      var index = GetPrimaryIndex(key);
      var request = new FetchRequest(index, index.GetDefaultFetchColumnsIndexes());

      // Key could be with or without exact TypeId
      return Execute(request, key);
    }

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

      var request = new FetchRequest(index, columns);

      // Key always contains exact TypeId
      Execute(request, key);
    }

    private EntityState Execute(FetchRequest request, Key key)
    {
      var recordSet = GetRecordSet(request);

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

    private static RecordSet GetRecordSet(FetchRequest request)
    {
      return recordSetCache.GetValue(request, delegate {
        return IndexProvider.Get(request.Index).Result
          .Seek(() => fetchParameter.Value)
          .Select(request.Columns);
      });
    }

    #endregion

    protected internal virtual QueryProvider Provider {get { return QueryProvider.Instance; }}

    protected internal virtual IEnumerable<T> Execute<T>(Expression expression)
    {
      return Provider.Execute<IEnumerable<T>>(expression);
    }

    protected internal virtual TranslatedQuery<IEnumerable<T>> Translate<T>(Expression expression)
    {
      return Provider.Translate<IEnumerable<T>>(expression);
    }

    static SessionHandler()
    {
      recordSetCache = ThreadSafeDictionary<FetchRequest, RecordSet>.Create(new object());
    }
  }
}