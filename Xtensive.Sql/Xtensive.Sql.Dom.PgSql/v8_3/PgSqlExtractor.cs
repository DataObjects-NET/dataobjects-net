using System;
using System.Data.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.PgSql.v8_3
{
  public class PgSqlExtractor : v8_2.PgSqlExtractor
  {
    private const int indoptionDesc = 0x0001;

    public PgSqlExtractor(PgSqlDriver driver)
      : base(driver)
    {
    }

    protected override void AddSpecialIndexQueryColumns(SqlSelect query, SqlTableRef spc, SqlTableRef rel, SqlTableRef ind, SqlTableRef depend)
    {
      base.AddSpecialIndexQueryColumns(query, spc, rel, ind, depend);
      query.Columns.Add(ind["indoption"]);
    }

    protected override void ReadSpecialIndexProperties(DbDataReader dr, Index i)
    {
      base.ReadSpecialIndexProperties(dr, i);
      string indoption = dr["indoption"].ToString();
      string[] columnOption = indoption.Split(' ');
      for (int j = 0; j < columnOption.Length; j++) {
        int option = Int32.Parse(columnOption[j]);
        if ((option & indoptionDesc)==indoptionDesc)
          i.Columns[j].Ascending = false;
      }
    }
  }
}