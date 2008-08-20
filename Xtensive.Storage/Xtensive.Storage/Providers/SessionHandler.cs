// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers
{
  public abstract class SessionHandler : InitializableHandlerBase
  {
    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    public virtual void Commit()
    {
    }

    public void Persist(FlagRegistry<PersistenceState, EntityData> registry)
    {
      HashSet<EntityData> @new = registry.GetItems(PersistenceState.New);
      HashSet<EntityData> modified = registry.GetItems(PersistenceState.Modified);
      HashSet<EntityData> removed = registry.GetItems(PersistenceState.Removed);

      foreach (EntityData data in @new)
        Insert(data);
      foreach (EntityData data in modified.Except(@new))
        Update(data);
      foreach (EntityData data in removed)
        Remove(data);
    }

    protected abstract void Insert(EntityData data);

    protected abstract void Update(EntityData data);

    protected abstract void Remove(EntityData data);
  }
}