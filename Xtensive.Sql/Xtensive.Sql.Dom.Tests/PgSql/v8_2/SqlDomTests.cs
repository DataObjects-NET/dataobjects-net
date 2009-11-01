using NUnit.Framework;

namespace Xtensive.Sql.Dom.Tests.PgSql.v8_2
{
  [TestFixture]
  public class SqlDomTests : v8_1.SqlDomTests
  {
    protected override string Url
    {
      get { return "pgsql://do4test:do4testpwd@localhost:8232/do4test?Encoding=UNICODE&Pooling=on&MinPoolSize=1&MaxPoolSize=5"; }
    }

    [TestFixtureSetUp]
    public override void FixtureSetup()
    {
      base.FixtureSetup();
    }

    [TestFixtureTearDown]
    public override void FixtureTearDown()
    {
      base.FixtureTearDown();
    }

    [Test]
    public override void ModelTest()
    {
      base.ModelTest();
    }
  }
}