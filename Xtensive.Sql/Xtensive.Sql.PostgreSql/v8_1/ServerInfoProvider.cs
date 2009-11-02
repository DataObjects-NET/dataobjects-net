using Npgsql;

namespace Xtensive.Sql.PostgreSql.v8_1
{
  internal class ServerInfoProvider : v8_0.ServerInfoProvider
  {
    public ServerInfoProvider(NpgsqlConnection connection)
      : base(connection)
    {
    }
  }
}