// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlProvider : ExecutableProvider
  {
    private readonly SqlSelect sqlSelect;

    public SqlSelect Query
    {
      get { return sqlSelect; }
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      throw new System.NotImplementedException();
    }

    protected override void Initialize()
    {}


    // Constructor

    public SqlProvider(Provider origin, SqlSelect sqlSelect)
      : base(origin)
    {
      this.sqlSelect = sqlSelect;
    }
  }
}