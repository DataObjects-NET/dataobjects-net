// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data.Common;
using Xtensive.Sql.Model;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_3
{
  internal class Extractor : v8_2.Extractor
  {
    private const int indoptionDesc = 0x0001;

    protected override void AddSpecialIndexQueryColumns(SqlSelect query, SqlTableRef spc, SqlTableRef rel, SqlTableRef ind, SqlTableRef depend)
    {
      base.AddSpecialIndexQueryColumns(query, spc, rel, ind, depend);
      query.Columns.Add(ind["indoption"]);
    }

    protected override void ReadSpecialIndexProperties(DbDataReader dr, Index i)
    {
      base.ReadSpecialIndexProperties(dr, i);
      var indoption = (short [])dr["indoption"];
      for (int j = 0; j < indoption.Length; j++) {
        int option = indoption[j];
        if ((option & indoptionDesc)==indoptionDesc)
          i.Columns[j].Ascending = false;
      }
    }

    // Consructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}