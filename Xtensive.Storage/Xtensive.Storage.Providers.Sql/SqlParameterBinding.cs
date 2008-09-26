// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.26

using System;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Model;
using Action=System.Action;

namespace Xtensive.Storage.Providers.Sql
{
  public abstract class SqlParameterBinding
  {
    public ColumnInfo Column { get; protected set; }

    public SqlParameter Parameter { get; private set; }

    public Action ValueConverter { get; set; }

    protected SqlParameterBinding()
    {
      Parameter = new SqlParameter();
    }
  }
}