// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Aspects;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  public sealed class EntitySetState : TransactionalStateContainer<EntitySetStateCache>,
    IEnumerable<Key>,
    IHasVersion<long>
  {
    private const int CacheSize = 10240;
    private readonly Func<long> countLoader;

    #region IHasVersion<...> methods

    /// <inheritdoc/>
    [Infrastructure]
    public long Version { get; private set; }

    /// <inheritdoc/>
    object IHasVersion.Version
    {
      get { return Version; }
    }


    #endregion

    [Infrastructure]
    public bool IsFullyLoaded {
      get {
        var state = State;
        return state.Count.HasValue && 
          state.Count.GetValueOrDefault()==state.ExistingKeys.Count;
      }
    }

    [Infrastructure]
    public long Count {
      get {
        var state = State;
        if (!state.Count.HasValue)
          state.Count = countLoader.Invoke();
        return state.Count.GetValueOrDefault();
      }
    }

    public bool Contains(Key key)
    {
      return State.ExistingKeys.ContainsKey(key);
    }

    public void Cache(Key key)
    {
      State.ExistingKeys.Add(key);
    }

    public void Add(Key key)
    {
      Cache(key);
      var state = State;
      if (state.Count.HasValue)
        state.Count++;
      Version++;
    }

    public void Remove(Key key)
    {
      var state = State;
      state.ExistingKeys.RemoveKey(key);
      if (state.Count.HasValue)
        state.Count--;
      Version++;
    }

    public void Clear()
    {
      State.ExistingKeys.Clear();
      State.Count = 0;
      Version++;
    }

    /// <inheritdoc/>
    protected override EntitySetStateCache LoadState()
    {
      return new EntitySetStateCache(CacheSize);
    }

    #region GetEnumerator<...> methods

    public IEnumerator<Key> GetEnumerator()
    {
      return State.ExistingKeys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructor

    /// <inheritdoc/>
    /// <param name="countLoader">The load count.</param>
    public EntitySetState(Session session, Func<long> countLoader)
      : base(session)
    {
      this.countLoader = countLoader;
    }
  }
}