using NUnit.Framework;

namespace Xtensive.Sql.Tests.PgSql.v8_4
{
  [TestFixture]
  public class SqlDomTests : v8_3.SqlDomTests
  {
    protected override string Url { get { return TestUrl.PostgreSql84; } }
  }
}