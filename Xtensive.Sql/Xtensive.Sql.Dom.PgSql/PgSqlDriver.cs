using System;
using Npgsql;
using Xtensive.Sql.Common;
using Xtensive.Sql.Common.PgSql;
using Xtensive.Sql.Common.PgSql.v8_0;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database.Extractor;
using Xtensive.Sql.Dom.PgSql.v8_0;
using CommonPg = Xtensive.Sql.Common.PgSql;
using PgSqlExtractor=Xtensive.Sql.Dom.PgSql.v8_1.PgSqlExtractor;

namespace Xtensive.Sql.Dom.PgSql
{
  [Protocol("pgsql"), Protocol("postgres"), Protocol("postgresql")]
  public class PgSqlDriver : SqlDriver
  {
    // Methods
    public PgSqlDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }

    public PgSqlDriver(VersionInfo versionInfo)
      : base(versionInfo)
    {
      throw new NotSupportedException("This overload of constructor is not supported, use the ConnectionInfo based one.");
    }

    protected override Connection CreateDbConnection(ConnectionInfo info)
    {
      return new PgSqlSqlConnection(this, info);
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
      throw new NotSupportedException("This overload of CreateServerInfoProvider method is not supported, use the ConnectionInfo based one.");
    }

    protected override SqlCompiler CreateCompiler()
    {
      int major = base.ServerInfo.Version.ProductVersion.Major;
      int minor = base.ServerInfo.Version.ProductVersion.Minor;
      if (major < 8)
        throw new NotSupportedException("PostgreSQL below 8.0 is not supported!");
      return new PgSqlCompiler(this);
    }

    protected override SqlExtractor CreateExtractor()
    {
      int major = base.ServerInfo.Version.ProductVersion.Major;
      int minor = base.ServerInfo.Version.ProductVersion.Minor;
      if (major < 8)
        throw new NotSupportedException("PostgreSQL below 8.0 is not supported!");
      else if (major==8 && minor==0) {
        return new v8_0.PgSqlExtractor(this);
      }
      else if (major==8 && minor==1) {
        return new PgSqlExtractor(this);
      }
      else if (major==8 && minor==2) {
        return new v8_2.PgSqlExtractor(this);
      }
      else {
        return new v8_3.PgSqlExtractor(this);
      }
    }

    protected override SqlTranslator CreateTranslator()
    {
      int major = base.ServerInfo.Version.ProductVersion.Major;
      int minor = base.ServerInfo.Version.ProductVersion.Minor;
      if (major < 8)
        throw new NotSupportedException("PostgreSQL below 8.0 is not supported!");
      else if (major==8 && minor==0) {
        return new PgSqlTranslator(this);
      }
      else if (major==8 && minor==1) {
        return new v8_1.PgSqlTranslator(this);
      }
      else if (major==8 && minor==2) {
        return new v8_2.PgSqlTranslator(this);
      }
      else {
        return new v8_3.PgSqlTranslator(this);
      }
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
        return new Common.PgSql.v8_1.PgSqlServerInfoProvider(connection);
      }
      else if (major==8 && minor==2) {
        return new Common.PgSql.v8_2.PgSqlServerInfoProvider(connection);
      }
      else
        return new Common.PgSql.v8_3.PgSqlServerInfoProvider(connection);
    }

    public new IPgSqlServerInfoProvider ServerInfoProvider
    {
      get { return base.ServerInfoProvider as IPgSqlServerInfoProvider; }
    }
  }
}