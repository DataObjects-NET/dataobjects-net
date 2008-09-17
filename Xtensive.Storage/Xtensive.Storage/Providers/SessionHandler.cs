// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base session handler class.
  /// </summary>
  public abstract class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

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
    public void Persist()
    {
      foreach (EntityData data in Session.removedEntities)
        Remove(data);
      foreach (EntityData data in Session.newEntities)
        Insert(data);
      foreach (EntityData data in Session.modifiedEntities)
        Update(data);
    }

    /// <summary>
    /// Inserts the specified data into database.
    /// </summary>
    /// <param name="data">The data to insert.</param>
    protected abstract void Insert(EntityData data);

    /// <summary>
    /// Updates the specified data in database.
    /// </summary>
    /// <param name="data">The data to update.</param>
    protected abstract void Update(EntityData data);

    /// <summary>
    /// Removes the specified data from database.
    /// </summary>
    /// <param name="data">The data to remove.</param>
    protected abstract void Remove(EntityData data);

    /// <inheritdoc/>
    public override void Initialize()
    {}

    /// <inheritdoc/>
    public abstract void Dispose();
  }
}