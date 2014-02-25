// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.Common;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql.SqlServer.v09
{
  public class CompilerTests: AdventureWorks
  {
    private SqlConnection sqlConnection;
    private DbCommand sqlCommand;
    private SqlDriver sqlDriver;

    [TestFixtureSetUp]
    public override void SetUp()
    {
      base.SetUp();

      sqlDriver = TestSqlDriver.Create(Url);
      sqlConnection = sqlDriver.CreateConnection();
      sqlCommand = sqlConnection.CreateCommand();
      try {
        sqlConnection.Open();
      }
      catch (SystemException e) {
        Console.WriteLine(e);
      }
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
      SqlDelete delete = SqlDml.Delete();
      sqlDriver.Compile(delete);
    }

    [Test]
    [ExpectedException(typeof(SqlCompilerException))]
    public void TableIsNotSetTest2()
    {
      SqlInsert insert = SqlDml.Insert();
      sqlDriver.Compile(insert);
    }

    [Test]
    [ExpectedException(typeof(SqlCompilerException))]
    public void TableIsNotSetTest3()
    {
      SqlUpdate update = SqlDml.Update();
      sqlDriver.Compile(update);
    }

    [Test]
    [ExpectedException(typeof(SqlCompilerException))]
    public void UnboundColumnsTest()
    {
      SqlTableRef t = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);
      SqlTableRef t1 = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "Contact2");
      SqlTableRef t2 = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);

      SqlUpdate update = SqlDml.Update(t);
      update.Values[t[0]] = 123;
      update.Values[t[0]] = t2[0];
      update.Values[t2[0]] = t1[0];
      update.Values[t2[0]] = SqlDml.Null;
      update.Where = t[0]>2;

      sqlDriver.Compile(update);
    }

    [Test]
    public void TableAutoAliasTest()
    {
      SqlTableRef tr1 = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "a");
      SqlTableRef tr2 = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "a");

      SqlSelect select = SqlDml.Select(tr1.CrossJoin(tr2));
      select.Limit = 10;
      select.Columns.AddRange(tr1[0], tr1[0], tr2[0]);
      select.Where = tr1[0]>1 && tr2[0]>1;

      sqlCommand.CommandText = sqlDriver.Compile(select).GetCommandText();
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);
      GetExecuteDataReaderResult(sqlCommand);
    }

    [Test]
    public void VariantTest()
    {
      var variantId = new object();
      var select = SqlDml.Select(SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"]));
      select.Limit = 1;
      select.Columns.Add(SqlDml.Variant(variantId, 1, 2), "value");
      var result = sqlConnection.Driver.Compile(select);
      using (var command = sqlConnection.CreateCommand()) {
        command.CommandText = result.GetCommandText();
        using (var reader = command.ExecuteReader()) {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual(1, reader.GetInt32(0));
        }
        var configuration = new SqlPostCompilerConfiguration {AlternativeBranches = {variantId}};
        command.CommandText = result.GetCommandText(configuration);
        using (var reader = command.ExecuteReader()) {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual(2, reader.GetInt32(0));
        }
      }
    }

    [Test]
    public void PlaceholderTest()
    {
      var placeholderId = new object();
      var select = SqlDml.Select(SqlDml.Placeholder(placeholderId));
      var result = sqlConnection.Driver.Compile(select);
      using (var command = sqlConnection.CreateCommand()) {
        var configuration = new SqlPostCompilerConfiguration {PlaceholderValues = {{placeholderId, "@xxx"}}};
        command.CommandText = result.GetCommandText(configuration);
        var parameter = command.CreateParameter();
        parameter.ParameterName = "xxx";
        parameter.DbType = DbType.Int32;
        parameter.Value = (int) 'x';
        command.Parameters.Add(parameter);
        Assert.AreEqual((int) 'x', command.ExecuteScalar());
      }
      using (var command = sqlConnection.CreateCommand()) {
        var configuration = new SqlPostCompilerConfiguration {PlaceholderValues = {{placeholderId, "@yyy"}}};
        command.CommandText = result.GetCommandText(configuration);
        var parameter = command.CreateParameter();
        parameter.ParameterName = "yyy";
        parameter.DbType = DbType.Int32;
        parameter.Value = (int) 'y';
        command.Parameters.Add(parameter);
        Assert.AreEqual((int) 'y', command.ExecuteScalar());
      }
    }

    [Test]
    public void SingleColumnDynamicFilterTest()
    {
      var dynamicFilterId = new object();
      var tableRef = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);
      var select = SqlDml.Select(tableRef);
      select.Columns.Add(SqlDml.Count());
      var filter = SqlDml.DynamicFilter(dynamicFilterId);
      select.Where = filter;
      filter.Expressions.Add(tableRef["LastName"]);
      var result = sqlConnection.Driver.Compile(select);
      using (var command = sqlConnection.CreateCommand()) {
        var values = new List<string[]> {new[] {"'Achong'"}, new[] {"'Abel'"}};
        var configuration = new SqlPostCompilerConfiguration {DynamicFilterValues = {{dynamicFilterId, values}}};
        command.CommandText = result.GetCommandText(configuration);
        int count = Convert.ToInt32(command.ExecuteScalar());
        Assert.AreEqual(2, count);
      }
    }
    
    [Test]
    public void MultiColumnDynamicFilterTest()
    {
      var dynamicFilterId = new object();
      var tableRef = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"]);
      var select = SqlDml.Select(tableRef);
      select.Columns.Add(SqlDml.Count());
      var filter = SqlDml.DynamicFilter(dynamicFilterId);
      select.Where = filter;
      filter.Expressions.Add(tableRef["FirstName"]);
      filter.Expressions.Add(tableRef["LastName"]);
      var result = sqlConnection.Driver.Compile(select);
      using (var command = sqlConnection.CreateCommand()) {
        var values = new List<string[]> {new[] {"'Gustavo'", "'Achong'"}, new[] {"'Catherine'", "'Abel'"}};
        var configuration = new SqlPostCompilerConfiguration {DynamicFilterValues = {{dynamicFilterId, values}}};
        command.CommandText = result.GetCommandText(configuration);
        int count = Convert.ToInt32(command.ExecuteScalar());
        Assert.AreEqual(2, count);
      }
    }
  }
}