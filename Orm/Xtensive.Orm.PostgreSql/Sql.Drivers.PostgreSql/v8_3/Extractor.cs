// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Data.Common;
using Xtensive.Sql.Dml;
using Index = Xtensive.Sql.Model.Index;

namespace Xtensive.Sql.Drivers.PostgreSql.v8_3
{
  /// <inheritdoc/>
  internal class Extractor : v8_2.Extractor
  {
    private const int indoptionDesc = 0x0001;

    /// <inheritdoc/>
    protected override void AddSpecialIndexQueryColumns(SqlSelect query, SqlTableRef spc, SqlTableRef rel, SqlTableRef ind, SqlTableRef depend)
    {
      base.AddSpecialIndexQueryColumns(query, spc, rel, ind, depend);
      query.Columns.Add(ind["indoption"]);
    }

    /// <inheritdoc/>
    protected override void ReadSpecialIndexProperties(DbDataReader dr, Index i)
    {
      base.ReadSpecialIndexProperties(dr, i);
      var indoption = (short[]) dr["indoption"];
      for (var j = 0; j < indoption.Length; j++) {
        int option = indoption[j];
        if ((option & indoptionDesc) == indoptionDesc) {
          i.Columns[j].Ascending = false;
        }
      }
    }

    // Constructors

    public Extractor(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
