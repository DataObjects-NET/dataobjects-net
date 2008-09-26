// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlFetchParameterBinding : SqlParameterBinding
  {
    public ColumnInfoRef ColumnRef { get; private set; }

    public Func<object> Value { get; private set; }

    public SqlFetchParameterBinding(ColumnInfoRef column, Func<object> value)
      : this(value)
    {
      ColumnRef = column;
    }

    public SqlFetchParameterBinding(Func<object> value)
    {
      Value = value;
    }
  }
}