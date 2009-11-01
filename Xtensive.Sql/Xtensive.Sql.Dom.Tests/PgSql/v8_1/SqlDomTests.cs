using NUnit.Framework;

namespace Xtensive.Sql.Dom.Tests.PgSql.v8_1
{
  [TestFixture]
  public class SqlDomTests : v8_0.SqlDomTests
  {
    protected override string Url
    {
      get { return "pgsql://do4test:do4testpwd@localhost:8132/do4test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5"; }
    }

/*
    [TestFixtureSetUp]
    public override void FixtureSetup()
    {
      base.FixtureSetup();
    }
*/
  }
}