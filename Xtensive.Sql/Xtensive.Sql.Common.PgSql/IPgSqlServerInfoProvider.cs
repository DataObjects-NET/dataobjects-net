namespace Xtensive.Sql.Common.PgSql
{
  public interface IPgSqlServerInfoProvider : IServerInfoProvider
  {
    ServerConfiguration ServerConfig { get; }

    short MaxDateTimePrecision { get; }
  }
}