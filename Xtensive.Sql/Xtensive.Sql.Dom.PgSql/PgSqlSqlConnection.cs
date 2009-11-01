using Xtensive.Sql.Common;
using Xtensive.Sql.Common.PgSql;

namespace Xtensive.Sql.Dom.PgSql
{
  public class PgSqlSqlConnection : SqlConnection
  {
    public PgSqlSqlConnection(SqlDriver driver, ConnectionInfo info)
      : base(driver, PgSqlConnection.GetRealConnection(info), info)
    {
    }
  }
}