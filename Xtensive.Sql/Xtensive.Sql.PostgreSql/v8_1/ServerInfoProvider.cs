using System;
using Npgsql;

namespace Xtensive.Sql.PostgreSql.v8_1
{
  internal class ServerInfoProvider : v8_0.ServerInfoProvider
  {
    // Constructors

    public ServerInfoProvider(NpgsqlConnection connection, Version version)
      : base(connection, version)
    {
    }
  }
}