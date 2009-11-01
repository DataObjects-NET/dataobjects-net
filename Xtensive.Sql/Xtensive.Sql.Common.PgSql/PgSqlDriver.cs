using System;
using Npgsql;
using Xtensive.Sql.Common.PgSql.v8_0;

namespace Xtensive.Sql.Common.PgSql
{
  [Protocol("pgsql"), Protocol("postgres"), Protocol("postgresql")]
  public class PgSqlDriver : Driver
  {
    public PgSqlDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }

    public PgSqlDriver(VersionInfo version)
      : base(version)
    {
      throw new NotSupportedException("This version of constructor is not supported, use the ConnectionInfo based one.");
    }

    protected override Connection CreateDbConnection(ConnectionInfo info)
    {
      return new PgSqlConnection(this, info);
    }

    protected override IServerInfoProvider CreateServerInfoProvider(ConnectionInfo connectionInfo)
    {
      using (Connection connection = base.CreateConnection(connectionInfo)) {
        connection.Open();
        NpgsqlConnection nconn = connection.RealConnection as NpgsqlConnection;
        ServerVersion sv = nconn.PostgreSqlVersion;
        return GetServerInfoProvider(connection, sv.Major, sv.Minor);
      }
    }

    protected override IServerInfoProvider CreateServerInfoProvider(VersionInfo versionInfo)
    {
      throw new NotSupportedException("This version of CreateServerInfoProvider method is not supported, use the ConnectionInfo based one.");
    }

    private IServerInfoProvider GetServerInfoProvider(Connection connection, int major, int minor)
    {
      if (major < 8) {
        throw new NotSupportedException("PostgreSQL below 8.0 is not supported!");
      }
      else if (major==8 && minor==0) {
        return new PgSqlServerInfoProvider(connection);
      }
      else if (major==8 && minor==1) {
        return new v8_1.PgSqlServerInfoProvider(connection);
      }
      else if (major==8 && minor==2) {
        return new v8_2.PgSqlServerInfoProvider(connection);
      }
      else
        return new v8_3.PgSqlServerInfoProvider(connection);
    }
  }
}