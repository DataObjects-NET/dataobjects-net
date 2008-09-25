// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlUpdateRequestParameter
  {
    public ColumnInfo Column { get; private set; }

    public SqlParameter Parameter { get; private set; }

    public Func<Tuple, object> Value { get; private set; }

    public SqlUpdateRequestParameter(ColumnInfo column, Func<Tuple, object> value)
      : this(value)
    {
      Column = column;
    }

    public SqlUpdateRequestParameter(Func<Tuple, object> value)
    {
      Parameter = new SqlParameter();
      Value = value;
    }
  }
}