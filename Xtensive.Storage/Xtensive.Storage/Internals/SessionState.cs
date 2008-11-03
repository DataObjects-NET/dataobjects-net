// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.03

using System;
using System.Collections.Generic;

namespace Xtensive.Storage.Internals
{
  internal class SessionState : SessionBound
  {
    private readonly List<EntityState> @new = new List<EntityState>();
    private readonly List<EntityState> modified = new List<EntityState>();
    private readonly List<EntityState> removed = new List<EntityState>();

    public void Register(EntityState item)
    {
      List<EntityState> container = GetContainer(item.PersistenceState);
      container.Add(item);
    }

    public IEnumerable<EntityState> GetItems(PersistenceState state)
    {
      foreach (var item in GetContainer(state))
        yield return item;
    }

    public void Clear()
    {
      foreach (var item in @new)
        item.PersistenceState = PersistenceState.Synchronized;
      foreach (var item in modified)
        item.PersistenceState = PersistenceState.Synchronized;
      foreach (var item in removed)
        item.PersistenceState = PersistenceState.Synchronized;

      @new.Clear();
      modified.Clear();
      removed.Clear();
    }

    private List<EntityState> GetContainer(PersistenceState state)
    {
      switch (state) {
      case PersistenceState.New:
        return @new;
      case PersistenceState.Modified:
        return modified;
      case PersistenceState.Removed:
        return removed;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }

    // Constructor

    public SessionState(Session session)
      : base(session)
    {
      
    }
  }
}