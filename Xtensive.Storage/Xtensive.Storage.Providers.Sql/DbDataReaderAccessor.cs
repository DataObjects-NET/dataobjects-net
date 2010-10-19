// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.30

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Provider-level <see cref="DbDataReader"/> accessor.
  /// </summary>
  public sealed class DbDataReaderAccessor
  {
    private readonly TypeMapping[] mappings;

    public void Read(DbDataReader source, Tuple target)
    {
      for (int i = 0; i < mappings.Length; i++) {
        var value = !source.IsDBNull(i)
          ? mappings[i].ReadValue(source, i)
          : null;
        target.SetValue(i, value);
      }
    }

    // Constructors

    internal DbDataReaderAccessor(IEnumerable<TypeMapping> mappings)
    {
      this.mappings = mappings.ToArray();
    }
  }
}