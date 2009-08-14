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
      var url = UrlInfo.Parse(TestUrl.SqlServer2005Aw);
      Assert.AreEqual(url.Protocol, "sqlserver");
      Assert.AreEqual(url.Host, "localhost");
      Assert.AreEqual(url.Resource, "AdventureWorks");

      url = UrlInfo.Parse("sqlserver://localhost/database");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("default schema", url.GetSchema("default schema"));

      url = UrlInfo.Parse("sqlserver://localhost/database/");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("default schema", url.GetSchema("default schema"));

      url = UrlInfo.Parse("sqlserver://localhost/database/schema");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("schema", url.GetSchema(string.Empty));

      url = UrlInfo.Parse("sqlserver://localhost/database/schema/");
      Assert.AreEqual("database", url.GetDatabase());
      Assert.AreEqual("schema", url.GetSchema(string.Empty));
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
//      driver = SqlDriver.Create(TestUrl.Oracle11);
//      Assert.IsNotNull(driver);
    }
  }
}