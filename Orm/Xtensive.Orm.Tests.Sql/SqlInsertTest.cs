// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql
{
  public class SqlInsertTest : SqlTest
  {
    #region Sql templates
    private const string FirebirdCreateTableTemplate = "CREATE TABLE \"{0}\" (\"Id\" integer not null" +
      ", \"FirstName\" varchar(255) not null, \"LastName\" varchar(255) not null, \"Birthday\" timestamp not null)";

    private const string MysqlCreateTableTemplate = "CREATE TABLE `{0}` (`Id` int not null" +
      ", `FirstName` varchar(255) not null, `LastName` varchar(255) not null, `Birthday` datetime not null)";

    private const string OracleCreateTableTemplate = "CREATE TABLE \"{0}\" (\"Id\" number not null" +
      ", \"FirstName\" nvarchar2(255) not null, \"LastName\" nvarchar2(255) not null, \"Birthday\" timestamp not null)";

    private const string PgSqlCreateTableTemplate = "CREATE TABLE \"{0}\" (\"Id\" integer not null" +
      ", \"FirstName\" varchar(255) not null, \"LastName\" varchar(255) not null, \"Birthday\" timestamp not null)";

    private const string SqlServerCreateTableTemplate = "CREATE TABLE [{0}] ([Id] integer not null" +
      ", [FirstName] nvarchar(255) not null, [LastName] nvarchar(255) not null, [Birthday] datetime2 not null)";

    private const string SqliteCreateTableTemplate = "CREATE TABLE \"{0}\" (\"Id\" integer not null" +
      ", \"FirstName\" varchar(255) not null, \"LastName\" varchar(255) not null, \"Birthday\" datetime not null)";

    private const string MysqlDropTableTemplate = "DROP TABLE `{0}`";
    private const string SqlServerDropTableTemplate = "DROP TABLE [{0}]";
    private const string DefaultDropTableTemplate = "DROP TABLE \"{0}\"";

    #endregion

    public override void SetUp()
    {
      var testTable = TestContext.CurrentContext.Test.MethodName;
      _ = ExecuteNonQuery(GetCreateTableScript(testTable));
    }

    public override void TearDown()
    {
      var testTable = TestContext.CurrentContext.Test.MethodName;
      _ = ExecuteNonQuery(GetDropTableScript(testTable));
    }

    private string GetCreateTableScript(string tableName)
    {
      return StorageProviderInfo.Instance.Provider switch {
        StorageProvider.Firebird => string.Format(FirebirdCreateTableTemplate, tableName),
        StorageProvider.MySql => string.Format(MysqlCreateTableTemplate, tableName),
        StorageProvider.Oracle => string.Format(OracleCreateTableTemplate, tableName),
        StorageProvider.PostgreSql => string.Format(PgSqlCreateTableTemplate, tableName),
        StorageProvider.SqlServer => string.Format(SqlServerCreateTableTemplate, tableName),
        StorageProvider.Sqlite => string.Format(SqliteCreateTableTemplate, tableName),
        _ => throw new ArgumentOutOfRangeException()
      };
    }

    private string GetDropTableScript(string tableName)
    {
      return StorageProviderInfo.Instance.Provider switch {
        StorageProvider.Firebird => string.Format(DefaultDropTableTemplate, tableName),
        StorageProvider.MySql => string.Format(MysqlDropTableTemplate, tableName),
        StorageProvider.Oracle => string.Format(DefaultDropTableTemplate, tableName),
        StorageProvider.PostgreSql => string.Format(DefaultDropTableTemplate, tableName),
        StorageProvider.SqlServer => string.Format(SqlServerDropTableTemplate, tableName),
        StorageProvider.Sqlite => string.Format(DefaultDropTableTemplate, tableName),
        _ => throw new ArgumentOutOfRangeException()
      };
    }

    [Test]
    public void OneRow()
    {
      var schema = ExtractDefaultSchema();
      var table = schema.Tables[nameof(OneRow)];
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);

      var row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(1));
      row.Add(tableRef["FirstName"], SqlDml.Literal("Ivan"));
      row.Add(tableRef["LastName"], SqlDml.Literal("Smith"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 12)));

      insert.ValueRows.Add(row);

      Assert.That(insert.ValueRows.Count, Is.EqualTo(1));
      Assert.That(insert.ValueRows.Columns.Count, Is.EqualTo(4));

      var compiled = Driver.Compile(insert);
      var insertCommand = compiled.GetCommandText();
      Console.WriteLine(insertCommand);
      _ = ExecuteNonQuery(insertCommand);
    }

    [Test]
    public void MultiRowSameColumnOrder()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.Oracle);

      var schema = ExtractDefaultSchema();
      var table = schema.Tables[nameof(MultiRowSameColumnOrder)];
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);

      var row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(1));
      row.Add(tableRef["FirstName"], SqlDml.Literal("Ivan"));
      row.Add(tableRef["LastName"], SqlDml.Literal("Smith"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 12)));
      insert.ValueRows.Add(row);

      Assert.That(insert.ValueRows.Count, Is.EqualTo(1));
      Assert.That(insert.ValueRows.Columns.Count, Is.EqualTo(4));

      row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(2));
      row.Add(tableRef["FirstName"], SqlDml.Literal("NotIvan"));
      row.Add(tableRef["LastName"], SqlDml.Literal("NotSmith"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 14)));
      insert.ValueRows.Add(row);

      Assert.That(insert.ValueRows.Count, Is.EqualTo(2));
      Assert.That(insert.ValueRows.Columns.Count, Is.EqualTo(4));

      var compiled = Driver.Compile(insert);
      var insertCommand = compiled.GetCommandText();
      Console.WriteLine(insertCommand);
      _ = ExecuteNonQuery(insertCommand);
    }

    [Test]
    public void SameColumnsDifferentOrder()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.Oracle);

      var schema = ExtractDefaultSchema();
      var table = schema.Tables[nameof(SameColumnsDifferentOrder)];
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);

      var row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(1));
      row.Add(tableRef["FirstName"], SqlDml.Literal("Ivan"));
      row.Add(tableRef["LastName"], SqlDml.Literal("Smith"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 12)));
      insert.ValueRows.Add(row);

      Assert.That(insert.ValueRows.Count, Is.EqualTo(1));
      Assert.That(insert.ValueRows.Columns.Count, Is.EqualTo(4));

      row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(2));
      row.Add(tableRef["LastName"], SqlDml.Literal("NotSmith"));
      row.Add(tableRef["FirstName"], SqlDml.Literal("NotIvan"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 14)));
      insert.ValueRows.Add(row);

      Assert.That(insert.ValueRows.Count, Is.EqualTo(2));
      Assert.That(insert.ValueRows.Columns.Count, Is.EqualTo(4));

      var secondRow = insert.ValueRows[1];
      Assert.That(((SqlLiteral<int>) secondRow[0]).Value, Is.EqualTo(2));
      Assert.That(((SqlLiteral<string>) secondRow[1]).Value, Is.EqualTo("NotIvan"));
      Assert.That(((SqlLiteral<string>) secondRow[2]).Value, Is.EqualTo("NotSmith"));
      Assert.That(((SqlLiteral<DateTime>) secondRow[3]).Value, Is.EqualTo(new DateTime(2008, 10, 14)));

      var compiled = Driver.Compile(insert);
      var insertCommand = compiled.GetCommandText();
      Console.WriteLine(insertCommand);
      _ = ExecuteNonQuery(insertCommand);
    }

    [Test]
    public void LessColumnsThanExpected()
    {
      var schema = ExtractDefaultSchema();
      var table = schema.Tables[nameof(LessColumnsThanExpected)];
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);

      var row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(1));
      row.Add(tableRef["FirstName"], SqlDml.Literal("Ivan"));
      row.Add(tableRef["LastName"], SqlDml.Literal("Smith"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 12)));
      insert.ValueRows.Add(row);

      row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(2));
      row.Add(tableRef["FirstName"], SqlDml.Literal("NotIvan"));
      //row.Add(tableRef["LastName"], SqlDml.Literal("NotSmith"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 14)));
      _ = Assert.Throws<ArgumentException>(() => insert.ValueRows.Add(row));
    }

    [Test]
    public void SameCountDifferentColumns()
    {
      var schema = ExtractDefaultSchema();
      var table = schema.Tables[nameof(SameCountDifferentColumns)];
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);

      var row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(1));
      row.Add(tableRef["FirstName"], SqlDml.Literal("Ivan"));
      row.Add(tableRef["LastName"], SqlDml.Literal("Smith"));
      //row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 12)));
      insert.ValueRows.Add(row);

      row = new Dictionary<SqlColumn, SqlExpression>(4);
      row.Add(tableRef["Id"], SqlDml.Literal(2));
      //row.Add(tableRef["FirstName"], SqlDml.Literal("NotIvan"));
      row.Add(tableRef["LastName"], SqlDml.Literal("NotSmith"));
      row.Add(tableRef["Birthday"], SqlDml.Literal(new DateTime(2008, 10, 14)));
      _ = Assert.Throws<ArgumentException>(() => insert.ValueRows.Add(row));
    }

    [Test]
    public void NullOrEmptyRowTest()
    {
      var schema = ExtractDefaultSchema();
      var table = schema.Tables[nameof(NullOrEmptyRowTest)];
      var tableRef = SqlDml.TableRef(table);

      var insert = SqlDml.Insert(tableRef);
      _ = Assert.Throws<ArgumentNullException>(() => insert.ValueRows.Add(null));
      _ = Assert.Throws<ArgumentException>(() => insert.ValueRows.Add(new Dictionary<SqlColumn, SqlExpression>(4)));
    }
  }
}