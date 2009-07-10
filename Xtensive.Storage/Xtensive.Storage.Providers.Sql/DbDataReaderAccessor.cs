// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.30

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Storage.Providers.Sql
{
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