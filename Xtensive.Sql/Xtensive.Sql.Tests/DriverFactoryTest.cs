using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Sql.Tests
{
  [TestFixture]
  public class DriverFactoryTest
  {
    [Test]
    public void ConnectionUrlTest()
    {
      var url = new UrlInfo(TestUrl.SqlServer2005AW);
      Assert.AreEqual(url.Protocol, "sqlserver");
      Assert.AreEqual(url.Host, "localhost");
      Assert.AreEqual(url.Resource, "AdventureWorks");
    }

    [Test]
    public void ServerInfoTest()
    {
      var driver = SqlDriver.Create(TestUrl.SqlServer2005);
      Assert.Greater(driver.ServerInfo.Version.ProductVersion.Major, 8);
    }

    [Test]
    public void ProviderTest()
    {
      SqlDriver driver;
      driver = SqlDriver.Create(TestUrl.SqlServer2005);
      Assert.IsNotNull(driver);
      driver = SqlDriver.Create(TestUrl.PostgreSql83);
      Assert.IsNotNull(driver);
      driver = SqlDriver.Create(TestUrl.VistaDb);
      Assert.IsNotNull(driver);
    }
  }
}