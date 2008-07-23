// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.17

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal static class Fetcher
  {
    public static Tuple Fetch(Key key)
    {
      IndexInfo index = key.Hierarchy.Root.Indexes.PrimaryIndex;
      return Session.Current.Handler.Fetch(index, key, index.Columns.Where(c => !c.LazyLoad));
    }

    public static Tuple Fetch(Key key, FieldInfo field)
    {
      IndexInfo index = key.Hierarchy.Root.Indexes.PrimaryIndex;
      IEnumerable<ColumnInfo> columns = key.Type.Columns
        .Skip(field.MappingInfo.Offset)
        .Take(field.MappingInfo.Length);
      return Session.Current.Handler.Fetch(index, key, columns);
    }
  }
}