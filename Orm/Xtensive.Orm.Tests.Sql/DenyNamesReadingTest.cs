using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  public class DenyNamesReadingTest : SqlTest
  {
    
    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateTablesForTest();
    }

    protected override void TestFixtureTearDown()
    {
      RemoveCreatedTables();
      base.TestFixtureTearDown();
    }

    [Test]
    public void MainTest()
    {
      var defaultSchema = ExtractDefaultSchema();
      Catalog catalog = null;
      Assert.DoesNotThrow(()=> catalog = defaultSchema.Catalog);
      Table table = null;
      Assert.DoesNotThrow(()=> table = defaultSchema.Tables["DenyNamesReadingTest"]);

      Assert.DoesNotThrow(()=> { var catalogName = catalog.Name; });
      Assert.DoesNotThrow(()=> { var catalogDbName = catalog.DbName; });

      Assert.DoesNotThrow(() => { var catalogName = catalog.GetNameInternal(); });
      Assert.DoesNotThrow(() => { var catalogDbName = catalog.GetDbNameInternal(); });

      Assert.DoesNotThrow(() => { var schemaName = defaultSchema.Name; });
      Assert.DoesNotThrow(() => { var schemaName = defaultSchema.DbName; });

      Assert.DoesNotThrow(() => { var schemaName = defaultSchema.GetNameInternal(); });
      Assert.DoesNotThrow(() => { var schemaName = defaultSchema.GetDbNameInternal(); });

      Assert.DoesNotThrow(() => { var schemaName = table.Name; });
      Assert.DoesNotThrow(() => { var schemaName = table.DbName; });

      catalog.MakeNamesUnreadable();

      Assert.Throws<InvalidOperationException>(() => { var catalogName = catalog.Name; });
      Assert.Throws<InvalidOperationException>(() => { var catalogDbName = catalog.DbName; });

      Assert.DoesNotThrow(() => { var catalogName = catalog.GetNameInternal(); });
      Assert.DoesNotThrow(() => { var catalogDbName = catalog.GetDbNameInternal(); });

      Assert.Throws<InvalidOperationException>(() => { var schemaName = defaultSchema.Name; });
      Assert.Throws<InvalidOperationException>(() => { var schemaName = defaultSchema.DbName; });

      Assert.DoesNotThrow(() => { var schemaName = defaultSchema.GetNameInternal(); });
      Assert.DoesNotThrow(() => { var schemaName = defaultSchema.GetDbNameInternal(); });

      Assert.DoesNotThrow(() => { var schemaName = table.Name; });
      Assert.DoesNotThrow(() => { var schemaName = table.DbName; });

    }

    private void CreateTablesForTest()
    {
      var defaultSchema = ExtractDefaultSchema();
      if (defaultSchema.Tables["DenyNamesReadingTest"]!=null)
        return;
      var table = defaultSchema.CreateTable("DenyNamesReadingTest");
      var column = table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_DenyNamesReadingTest", column);
      column = table.CreateColumn("CreationDate", new SqlValueType(SqlType.DateTime));
      var createTableQuery = SqlDdl.Create(table);
      using (var command = Connection.CreateCommand(createTableQuery)) {
        command.ExecuteNonQuery();
      }
    }

    private void RemoveCreatedTables()
    {
      var defaultSchema = ExtractDefaultSchema();
      var tableToRemove = defaultSchema.Tables["DenyNamesReadingTest"];
      var dropTableQuery = SqlDdl.Drop(tableToRemove);
      using (var command = Connection.CreateCommand(dropTableQuery)) {
        command.ExecuteNonQuery();
      }
    }
  }
}
