namespace Xtensive.Sql.Common.PgSql.v8_2
{
  public class PgSqlServerInfoProvider : v8_1.PgSqlServerInfoProvider
  {
    public PgSqlServerInfoProvider(Connection conn)
      : base(conn)
    {
    }

    protected override IndexFeatures IndexFeatures
    {
      get { return base.IndexFeatures | IndexFeatures.FillFactor; }
    }
  }
}