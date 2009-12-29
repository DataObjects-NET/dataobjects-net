using Xtensive.Sql.Info;

namespace Xtensive.Sql.PostgreSql.v8_3
{
  internal class ServerInfoProvider : v8_2.ServerInfoProvider
  {
    protected override IndexFeatures GetIndexFeatures()
    {
      return base.GetIndexFeatures() | IndexFeatures.SortOrder;
    }
    

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}