using NUnit.Framework;

namespace Xtensive.Sql.Dom.Tests.PgSql.v8_3
{
  [TestFixture]
  public class SqlDomTests : v8_2.SqlDomTests
  {
    protected override string Url
    {
      get { return "pgsql://do4test:do4testpwd@localhost:8332/do4test?Encoding=ASCII&Pooling=on&MinPoolSize=1&MaxPoolSize=5"; }
    }
  }
}