using System;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Sql.Firebird
{
  [TestFixture, Explicit]
  public class QueryTest : SqlTest
  {
    protected override string Url { get { return TestUrl.Firebird25_AgentThompson; } }

    public override void SetUp()
    {
      base.SetUp();
      // hack because Visual Nunit doesn't use TestFixtureSetUp attribute, just SetUp attribute
      RealTestFixtureSetUp();
    }

    public override void TearDown()
    {
      base.TearDown();
      // hack because Visual Nunit doesn't use TestFixtureTearDown attribute, just TearDown attribute
      RealTestFixtureTearDown();
    }

    [Test]
    public void PagingTest()
    {
      var schema = ExtractDefaultSchema();
      var table = schema.CreateTable("Some");
      table.CreateColumn("ID", new SqlValueType(SqlType.UInt64));
      table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 255));

      var tableRef = SqlDml.TableRef(table);
      var select = SqlDml.Select(tableRef);
      select.Columns.AddRange(tableRef[0], tableRef[1]);
      select.Limit = 5;
      select.Offset = 3;

      var result = Connection.Driver.Compile(select);
      using (var command = Connection.CreateCommand()) {
        command.CommandText = result.GetCommandText();
        Console.Out.WriteLine(command.CommandText);
//        int count = Convert.ToInt32(command.ExecuteScalar());
//        Assert.AreEqual(2, count);
      }
    }
  }
}