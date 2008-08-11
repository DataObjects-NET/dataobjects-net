// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.17

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Internals
{
  internal static class Fetcher
  {
    /// <inheritdoc/>
    public static Tuple Fetch(IndexInfo index, Key key, IEnumerable<ColumnInfo> columns)
    {
      var rs = Session.Current.Handler.Select(index)
        .Range(key.Tuple, key.Tuple)
        .Select(columns.Select(c => index.Columns.IndexOf(c)).ToArray());
      var enumerator = rs.GetEnumerator();
      if (enumerator.MoveNext())
        return enumerator.Current;
      throw new InvalidOperationException(/*Strings.ExInstanceNotFound*/);
    }


    public static Tuple Fetch(Key key)
    {
      IndexInfo index = key.Type.Indexes.PrimaryIndex;
      return Fetch(index, key, index.Columns.Where(c => !c.LazyLoad));
    }

    public static Tuple Fetch(Key key, FieldInfo field)
    {
      // Fetching all non-lazyload fields instead of exactly one non-lazy load field.
      if (!field.LazyLoad)
        return Fetch(key);

      IndexInfo index = key.Type.Indexes.PrimaryIndex;
      IEnumerable<ColumnInfo> columns = key.Type.Columns
        .Skip(field.MappingInfo.Offset)
        .Take(field.MappingInfo.Length);
      return Fetch(index, key, columns);
    }
  }
}