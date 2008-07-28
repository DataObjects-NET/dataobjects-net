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
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  [Storage]
  public class Session : ConfigurableBase<SessionConfiguration>,
    IResource,
    IContext<SessionScope>
  {
    private readonly Set<object> consumers = new Set<object>();
    private WeakCache<Key, EntityData> identityMap;
    private readonly FlagRegistry<PersistenceState, EntityData> dirtyData = new FlagRegistry<PersistenceState, EntityData>(data => data.PersistenceState);

    /// <summary>
    /// Gets the <see cref="Domain"/> to which this instance belongs.
    /// </summary>
    [DebuggerHidden]
    public Domain Domain
    {
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
      if (DirtyData.GetCount()==0)
        return;

      Handler.Persist(DirtyData);

      HashSet<EntityData> @new = DirtyData.GetItems(PersistenceState.New);
      HashSet<EntityData> modified = DirtyData.GetItems(PersistenceState.Modified);
      HashSet<EntityData> removed = DirtyData.GetItems(PersistenceState.Removed);

      foreach (EntityData data in @new.Union(modified).Except(removed))
        data.PersistenceState = PersistenceState.Persisted;

      DirtyData.Clear();
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
        Key key = ProcessFetched(type.Hierarchy, tuple);
        T item = (T)key.Resolve();
        if (item == null)
          throw new InvalidOperationException();
        yield return item;
      }
    }

    #region Private \ internal members

    internal Key ProcessFetched(HierarchyInfo hierarchy, Tuple tuple)
    {
      Tuple t = Tuple.Create(hierarchy.TupleDescriptor);
      tuple.CopyTo(t, 0, t.Count);
      Key key = new Key(hierarchy, t);
      EntityData data = IdentityMap[key, false];
      if (data != null)
        data.Tuple.Origin.MergeWith(tuple);
      else {
        data = new EntityData(key, new DifferentialTuple(tuple));
        IdentityMap.Add(data);
      }
      return key;
    }

    [DebuggerHidden]
    internal HandlerAccessor HandlerAccessor {
      get;
      private set; 
    }

    [DebuggerHidden]
    internal WeakCache<Key, EntityData> IdentityMap
    {
      get { return identityMap; }
    }

    [DebuggerHidden]
    internal SessionHandler Handler
    {
      get;
      set;
    }

    [DebuggerHidden]
    internal FlagRegistry<PersistenceState, EntityData> DirtyData
    {
      get { return dirtyData; }
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
      get { return SessionScope.Current==null ? null : SessionScope.Current.Session; }
    }

    /// <inheritdoc/>
    public SessionScope Activate()
    {
      if (IsActive)
        return null;
      return new SessionScope(this);
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public bool IsActive
    {
      get { return SessionScope.Current.Session==this; }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      identityMap = new WeakCache<Key, EntityData>(Configuration.CacheSize, item => item.Key);
    }


    // Constructors

    internal Session(HandlerAccessor handlerAccessor, SessionConfiguration configuration)
    {
      HandlerAccessor = handlerAccessor;
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
