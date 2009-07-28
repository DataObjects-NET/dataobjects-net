// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Transactions;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
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

    protected internal virtual Key Fetch(Key key)
    {
      return Fetcher.Fetch(key);
    }

    protected internal virtual void FetchField(EntityState state, FieldInfo field)
    {
      Fetcher.Fetch(state.Key, field);
    }

    protected internal virtual QueryProvider Provider {get { return QueryProvider.Instance; }}

    protected internal virtual IEnumerable<T> Execute<T>(Expression expression)
    {
      return Provider.Execute<IEnumerable<T>>(expression);
    }

    protected internal virtual TranslatedQuery<IEnumerable<T>> Translate<T>(Expression expression)
    {
      return Provider.Translate<IEnumerable<T>>(expression);
    }
  }
}