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
  public class SqlProvider : ExecutableProvider
  {
    private readonly SqlSelect sqlSelect;

    public SqlSelect Query
    {
      get { return sqlSelect; }
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
/*
      using (DbDataReader reader = ExecuteReader(Query))
      {
        if (reader.RecordsAffected > 1)
          throw new InvalidOperationException(Strings.ExQueryMultipleResults);
        if (reader.Read())
        {
          Tuple tuple = GetTuple(reader, select);
          return tuple;
        }
        return null;
      }
*/
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