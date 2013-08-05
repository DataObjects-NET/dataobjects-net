// Copyright (C) 2013 Xtensive LLC
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.07.18

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class PartialIndexInfoMap
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private readonly Dictionary<string, Dictionary<string, StoredPartialIndexFilterInfo>> items;

    public StoredPartialIndexFilterInfo FindIndex(string table, string index)
    {
      Dictionary<string, StoredPartialIndexFilterInfo> indexesForTable;
      StoredPartialIndexFilterInfo result;
      if (items.TryGetValue(table, out indexesForTable) && indexesForTable.TryGetValue(index, out result))
        return result;
      return null;
    }

    public PartialIndexInfoMap(MappingResolver resolver, IEnumerable<StoredPartialIndexFilterInfo> indexes)
    {
      ArgumentValidator.EnsureArgumentNotNull(resolver, "resolver");
      ArgumentValidator.EnsureArgumentNotNull(indexes, "indexes");

      items = new Dictionary<string, Dictionary<string, StoredPartialIndexFilterInfo>>(Comparer);

      foreach (var index in indexes) {
        var tableName = resolver.GetNodeName(index.Database, index.Schema, index.Table);
        Dictionary<string, StoredPartialIndexFilterInfo> indexesForTable;
        if (!items.TryGetValue(tableName, out indexesForTable)) {
          indexesForTable = new Dictionary<string, StoredPartialIndexFilterInfo>(Comparer);
          items.Add(tableName, indexesForTable);
        }
        indexesForTable.Add(index.Name, index);
      }
    }
  }
}