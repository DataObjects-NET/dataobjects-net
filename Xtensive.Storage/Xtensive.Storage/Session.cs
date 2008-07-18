// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Aspects;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  [Storage]
  public class Session : ConfigurableBase<SessionConfiguration>,
    IResource,
    IContext<SessionScope>,
    IContextBound<Session>
  {
    private readonly Set<object> consumers = new Set<object>();
    private WeakCache<Key, EntityData> identityMap;
    private readonly FlagRegistry<PersistenceState, EntityData> dirtyItems = new FlagRegistry<PersistenceState, EntityData>(data => data.PersistenceState);

    /// <summary>
    /// Gets the <see cref="Domain"/> to which this instance belongs.
    /// </summary>
    [DebuggerHidden]
    public Domain Domain
    {
      [SuppressActivation(typeof (Session))]
      get { return HandlerAccessor.Domain; }
    }

    /// <summary>
    /// Persists all modified instances immediately.
    /// </summary>
    /// <remarks>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. Note, that this method 
    /// is called automatically when it's necessary - e.g.
    /// before beginning\committing\rolling back a transaction,
    /// establishing a save point or rolling back to it, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </remarks>
    public void Persist()
    {
      if (DirtyItems.GetCount()==0)
        return;

      Handler.Persist(DirtyItems);

      HashSet<EntityData> @new = DirtyItems.GetItems(PersistenceState.New);
      HashSet<EntityData> modified = DirtyItems.GetItems(PersistenceState.Modified);
      HashSet<EntityData> removed = DirtyItems.GetItems(PersistenceState.Removed);

      foreach (EntityData data in @new.Union(modified).Except(removed))
        data.PersistenceState = PersistenceState.Persisted;

      DirtyItems.Clear();
    }

    public RecordSet Select(IndexInfo index)
    {
      Persist();
      return Handler.Select(index);
    }

    public IEnumerable<T> All<T>() where T : Entity
    {
      Persist();
      TypeInfo type = Domain.Model.Types[typeof (T)];
      RecordSet result = Handler.Select(type.Indexes.PrimaryIndex);
      foreach (Tuple tuple in result) {
        Key key = Domain.KeyManager.BuildPrimaryKey(type.Hierarchy, tuple);
        T item = (T)key.Resolve(tuple);
        if (item == null)
          throw new InvalidOperationException();
        yield return item;
      }
    }

    #region Internals & private

    [DebuggerHidden]
    internal HandlerAccessor HandlerAccessor {
      [SuppressActivation(typeof (Session))]
      get;
      [SuppressActivation(typeof (Session))]
      private set; 
    }

    [DebuggerHidden]
    internal WeakCache<Key, EntityData> IdentityMap
    {
      [SuppressActivation(typeof (Session))]
      get { return identityMap; }
    }

    [DebuggerHidden]
    internal SessionHandler Handler
    {
      [SuppressActivation(typeof (Session))]
      get;
      [SuppressActivation(typeof (Session))]
      set;
    }

    [DebuggerHidden]
    internal FlagRegistry<PersistenceState, EntityData> DirtyItems
    {
      [SuppressActivation(typeof (Session))]
      get { return dirtyItems; }
    }

    #endregion

    #region IResource members

    /// <inheritdoc/>
    void IResource.AddConsumer(object consumer)
    {
      consumers.Add(consumer);
    }

    /// <inheritdoc/>
    void IResource.RemoveConsumer(object consumer)
    {
      consumers.Remove(consumer);
      if (!(this as IResource).HasConsumers)
        Dispose();
    }

    /// <inheritdoc/>
    bool IResource.HasConsumers
    {
      get { return consumers.Count > 0; }
    }

    #endregion

    #region IContext<SessionScope> Members

    /// <summary>
    /// Gets the current active <see cref="Session"/> instance.
    /// </summary>
    [DebuggerHidden]
    public static Session Current
    {
      [SuppressActivation(typeof (Session))]
      get { return SessionScope.Current==null ? null : SessionScope.Current.Session; }
    }

    /// <inheritdoc/>
    [SuppressActivation(typeof (Session))]
    public SessionScope Activate()
    {
      if (IsActive)
        return null;
      return new SessionScope(this);
    }

    /// <inheritdoc/>
    [SuppressActivation(typeof (Session))]
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public bool IsActive
    {
      [SuppressActivation(typeof (Session))]
      get { return SessionScope.Current.Session==this; }
    }

    #endregion

    #region IContextBound<Session> Members

    /// <inheritdoc/>
    public Session Context
    {
      [SuppressActivation(typeof (Session))]
      get { return this; }
    }

    #endregion

    /// <inheritdoc/>
    [SuppressActivation(typeof (Session))]
    protected override void OnConfigured()
    {
      base.OnConfigured();
      identityMap = new WeakCache<Key, EntityData>(Configuration.CacheSize, item => item.Key);
    }


    // Constructors

    internal Session(HandlerAccessor handlerAccessor, SessionHandler handler, SessionConfiguration configuration)
    {
      HandlerAccessor = handlerAccessor;
      Handler = handler;
      Configure(configuration);
    }

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    public void Dispose()
    {
      Persist();
      Handler.Commit();
    }
  }
}
