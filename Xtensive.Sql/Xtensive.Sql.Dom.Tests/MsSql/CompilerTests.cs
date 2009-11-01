using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Sql.Dom.Exceptions;
using Xtensive.Sql.Dom.Tests.MsSql;

namespace Xtensive.Sql.Dom.Tests.MsSql
{
  public class CompilerTests: AdventureWorks
  {
    private SqlConnection sqlConnection;
    private SqlCommand sqlCommand;
    private SqlDriver sqlDriver;


    [TestFixtureSetUp]
    public override void SetUp()
    {
      base.SetUp();

      SqlConnectionProvider provider = new SqlConnectionProvider();
      sqlConnection = (SqlConnection)provider.CreateConnection(
        @"mssql://localhost/AdventureWorks");
      sqlDriver = sqlConnection.Driver as SqlDriver;
      sqlCommand = new SqlCommand(sqlConnection);
      sqlConnection.Open();
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      try {
        if ((sqlConnection!=null) && (sqlConnection.State!=ConnectionState.Closed))
          sqlConnection.Close();
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }

    [Test]
    [ExpectedException(typeof(SqlCompilerException))]
    public void TableIsNotSetTest1()
    {
      SqlDelete delete = Sql.Delete();
      sqlDriver.Compile(delete);
    }

    [Test]
    [ExpectedException(typeof(SqlCompilerException))]
    public void TableIsNotSetTest2()
    {
      SqlInsert insert = Sql.Insert();
      sqlDriver.Compile(insert);
    }

    [Test]
    [ExpectedException(typeof(SqlCompilerException))]
    public void TableIsNotSetTest3()
    {
      SqlUpdate update = Sql.Update();
      sqlDriver.Compile(update);
    }

    [Test]
    [ExpectedException(typeof(SqlCompilerException))]
    public void UnboundColumnsTest()
    {
      SqlTableRef t = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);
      SqlTableRef t1 = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "Contact2");
      SqlTableRef t2 = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);

      SqlUpdate update = Sql.Update(t);
      update.Values[t[0]] = 123;
      update.Values[t[0]] = t2[0];
      update.Values[t2[0]] = t1[0];
      update.Values[t2[0]] = Sql.Null;
      update.Where = t[0]>2;

      sqlDriver.Compile(update);
    }

    [Test]
    public void TableAutoAliasTest()
    {
      SqlTableRef tr1 = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "a");
      SqlTableRef tr2 = Sql.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "a");

      SqlSelect select = Sql.Select(tr1.CrossJoin(tr2));
      select.Top = 10;
      select.Columns.AddRange(tr1[0], tr1[0], tr2[0]);
      select.Where = tr1[0]>1 && tr2[0]>1;

      sqlCommand.Statement = select;
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);
      GetExecuteDataReaderResult(sqlCommand);
    }
  }
}