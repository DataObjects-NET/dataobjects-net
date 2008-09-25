// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.25

using System;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlFetchRequestParameter
  {
    public ColumnInfoRef ColumnRef { get; private set; }

    public SqlParameter Parameter { get; private set; }

    public Func<object> Value { get; private set; }

    public SqlFetchRequestParameter(ColumnInfoRef column, Func<object> value)
      : this(value)
    {
      ColumnRef = column;
    }

    public SqlFetchRequestParameter(Func<object> value)
    {
      Parameter = new SqlParameter();
      Value = value;
    }
  }
}