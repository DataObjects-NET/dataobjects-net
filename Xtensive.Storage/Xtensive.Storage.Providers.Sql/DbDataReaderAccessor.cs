// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.30

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class DbDataReaderAccessor
  {
    private List<Func<DbDataReader, int, object>> readers;
    private List<Func<object, object>> converters;

    public void Read(DbDataReader source, Tuple target)
    {
      for (int i = 0; i < readers.Count; i++) {
        if (source.IsDBNull(i)) {
          target.SetValue(i, null);
          continue;
        }
        if (converters[i]!=null)
          target.SetValue(i, converters[i](readers[i](source, i)));
        else
          target.SetValue(i, readers[i](source, i));
      }
    }

    internal DbDataReaderAccessor(IEnumerable<Func<DbDataReader, int, object>> readers, IEnumerable<Func<object, object>> converters)
    {
      this.readers = new List<Func<DbDataReader, int, object>>(readers);
      this.converters = new List<Func<object, object>>(converters);
    }
  }
}