// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlProvider : ProviderImplementation
  {
    private readonly SqlSelect sqlSelect;

    public SqlSelect Query
    {
      get { return sqlSelect; }
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      throw new System.NotImplementedException();
    }


    // Constructor

    public SqlProvider(RecordHeader header, SqlSelect sqlSelect)
      : base(header)
    {
      this.sqlSelect = sqlSelect;
    }
  }
}