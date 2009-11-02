using Npgsql;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql.v8_2
{
  internal class ServerInfoProvider : v8_1.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures()
    {
      return base.GetIndexFeatures() | IndexFeatures.FillFactor;
    }

    // Constructors

    public ServerInfoProvider(NpgsqlConnection connection)
      : base(connection)
    {
    }
  }
}