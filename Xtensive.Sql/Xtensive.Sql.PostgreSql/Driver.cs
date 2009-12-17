using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql
{
  internal abstract class Driver : SqlDriver
  {
    protected override ValueTypeMapping.TypeMapper CreateTypeMapper()
    {
      return new TypeMapper(this);
    }

    public override SqlConnection CreateConnection(UrlInfo url)
    {
      return new Connection(this, url);
    }

    // Constructors

    protected Driver(ServerInfoProvider serverInfoProvider)
      : base(serverInfoProvider)
    {
    }
  }
}