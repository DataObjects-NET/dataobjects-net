// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class TypeMappingTest : SqlTest
  {
    private const string IdParameterName = "PId";
    private const string IdColumnName = "Id";
    private const string TableName = "TypeMappingTest";

    private TypeMapping[] typeMappings;
    private object[][] testValues;
    
    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      typeMappings = Driver.TypeMappings.Where(mapping => StringComparer.CurrentCultureIgnoreCase.Compare(mapping.Type.Namespace, "System")==0).ToArray();
      testValues = typeMappings
        .Select(mapping => GetTestValues(mapping.Type))
        .ToArray();
    }
    
    [Test, Ignore("SQLFIXME")]
    public void InsertAndSelectTest()
    {
      var schema = ExtractDefaultSchema();
      EnsureTableNotExists(schema, TableName);
      var table = schema.CreateTable(TableName);
      var idColumnType = Driver.TypeMappings[typeof (int)].MapType();
      var idColumn = table.CreateColumn(IdColumnName, idColumnType);
      table.CreatePrimaryKey("PK_" + TableName, idColumn);
      for (int columnIndex = 0; columnIndex < typeMappings.Length; columnIndex++) {
        var mapping = typeMappings[columnIndex];
        var column = table.CreateColumn(GetColumnName(columnIndex), mapping.MapType());
        column.IsNullable = true;
      }
      ExecuteNonQuery(SqlDdl.Create(table));
      var tableRef = SqlDml.TableRef(table);
      using (var insertCommand = Connection.CreateCommand()) {
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
            parameterExpression = SqlDml.Cast(parameterExpression, mapping.MapType());
          insertQuery.Values.Add(tableRef[GetColumnName(columnIndex)], parameterExpression);
          var parameter = insertCommand.CreateParameter();
          parameter.ParameterName = parameterName;
          parameters.Add(parameter);
          insertCommand.Parameters.Add(parameter);
        }
        var insertQueryText = Driver.Compile(insertQuery).GetCommandText();
        insertCommand.CommandText = insertQueryText;
        for (int rowIndex = 0; rowIndex < testValues[0].Length; rowIndex++) {
          idParameter.Value = rowIndex;
          for (int columnIndex = 0; columnIndex < typeMappings.Length; columnIndex++)
            typeMappings[columnIndex].BindValue(parameters[columnIndex], testValues[columnIndex][rowIndex]);
          insertCommand.ExecuteNonQuery();
        }
      }
      var resultQuery = SqlDml.Select(tableRef);
      resultQuery.Columns.Add(SqlDml.Asterisk);
      resultQuery.OrderBy.Add(tableRef[IdColumnName]);
      VerifyResults(Connection.CreateCommand(resultQuery));
    }

    [Test, Ignore("SQLFIXME")]
    public void SelectParametersTest()
    {
      int parameterIndex = 0;
      var queries = new List<SqlSelect>();
      var command = Connection.CreateCommand();
      for (int rowIndex = 0; rowIndex < testValues[0].Length; rowIndex++) {
        var query = SqlDml.Select();
        queries.Add(query);
        query.Columns.Add(SqlDml.Literal(rowIndex), IdColumnName);
        for (int columnIndex = 0; columnIndex < testValues.Length; columnIndex++) {
          var mapping = typeMappings[columnIndex];
          var columnName = GetColumnName(columnIndex);
          var parameterName = GetParameterName(parameterIndex++);
          SqlExpression parameterExpression = SqlDml.ParameterRef(parameterName);
          if (mapping.ParameterCastRequired)
            parameterExpression = SqlDml.Cast(parameterExpression, mapping.MapType());
          query.Columns.Add(parameterExpression, columnName);
          var parameter = command.CreateParameter();
          parameter.ParameterName = parameterName;
          typeMappings[columnIndex].BindValue(parameter, testValues[columnIndex][rowIndex]);
          command.Parameters.Add(parameter);
        }
      }
      var unionQueryRef = SqlDml.QueryRef(queries.Cast<ISqlQueryExpression>().Aggregate(SqlDml.UnionAll));
      var resultQuery = SqlDml.Select(unionQueryRef);
      resultQuery.Columns.Add(SqlDml.Asterisk);
      resultQuery.OrderBy.Add(unionQueryRef[IdColumnName]);
      command.CommandText = Driver.Compile(resultQuery).GetCommandText();
      VerifyResults(command);
    }

    [Test, Ignore("SQLFIXME")]
    public void SelectConstantsTest()
    {
      var queries = new List<SqlSelect>();
      var command = Connection.CreateCommand();
      for (int rowIndex = 0; rowIndex < testValues[0].Length; rowIndex++) {
        var query = SqlDml.Select();
        queries.Add(query);
        query.Columns.Add(SqlDml.Literal(rowIndex), IdColumnName);
        for (int columnIndex = 0; columnIndex < testValues.Length; columnIndex++) {
          var columnName = GetColumnName(columnIndex);
          var value = testValues[columnIndex][rowIndex];
          var mapping = typeMappings[columnIndex];
          var valueExpression = value==null
            ? (SqlExpression) SqlDml.Null
            : SqlDml.Literal(value);
          query.Columns.Add(valueExpression, columnName);
        }
      }
      var unionQueryRef = SqlDml.QueryRef(queries.Cast<ISqlQueryExpression>().Aggregate(SqlDml.UnionAll));
      var resultQuery = SqlDml.Select(unionQueryRef);
      resultQuery.Columns.Add(SqlDml.Asterisk);
      resultQuery.OrderBy.Add(unionQueryRef[IdColumnName]);
      command.CommandText = Driver.Compile(resultQuery).GetCommandText();
      VerifyResults(command, true);
    }

    protected virtual void CheckEquality(object expected, object actual)
    {
      Assert.AreEqual(expected, actual);
    }

    protected virtual object ReadValue(TypeMapping mapping, DbDataReader reader, int index)
    {
      return mapping.ReadValue(reader, index);
    }

    private void VerifyResults(DbCommand command)
    {
      VerifyResults(command, false);
    }

    private void VerifyResults(DbCommand command, bool skipCheckForByteArray)
    {
      using (command)
      using (var reader = command.ExecuteReader()) {
        int rowIndex = 0;
        while (reader.Read()) {
          for (int columnIndex = 0; columnIndex < testValues.Length; columnIndex++) {
            var expectedValue = testValues[columnIndex][rowIndex];
            if (expectedValue is byte[] && skipCheckForByteArray)
              continue; // stupid hack for oracle
            var actualValue = !reader.IsDBNull(columnIndex + 1)
              ? ReadValue(typeMappings[columnIndex], reader, columnIndex + 1)
              : null;
            CheckEquality(expectedValue, actualValue);
          }
          rowIndex++;
        }
        Assert.AreEqual(testValues[0].Length, rowIndex);
      }
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
            new [] {(byte) 5, (byte) 6, (byte) 8},
            new [] {byte.MinValue, byte.MaxValue},
            null,
          };
      if (type==typeof(DateTimeOffset))
        return new object[] {
                              new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0)),
                              new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 11, 0)),
                              new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(3, 10, 0)),
                              null
                            };
      throw new ArgumentOutOfRangeException();
    }

    private static string GetParameterName(int parameterIndex)
    {
      return string.Format("P{0:00}", parameterIndex);
    }

    private static string GetColumnName(int columnIndex)
    {
      return string.Format("C{0:00}", columnIndex);
    }
  }
}