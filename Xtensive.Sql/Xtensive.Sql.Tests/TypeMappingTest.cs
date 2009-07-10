// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.08

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;

namespace Xtensive.Sql.Tests
{
  [TestFixture]
  public abstract class TypeMappingTest
  {
    private const string IdParameterName = "PId";
    private const string IdColumnName = "Id";
    private const string TableName = "TypeMappingTest";

    private SqlDriver driver;
    private SqlConnection connection;
    private TypeMapping[] typeMappings;
    private object[][] testValues;

    protected abstract string Url { get; }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      var parsedUrl = new SqlConnectionUrl(Url);
      driver = SqlDriver.Create(parsedUrl);
      connection = driver.CreateConnection(parsedUrl);
      typeMappings = driver.TypeMappings.ToArray();
      testValues = typeMappings
        .Select(mapping => GetTestValues(mapping.Type))
        .ToArray();
      connection.Open();
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        connection.Close();
    }
    
    [Test]
    public void InsertAndSelectTest()
    {
      Catalog model;
      using (var transaction = connection.BeginTransaction()) {
        model = driver.ExtractModel(connection, transaction);
        transaction.Commit();
      }
      var table = model.DefaultSchema.Tables[TableName];
      if (table!=null) {
        using (var dropCommand = connection.CreateCommand(SqlDdl.Drop(table)))
          dropCommand.ExecuteNonQuery();
        model.DefaultSchema.Tables.Remove(table);
      }
      table = model.DefaultSchema.CreateTable(TableName);
      var idColumn = table.CreateColumn(IdColumnName, new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_" + TableName, idColumn);
      for (int columnIndex = 0; columnIndex < typeMappings.Length; columnIndex++) {
        var mapping = typeMappings[columnIndex];
        var column = table.CreateColumn(GetColumnName(columnIndex), mapping.BuildSqlType());
        column.IsNullable = true;
      }
      using (var createCommand = connection.CreateCommand(SqlDdl.Create(table)))
        createCommand.ExecuteNonQuery();
      var tableRef = SqlDml.TableRef(table);
      using (var insertCommand = connection.CreateCommand()) {
        var insertQuery = SqlDml.Insert(tableRef);
        var idParameter = insertCommand.CreateParameter();
        idParameter.DbType = DbType.Int32;
        idParameter.ParameterName = IdParameterName;
        insertCommand.Parameters.Add(idParameter);
        insertQuery.Values.Add(tableRef[IdColumnName], SqlDml.ParameterRef(IdParameterName));
        var parameters = new List<DbParameter>();
        for (int columnIndex = 0; columnIndex < typeMappings.Length; columnIndex++) {
          var mapping = typeMappings[columnIndex];
          var parameterName = GetParameterName(columnIndex);
          SqlExpression parameterExpression = SqlDml.ParameterRef(parameterName);
          if (mapping.ParameterCastRequired)
            parameterExpression = SqlDml.Cast(parameterExpression, mapping.BuildSqlType());
          insertQuery.Values.Add(tableRef[GetColumnName(columnIndex)], parameterExpression);
          var parameter = insertCommand.CreateParameter();
          parameter.ParameterName = parameterName;
          parameters.Add(parameter);
          insertCommand.Parameters.Add(parameter);
        }
        var insertQueryText = driver.Compile(insertQuery).GetCommandText();
        insertCommand.CommandText = insertQueryText;
        for (int rowIndex = 0; rowIndex < testValues[0].Length; rowIndex++) {
          idParameter.Value = rowIndex;
          for (int columnIndex = 0; columnIndex < typeMappings.Length; columnIndex++)
            typeMappings[columnIndex].SetParameterValue(parameters[columnIndex], testValues[columnIndex][rowIndex]);
          insertCommand.ExecuteNonQuery();
        }
      }
      var resultQuery = SqlDml.Select(tableRef);
      resultQuery.Columns.Add(SqlDml.Asterisk);
      resultQuery.OrderBy.Add(tableRef[IdColumnName]);
      using (var resultCommand = connection.CreateCommand(resultQuery))
        VerifyTestValues(resultCommand.ExecuteReader());
    }

    [Test]
    public void SelectFromParametersTest()
    {
      int parameterIndex = 0;
      var queries = new List<SqlSelect>();
      var command = connection.CreateCommand();
      for (int rowIndex = 0; rowIndex < testValues[0].Length; rowIndex++) {
        var query = SqlDml.Select();
        queries.Add(query);
        query.Columns.Add(SqlDml.Literal(rowIndex), IdColumnName);
        for (int columnIndex = 0; columnIndex < testValues.Length; columnIndex++) {
          var columnName = GetColumnName(columnIndex);
          var parameterName = GetParameterName(parameterIndex++);
          SqlExpression parameterExpression = SqlDml.ParameterRef(parameterName);
          if (typeMappings[columnIndex].ParameterCastRequired)
            parameterExpression = SqlDml.Cast(parameterExpression, typeMappings[columnIndex].BuildSqlType());
          query.Columns.Add(parameterExpression, columnName);
          var parameter = command.CreateParameter();
          parameter.ParameterName = parameterName;
          typeMappings[columnIndex].SetParameterValue(parameter, testValues[columnIndex][rowIndex]);
          command.Parameters.Add(parameter);
        }
      }
      var unionQueryRef = SqlDml.QueryRef(queries.Cast<ISqlQueryExpression>().Aggregate(SqlDml.UnionAll));
      var resultQuery = SqlDml.Select(unionQueryRef);
      resultQuery.Columns.Add(SqlDml.Asterisk);
      resultQuery.OrderBy.Add(unionQueryRef[IdColumnName]);
      command.CommandText = driver.Compile(resultQuery).GetCommandText();
      VerifyTestValues(command.ExecuteReader());
    }

    private void VerifyTestValues(DbDataReader reader)
    {
      int rowIndex = 0;
      using (reader)
      while (reader.Read()) {
        for (int columnIndex = 0; columnIndex < testValues.Length; columnIndex++) {
          var value = !reader.IsDBNull(columnIndex + 1)
            ? typeMappings[columnIndex].ReadValue(reader, columnIndex + 1)
            : null;
          Assert.AreEqual(testValues[columnIndex][rowIndex], value);
        }
        rowIndex++;
      }
      Assert.AreEqual(testValues[0].Length, rowIndex);
    }
    
    private static object[] GetTestValues(Type type)
    {
      // NOTE: there should be the same number of test values for each type
      switch (Type.GetTypeCode(type)) {
      case TypeCode.Boolean:
        return new object[] {default(bool), false, true, null};
      case TypeCode.Char:
        return new object[] {default(char), 'Y', '\n', null};
      case TypeCode.String:
        return new object[] {"write code", "??????", "profit", null};
      case TypeCode.Byte:
        return new object[] {default(byte), (byte) 10, (byte) 20, null};
      case TypeCode.SByte:
        return new object[] {default(sbyte), (sbyte) -10, (sbyte) 10, null};
      case TypeCode.Int16:
        return new object[] {default(short), (short) (sbyte.MinValue - 1), (short) (sbyte.MaxValue + 1), null};
      case TypeCode.UInt16:
        return new object[] {default(ushort), (ushort) 10, (ushort) (short.MaxValue + 1), null};
      case TypeCode.Int32:
        return new object[] {default(int), short.MinValue - 1, short.MaxValue + 1, null};
      case TypeCode.UInt32:
        return new object[] {default(uint), (uint) 10, ((uint) int.MaxValue + 1), null};
      case TypeCode.Int64:
        return new object[] {default(long), ((long) int.MinValue - 1), ((long) int.MaxValue + 1), null};
      case TypeCode.UInt64:
        return new object[] {default(ulong), (ulong) 10, ((ulong) long.MaxValue + 1), null};
      case TypeCode.Single:
        return new object[] {default(float), -5.55f, 0.34f, null};
      case TypeCode.Double:
        return new object[] {default(double), 3.98d, -3.3333d, null};
      case TypeCode.Decimal:
        return new object[] {default(decimal), 222.4444m, -0.0005m, null};
      case TypeCode.DateTime:
        return new object[]
          {
            new DateTime(2005, 5, 5, 5, 5, 5),
            new DateTime(1998, 8, 8, 8, 8, 8),
            new DateTime(1856, 4, 1, 5, 6, 7),
            null
          };
      }
      if (type==typeof(TimeSpan))
        return new object[]
          {
            new TimeSpan(10, 10, 10, 10),
            new TimeSpan(-3, -3, -3, -3),
            new TimeSpan(113, 4, 6, 8),
            null
          };
      if (type==typeof(Guid))
        return new object[] 
          {
            new Guid("{13826748-1625-4934-8CDC-3B2047138DD5}"),
            new Guid("{42D3CD2D-E909-4a7c-8C43-B35DE4BF4740}"),
            new Guid("{EA8496CC-2034-458f-91AA-2A77BCA407DA}"),
            null
          };
      if (type==typeof(byte[]))
        return new object[]
          {
            new byte[0],
            new [] {(byte) 5, (byte) 6, (byte)8},
            new [] {(byte) 0},
            null,
          };
      throw new ArgumentOutOfRangeException();
    }

    private static string GetParameterName(int parameterIndex)
    {
      return string.Format("P{0}", parameterIndex);
    }

    private static string GetColumnName(int columnIndex)
    {
      return string.Format("C{0}", columnIndex);
    }
  }
}