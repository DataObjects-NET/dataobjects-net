using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql
{
  internal abstract class Driver : SqlDriver
  {
    private const string DefaultSchemaName = "public";

    protected override ValueTypeMapping.TypeMappingHandler CreateTypeMappingHandler()
    {
      return new TypeMappingHandler(this);
    }

    protected override SqlConnectionHandler CreateConnectionHandler()
    {
      return new ConnectionHandler(this);
    }

    protected override string GetDefaultSchemaName(UrlInfo url)
    {
      return url.GetSchema(DefaultSchemaName);
    }


    // Constructors

    protected Driver(ServerInfoProvider serverInfoProvider)
      : base(serverInfoProvider)
    {
    }
  }
}