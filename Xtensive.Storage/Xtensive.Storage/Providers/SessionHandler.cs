// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Providers
{
  public abstract class SessionHandler
  {
    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    protected abstract void Insert(EntityData data);
    protected abstract void Update(EntityData data);
    protected abstract void Remove(EntityData data);
    public abstract Tuple Fetch(Key key, IEnumerable<ColumnInfo> columns);

    public IEnumerable Select(TypeInfo type)
    {
      return Select(type, type.Columns.Where(columnInfo => !columnInfo.LazyLoad));
    }

    public abstract IEnumerable<Tuple> Select(TypeInfo type, IEnumerable<ColumnInfo> columns);

    public abstract RecordSet QueryIndex(IndexInfo info);

    public virtual void Commit()
    {
    }

    public Tuple Fetch(Key key)
    {
      return Fetch(key, key.Type.Indexes.PrimaryIndex.Columns.Where(columnInfo => !columnInfo.LazyLoad));
    }

    public virtual Tuple FetchKey(Key key)
    {
      return Fetch(key, key.Type.Indexes.PrimaryIndex.Columns.Where(columnInfo => columnInfo.IsPrimaryKey));
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
  }
}