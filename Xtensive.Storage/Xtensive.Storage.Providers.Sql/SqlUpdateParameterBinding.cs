// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlUpdateParameterBinding : SqlParameterBinding
  {
    public Func<Tuple, object> Value { get; private set; }

    public SqlUpdateParameterBinding(ColumnInfo column, Func<Tuple, object> value)
      : this(value)
    {
      Column = column;
    }

    public SqlUpdateParameterBinding(Func<Tuple, object> value)
    {
      Value = value;
    }
  }
}