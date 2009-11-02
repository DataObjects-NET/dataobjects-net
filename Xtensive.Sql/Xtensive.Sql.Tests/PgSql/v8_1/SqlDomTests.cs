using NUnit.Framework;

namespace Xtensive.Sql.Tests.PgSql.v8_1
{
  [TestFixture, Explicit]
  public class SqlDomTests : v8_0.SqlDomTests
  {
    protected override string Url { get { return TestUrl.PostgreSql81; } }
  }
}