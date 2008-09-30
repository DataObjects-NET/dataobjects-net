// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.30

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Tuples;
using System.Linq;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class DbDataReaderAccessor
  {
    private Func<DbDataReader, int, object>[] accessors;

    public void Read(DbDataReader source, Tuple target)
    {
      for (int i = 0; i < accessors.Length; i++) {
        if (source.IsDBNull(i)) {
          target.SetValue(i, null);
          continue;
        }
        target.SetValue(i, accessors[i](source, i));
      }
    }


    // Constructors

    internal DbDataReaderAccessor(IEnumerable<Func<DbDataReader, int, object>> readers, IEnumerable<Func<object, object>> converters)
    {
      accessors = readers.ToArray();
      int i = 0;
      foreach (var item in converters) {
        var converter = item;
        var accessor = accessors[i];
        if (converter!=null)
          accessors[i] = (reader, index) => converter(accessor(reader, index));
        i++;
      }
    }
  }
}