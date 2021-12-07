// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data.Common;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_2
{
  /// <inheritdoc/>
  internal class Extractor : v8_1.Extractor
  {
    /// <inheritdoc/>
    protected override void BuildPgCatalogSchema(Schema schema)
    {
      base.BuildPgCatalogSchema(schema);
      var table = schema.Tables["pg_class"];
      // fillfactor extension
      CreateTextColumn(table, "reloptions");
    }

    protected RelOptions ParseRelOptions(object value)
    {
      var result = new RelOptions();
      var options = value as string[];
      if (options == null) {
        return result;
      }
      for (var i = 0; i < options.Length; i++) {
        options[i] = options[i].Trim();
        string optionName;
        string optionValue = "";
        var pos = options[i].IndexOf('=');
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
    /// <param name="options"><see href="RelOptions"/> instance to be modified.</param>
    protected virtual void ReadRelOption(string optionName, string optionValue, RelOptions options)
    {
      if (optionName == "fillfactor") {
        _ = byte.TryParse(optionValue, out var value);
        if (value > 0) {
          options.FillFactor = value;
        }
      }
    }


    /// <inheritdoc/>
    protected override void AddSpecialIndexQueryColumns(SqlSelect query, SqlTableRef spc, SqlTableRef rel, SqlTableRef ind, SqlTableRef depend)
    {
      query.Columns.Add(rel["reloptions"]);
    }

    /// <inheritdoc/>
    protected override void ReadSpecialIndexProperties(DbDataReader dr, Index i)
    {
      base.ReadSpecialIndexProperties(dr, i);
      if (dr["reloptions"] != DBNull.Value) {
        var ro = ParseRelOptions(dr["reloptions"]);
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
