using NUnit.Framework;

namespace Xtensive.Sql.Dom.Tests.PgSql.v8_3
{
  [TestFixture]
  public class SqlDomTests : v8_2.SqlDomTests
  {
    protected override string Url { get { return TestUrl.PgSql83; } }
  }
}