using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.SqlServer.v10
{
  public class ExtractorTest : v09.ExtractorTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2008);
    }

    [Test]
    public void ExtractUDTTest()
    {
      string create = "CREATE TYPE GuidList AS TABLE ( Id UNIQUEIDENTIFIER NULL )";
      string drop = "if type_id('GuidList') is not null drop type GuidList";

      ExecuteNonQuery(drop);
      ExecuteNonQuery(create);
      ExtractDefaultSchema();
    }

  }
}