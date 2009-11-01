using NUnit.Framework;
using Xtensive.Sql.Common;

namespace Xtensive.Sql.Dom.Tests
{
  [TestFixture]
  public class ConnectionProviderTest
  {
    [Test]
    public void ConnectionInfoTest()
    {
      ConnectionInfo info = new ConnectionInfo(@"mssql://localhost/DataObjectsDotNetDemos");
      Assert.AreEqual(info.Protocol, "mssql");
      Assert.AreEqual(info.Host, "localhost");
      Assert.AreEqual(info.Database, "DataObjectsDotNetDemos");
    }

    [Test]
    public void ServerInfoTest()
    {
      ConnectionProvider provider = new SqlConnectionProvider();
      SqlConnection c1 = (SqlConnection)provider.CreateConnection(@"mssql2005://localhost/AdventureWorks");
      Assert.Greater(c1.Driver.ServerInfo.Version.ProductVersion.Major, 8);
      c1.Close();
    }

    [Test]
    public void ProviderTest()
    {
      SqlConnectionProvider provider = new SqlConnectionProvider();
      SqlConnection c1 = (SqlConnection)provider.CreateConnection(@"mssql2005://localhost/AdventureWorks");
      Assert.IsNotNull(c1);
      Assert.IsNotNull(c1.Driver);

      SqlConnection c2 = (SqlConnection)provider.CreateConnection(@"yukon://localhost/AdventureWorks");
      Assert.IsNotNull(c2);
      Assert.IsNotNull(c2.Driver);
      Assert.AreEqual(c2.Driver, c1.Driver);

      SqlConnection c3 = (SqlConnection)provider.CreateConnection(@"mssql2005://localhost/AdventureWorks");
      Assert.IsNotNull(c3);
      Assert.AreEqual(c3.Driver, c1.Driver);

      c1.Close();
      c2.Close();
      c3.Close();
    }
  }
}