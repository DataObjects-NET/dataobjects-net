// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Internals
{
  internal static class Fetcher
  {
    private static readonly object syncRoot = new object();
    private static readonly Dictionary<Pair<IndexInfo, int>, RecordSet> cache = new Dictionary<Pair<IndexInfo, int>, RecordSet>();
    private static readonly Parameter<Key> pKey = new Parameter<Key>("Key");

    public static void Fetch(Key key)
    {
      IndexInfo index = key.Type.Indexes.PrimaryIndex;
      Fetch(index, key, index.Columns.Where(c => !c.IsLazyLoad));
    }

    public static void Fetch(Key key, FieldInfo field)
    {
      // Fetching all non-lazyload fields instead of exactly one non-lazy load field.
      if (!field.IsLazyLoad) {
        Fetch(key);
        return;
      }

      // TODO: Cache
      IndexInfo index = key.Type.Indexes.PrimaryIndex;
      IEnumerable<ColumnInfo> columns = key.Type.Columns
        .Where(c => c.IsPrimaryKey || c.IsSystem || !c.IsLazyLoad)
        .Union(key.Type.Columns
          .Skip(field.MappingInfo.Offset)
          .Take(field.MappingInfo.Length));
      Fetch(index, key, columns);
    }

    private static void Fetch(IndexInfo index, Key key, IEnumerable<ColumnInfo> columns)
    {
      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Session '{0}'. Fetching: Key = '{1}', Columns = '{2}'", Session.Current, pKey, columns.Select(c => c.Name).ToCommaDelimitedString());
      var columnIndexes = columns.Select(c => index.Columns.IndexOf(c)).ToArray();
      var rs = GetCachedRecordSet(index, columnIndexes);
      using (new ParameterScope()) {
        pKey.Value = key;
        rs.ImportToDataCache();
      }
    }

    private static RecordSet GetCachedRecordSet(IndexInfo index, int[] columnIndexes)
    {
      var columnsKey = columnIndexes.Select((i, j) => new Pair<int>(i, j)).Aggregate(0, (s, p) => s^((1 >> (p.First)) ^ p.Second*379));
      var key = new Pair<IndexInfo, int>(index, columnsKey);
      RecordSet value;
      if (!cache.TryGetValue(key, out value))lock(syncRoot)if(!cache.TryGetValue(key, out value)) {
        value = IndexProvider.Get(index).Result
        .Seek(() => pKey.Value.Tuple)
        .Select(columnIndexes);
        cache.Add(key, value);
      }
      return value;
    }

  }
}
