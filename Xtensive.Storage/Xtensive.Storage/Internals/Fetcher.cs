// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Internals
{
  internal static class Fetcher
  {
    private static readonly Dictionary<RequestKey, RecordSet> cache = new Dictionary<RequestKey, RecordSet>();
    private static readonly Parameter<Key> pKey = new Parameter<Key>("Key");
    private static readonly object _lock = new object();

    #region Nested type: RequestKey

    private sealed class RequestKey
    {
      public readonly IndexInfo Index;
      public readonly int[] ColumnIndexes;
      private int cachedHashCode;

      public bool Equals(RequestKey obj)
      {
        if (ReferenceEquals(null, obj))
          return false;
        if (cachedHashCode!=obj.cachedHashCode)
          return false;
        if (Index!=obj.Index)
          return false;
        var objColumnIndexes = obj.ColumnIndexes;
        if (ColumnIndexes.Length!=objColumnIndexes.Length)
          return false;
        for (int i = 0; i < ColumnIndexes.Length; i++)
          if (ColumnIndexes[i]!=objColumnIndexes[i])
            return false;
        return true;
      }

      public override bool Equals(object obj)
      {
        return Equals(obj as RequestKey);
      }

      public override int GetHashCode()
      {
        return cachedHashCode;
      }

      public RequestKey(IndexInfo index, int[] columnIndexes)
      {
        Index = index;
        ColumnIndexes = columnIndexes;
        cachedHashCode = columnIndexes.Length;
        for (int i = 0; i < ColumnIndexes.Length; i++)
          cachedHashCode = unchecked (379 * cachedHashCode + ColumnIndexes[i]);
      }
    }

    #endregion
    
    public static Key Fetch(Key key)
    {
      IndexInfo index = (key.IsTypeCached ? key.Type : key.Hierarchy.Root).Indexes.PrimaryIndex;
      return Fetch(index, key, index.Columns.Where(c => !c.IsLazyLoad));
    }

    public static Key Fetch(Key key, FieldInfo field)
    {
      // Fetching all non-lazyload fields instead of exactly one non-lazy load field.
      if (!field.IsLazyLoad) {
        return Fetch(key);
      }

      // TODO: Cache
      IndexInfo index = (key.IsTypeCached ? key.Type : key.Hierarchy.Root).Indexes.PrimaryIndex;
      IEnumerable<ColumnInfo> columns = index.Columns
        .Where(c => c.IsPrimaryKey || c.IsSystem || !c.IsLazyLoad)
        .Union(index.Columns
          .Skip(field.MappingInfo.Offset)
          .Take(field.MappingInfo.Length));
      return Fetch(index, key, columns);
    }

    private static Key Fetch(IndexInfo index, Key key, IEnumerable<ColumnInfo> columns)
    {
      Key result;
      var session = Session.Current;
      if (session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Fetching: Key = '{1}', Columns = '{2}'", session, pKey, columns.Select(c => c.Name).ToCommaDelimitedString());

      var columnIndexes = columns.Select(c => index.Columns.IndexOf(c)).ToArray();
      var rs = GetCachedRecordSet(index, columnIndexes);
      using (new ParameterScope()) {
        pKey.Value = key;
        result = session.Domain.RecordSetParser.ParseFirstFast(rs);
        if (result == null) {
          var state = session.Cache[key];
          if (state==null)
            state = session.Cache.Add(key);
          state.Update(null);
        }
      }
      return result;
    }

    private static RecordSet GetCachedRecordSet(IndexInfo index, int[] columnIndexes)
    {
      var key = new RequestKey(index, columnIndexes);
      RecordSet value;
      if (!cache.TryGetValue(key, out value)) lock (_lock) if (!cache.TryGetValue(key, out value)) {
        value = IndexProvider.Get(index).Result
        .Seek(() => pKey.Value)
        .Select(columnIndexes);
        cache.Add(key, value);
      }
      return value;
    }
  }
}
