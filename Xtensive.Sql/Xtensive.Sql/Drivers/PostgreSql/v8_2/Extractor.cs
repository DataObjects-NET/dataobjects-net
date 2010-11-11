// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data.Common;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.PostgreSql.v8_2
{
  internal class Extractor : v8_1.Extractor
  {
    protected override void BuildPgCatalogSchema(Schema schema)
    {
      base.BuildPgCatalogSchema(schema);
      Table t;
      t = schema.Tables["pg_class"];
      // fillfactor extension
      CreateTextColumn(t, "reloptions");
    }

    protected RelOptions ParseRelOptions(object value)
    {
      var result = new RelOptions();
      var options = value as string[];
      if (options==null)
        return result;
      for (int i = 0; i < options.Length; i++) {
        options[i] = options[i].Trim();
        string optionName;
        string optionValue = "";
        int pos = options[i].IndexOf('=');
        if (pos >= 0) {
          optionName = options[i].Substring(0, pos).ToLower();
          optionValue = options[i].Substring(pos + 1);
        }
        else {
          optionName = options[i];
        }
        ReadRelOption(optionName, optionValue, result);
      }
      return result;
    }

    /// <summary>
    /// Reads a certain reloption value.
    /// </summary>
    /// <param name="optionName">The name of the option in lowercase</param>
    /// <param name="optionValue">The value of the option, maybe empty, but not null</param>
    /// <returns></returns>
    protected virtual void ReadRelOption(string optionName, string optionValue, RelOptions options)
    {
      if (optionName=="fillfactor") {
        byte value = 0;
        Byte.TryParse(optionValue, out value);
        if (value > 0)
          options.FillFactor = value;
      }
    }


    protected override void AddSpecialIndexQueryColumns(SqlSelect query, SqlTableRef spc, SqlTableRef rel, SqlTableRef ind, SqlTableRef depend)
    {
      query.Columns.Add(rel["reloptions"]);
    }

    protected override void ReadSpecialIndexProperties(DbDataReader dr, Index i)
    {
      base.ReadSpecialIndexProperties(dr, i);
      if (dr["reloptions"]!=DBNull.Value) {
        RelOptions ro = ParseRelOptions(dr["reloptions"]);
        i.FillFactor = ro.FillFactor;
      }
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}