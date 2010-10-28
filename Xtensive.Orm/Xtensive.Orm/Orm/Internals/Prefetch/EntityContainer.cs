// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal abstract class EntityContainer
  {
    private static readonly object indexSeekCachingRegion = new object();
    private static readonly Parameter<Tuple> seekParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);

    private SortedDictionary<int, ColumnInfo> columns;

    protected readonly PrefetchManager Manager;

    public Key Key { get; protected set; }

    public readonly bool ExactType;

    public TypeInfo Type { get; protected set; }

    public EntityGroupTask Task { get; protected set; }

    protected List<int> ColumnIndexesToBeLoaded { get; set; }

    public abstract EntityGroupTask GetTask();

    public void AddColumns(IEnumerable<ColumnInfo> candidateColumns)
    {
      if (columns == null)
        columns = new SortedDictionary<int, ColumnInfo>();
      if (PrefetchHelper.AddColumns(candidateColumns, columns, Type) && ColumnIndexesToBeLoaded != null)
        ColumnIndexesToBeLoaded = null;
    }

    public void SetColumnCollections(SortedDictionary<int, ColumnInfo> forcedColumns,
      List<int> forcedColumnsToBeLoaded)
    {
      if (columns != null)
        throw new InvalidOperationException();
      columns = forcedColumns;
      ColumnIndexesToBeLoaded = forcedColumnsToBeLoaded;
    }

    protected bool SelectColumnsToBeLoaded()
    {
      var key = Key;
      EntityState state;
      if (!Manager.TryGetTupleOfNonRemovedEntity(ref key, out state))
        return false;
      var tuple = state == null ? null : state.Tuple;
      if (tuple == null && ColumnIndexesToBeLoaded != null)
        return true;
      if (ColumnIndexesToBeLoaded != null)
        ColumnIndexesToBeLoaded = null;
      var needToFetchSystemColumns = false;
      foreach (var pair in columns)
        if (tuple==null || !tuple.GetFieldState(pair.Key).IsAvailable())
          if (pair.Value.IsPrimaryKey || pair.Value.IsSystem)
            needToFetchSystemColumns = ExactType && tuple==null;
          else {
            if (ColumnIndexesToBeLoaded == null)
              ColumnIndexesToBeLoaded = CreateColumnIndexCollection();
            ColumnIndexesToBeLoaded.Add(pair.Key);
          }
      if (needToFetchSystemColumns && ColumnIndexesToBeLoaded == null)
        ColumnIndexesToBeLoaded = CreateColumnIndexCollection();
      return ColumnIndexesToBeLoaded != null;
    }

    private List<int> CreateColumnIndexCollection()
    {
      var result = new List<int>(columns.Count);
      result.AddRange(Type.Indexes.PrimaryIndex.ColumnIndexMap.System);
      return result;
    }


    // Constructors

    protected EntityContainer(Key key, TypeInfo type, bool exactType, PrefetchManager manager)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(manager, "processor");
      Key = key;
      Type = type;
      ExactType = exactType;
      Manager = manager;
    }
  }
}