// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal abstract class PrefetchTask
  {
    private static readonly object indexSeekCachingRegion = new object();
    private static readonly Parameter<Tuple> seekParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);

    private readonly SortedDictionary<int, ColumnInfo> columns = new SortedDictionary<int, ColumnInfo>();
    private List<int> columnIndexesToBeLoaded;
    private int? cachedHashCode;

    protected readonly PrefetchProcessor Processor;

    public Key Key { get; protected set; }

    public readonly bool ExactType;

    public RecordSet RecordSet { get; private set; }

    public abstract List<Tuple> Result { get; }

    public TypeInfo Type { get; protected set; }

    public abstract bool IsActive { get; }

    public void AddColumns(IEnumerable<ColumnInfo> candidateColumns)
    {
      foreach (var column in candidateColumns)
        columns[Type.Indexes.PrimaryIndex.Columns.IndexOf(column)] = column;
    }

    public abstract void RegisterQueryTask();

    public static bool IsFieldToBeLoadedByDefault(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem || !field.IsLazyLoad && !field.IsEntitySet;
    }

    public static bool IsFieldAvailable(Tuple tuple, int fieldIndex)
    {
      return (tuple.GetFieldState(fieldIndex) & TupleFieldState.Available)==TupleFieldState.Available;
    }

    public bool Equals(PrefetchTask other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      EnsureColumnIndexesIsSpecified();
      if (!Type.Equals(other.Type))
        return false;
      if (columnIndexesToBeLoaded.Count != other.columnIndexesToBeLoaded.Count)
        return false;
      for (int i = 0; i < other.columnIndexesToBeLoaded.Count; i++) {
        if (columnIndexesToBeLoaded[i] != other.columnIndexesToBeLoaded[i])
          return false;
      }
      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var otherType = obj.GetType();
      if (!otherType.IsSubclassOf(typeof (PrefetchTask)))
        return false;
      return Equals((PrefetchTask) obj);
    }

    public override int GetHashCode()
    {
      EnsureColumnIndexesIsSpecified();
      if (cachedHashCode==null) {
        cachedHashCode = 0;
        for (var i = 0; i < columnIndexesToBeLoaded.Count; i++)
          cachedHashCode = unchecked (379 * cachedHashCode.Value + columnIndexesToBeLoaded[i]);
        cachedHashCode = unchecked (cachedHashCode ^ Type.GetHashCode());
      }
      return cachedHashCode.Value;
    }

    protected QueryTask CreateQueryTask(Key entityKey)
    {
      var parameterContext = new ParameterContext();
      using (parameterContext.Activate()) {
        seekParameter.Value = entityKey.Value;
        RecordSet = (RecordSet) Processor.Owner.Session.Domain.GetCachedItem(
          new Pair<object, PrefetchTask>(indexSeekCachingRegion, this), CreateIndexSeekRecordSet);
        var executableProvider = CompilationContext.Current.Compile(RecordSet.Provider);
        return new QueryTask(executableProvider, parameterContext);
      }
    }

    protected bool TryActivate()
    {
      var key = Key;
      Tuple tuple;
      if (!Processor.TryGetTupleOfNonRemovedEntity(ref key, out tuple))
        return false;
      var needToFetchSystemColumns = false;
      foreach (var pair in columns)
        if (tuple==null || !tuple.GetFieldState(pair.Key).IsAvailable())
          if (pair.Value.IsPrimaryKey || pair.Value.IsSystem)
            needToFetchSystemColumns = ExactType && tuple==null;
          else {
            if (columnIndexesToBeLoaded == null)
              InitializeColumnIndexCollection();
            columnIndexesToBeLoaded.Add(pair.Key);
          }
      if (needToFetchSystemColumns && columnIndexesToBeLoaded == null)
        InitializeColumnIndexCollection();
      return columnIndexesToBeLoaded != null;
    }

    private void InitializeColumnIndexCollection()
    {
      columnIndexesToBeLoaded = new List<int>(columns.Count);
      columnIndexesToBeLoaded.AddRange(Type.Indexes.PrimaryIndex.ColumnIndexMap.System);
    }

    private static RecordSet CreateIndexSeekRecordSet(object cachingKey)
    {
      var pair = (Pair<object, PrefetchTask>) cachingKey;
      var selectedColumnIndexes = pair.Second.columnIndexesToBeLoaded.ToArray();
      return pair.Second.Type.Indexes.PrimaryIndex.ToRecordSet().Seek(() => seekParameter.Value)
        .Select(selectedColumnIndexes);
    }

    private void EnsureColumnIndexesIsSpecified()
    {
      if (columnIndexesToBeLoaded == null)
        throw Exceptions.InternalError(Strings.ExIndexesOfColumnsToBeLoadedAreNotSpecified, Log.Instance);
    }


    // Constructors

    protected PrefetchTask(Key key, TypeInfo type, bool exactType, PrefetchProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");
      Key = key;
      Type = type;
      ExactType = exactType;
      Processor = processor;
    }
  }
}