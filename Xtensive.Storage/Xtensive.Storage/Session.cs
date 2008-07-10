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
    private readonly FlagRegistry<PersistenceState, EntityData> dirty = new FlagRegistry<PersistenceState, EntityData>(data => data.PersistenceState);

    /// <summary>
    /// Gets the <see cref="Domain"/> to which this instance belongs.
    /// </summary>
    [DebuggerHidden]
    public Domain Domain {
      [SuppressContextActivation(typeof (Session))]
      get;
      [SuppressContextActivation(typeof (Session))]
      private set; 
    }

    internal WeakCache<Key, EntityData> IdentityMap
    {
      get { return identityMap; }
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
      if (dirty.GetCount()==0)
        return;

      Handler.Persist(dirty);

      HashSet<EntityData> @new = dirty.GetItems(PersistenceState.New);
      HashSet<EntityData> modified = dirty.GetItems(PersistenceState.Modified);
      HashSet<EntityData> removed = dirty.GetItems(PersistenceState.Removed);

      HashSet<EntityData> persisted = new HashSet<EntityData>(@new.Union(modified).Except(removed));
      foreach (EntityData data in persisted)
        data.PersistenceState = PersistenceState.Persisted;

      dirty.Clear();
    }

    public RecordSet QueryIndex(IndexInfo indexInfo)
    {
      Persist();
      return Handler.QueryIndex(indexInfo);
    }

    public IEnumerable<T> All<T>() where T : Entity
    {
      Persist();
      TypeInfo type = Domain.Model.Types[typeof (T)];
      foreach (Tuple tuple in Handler.Select(type)) {
        Key key = Domain.KeyManager.BuildPrimaryKey(type.Hierarchy, tuple);
        T item = key.Resolve<T>();
        if (item == null)
          throw new InvalidOperationException();
        yield return item;
      }
    }

    #region Internals & private

    internal void RegisterDirty(Entity entity)
    {
      if (entity.PersistenceState == PersistenceState.New)
        IdentityMap.Add(entity.Data);
      dirty.Register(entity.Data);
    }

    [DebuggerHidden]
    internal SessionHandler Handler
    {
      [SuppressContextActivation(typeof (Session))]
      get;
      [SuppressContextActivation(typeof (Session))]
      set;
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
      [SuppressContextActivation(typeof (Session))]
      get { return SessionScope.Current==null ? null : SessionScope.Current.Session; }
    }

    /// <inheritdoc/>
    [SuppressContextActivation(typeof (Session))]
    public SessionScope Activate()
    {
      if (IsActive)
        return null;
      return new SessionScope(this);
    }

    /// <inheritdoc/>
    [SuppressContextActivation(typeof (Session))]
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public bool IsActive
    {
      [SuppressContextActivation(typeof (Session))]
      get { return SessionScope.Current.Session==this; }
    }

    #endregion

    #region IContextBound<Session> Members

    /// <inheritdoc/>
    public Session Context
    {
      [SuppressContextActivation(typeof (Session))]
      get { return this; }
    }

    #endregion

    /// <inheritdoc/>
    [SuppressContextActivation(typeof (Session))]
    protected override void OnConfigured()
    {
      base.OnConfigured();
      identityMap = new WeakCache<Key, EntityData>(Configuration.CacheSize, item => item.Key);
    }


    // Constructors

    internal Session(Domain domain)
    {
      Domain = domain;
    }

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    public void Dispose()
    {
      Persist();
      Handler.Commit();
    }
  }
}
