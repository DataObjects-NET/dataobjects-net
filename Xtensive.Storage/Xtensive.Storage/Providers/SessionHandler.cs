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

    public abstract void Insert(EntityData data);
    public abstract void Update(EntityData data);
    public abstract void Remove(EntityData data);
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
      foreach (EntityData data in registry.GetItems(PersistenceState.New))
        Insert(data);
      foreach (EntityData data in registry.GetItems(PersistenceState.Modified))
        Update(data);
      foreach (EntityData data in registry.GetItems(PersistenceState.Removed))
        Remove(data);
    }
  }
}