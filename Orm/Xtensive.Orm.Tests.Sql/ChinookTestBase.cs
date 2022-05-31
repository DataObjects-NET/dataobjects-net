// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class ChinookTestBase
  {
    protected struct DbCommandExecutionResult
    {
      public int FieldCount;
      public string[] FieldNames;
      public int RowCount;

      public override string ToString()
      {
        if (FieldNames == null)
          FieldNames = new string[0];
        return string.Format("Fields: '{0}'; Rows: {1}", string.Join("', '", FieldNames), RowCount);
      }
    }

    protected SqlDriver sqlDriver;
    protected SqlConnection sqlConnection;

    protected virtual bool InMemory => false;

    protected virtual bool PerformanceCheck => false;

    protected int RunsPerGroup = 50;

    protected string Url => TestConnectionInfoProvider.GetConnectionUrl();

    protected Catalog Catalog { get; private set; }

    [OneTimeSetUp]
    public virtual void SetUp()
    {
      CheckRequirements();
      sqlDriver = TestSqlDriver.Create(Url);
      sqlConnection = sqlDriver.CreateConnection();
      if (InMemory) {
        var catalog = new Catalog("Dummy");
        var schema = catalog.CreateSchema("default");

        var creator = new ChinookSchemaCreator(sqlDriver);
        creator.CreateSchemaContent(schema);
        Catalog = catalog;
        return;
      }

      try {
        sqlConnection.Open();
      }
      catch (Exception exception) {
        Console.WriteLine(exception);
        throw;
      }
      try {
        sqlConnection.BeginTransaction();
        Catalog = sqlDriver.ExtractCatalog(sqlConnection);
        var schema = Catalog.DefaultSchema;

        var creator = new ChinookSchemaCreator(sqlDriver);
        creator.DropSchemaContent(sqlConnection, schema);
        creator.CreateSchemaContent(sqlConnection, schema);

        sqlConnection.Commit();
      }
      catch {
        sqlConnection.Rollback();
        throw;
      }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      try {
        if (sqlConnection!=null && sqlConnection.State!=ConnectionState.Closed)
          sqlConnection.Close();
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }

    protected virtual void CheckRequirements()
    {
    }

    protected static DbCommandExecutionResult GetExecuteDataReaderResult(IDbCommand cmd)
    {
      var result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        var rowCount = 0;
        var fieldCount = 0;
        var fieldNames = new string[0];
        using (var reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            if (rowCount == 0) {
              fieldCount = reader.FieldCount;
              fieldNames = new string[fieldCount];
              for (var i = 0; i < fieldCount; i++)
                fieldNames[i] = reader.GetName(i);
            }
            rowCount++;
          }
        }

        result.RowCount = rowCount;
        result.FieldCount = fieldCount;
        result.FieldNames = fieldNames;
      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected static DbCommandExecutionResult GetExecuteNonQueryResult(IDbCommand cmd)
    {
      var result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        result.RowCount = cmd.ExecuteNonQuery();
      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected string Compile(ISqlCompileUnit statement)
    {
      return PerformanceCheck
        ? CompileWithPerformanceCheck(statement)
        : CompileRegular(statement);
    }

    protected string CompileWithPerformanceCheck(ISqlCompileUnit statement)
    {
      var runs = new TimeSpan[RunsPerGroup * 5];
      var stopwatch = new Stopwatch();
      var compiledCommandText = sqlDriver.Compile(statement).GetCommandText();
      for (var i = 0; i < RunsPerGroup * 5; i++) {
        stopwatch.Start();
        _ = sqlDriver.Compile(statement).GetCommandText();
        stopwatch.Stop();

        runs[i] = stopwatch.Elapsed;
        stopwatch.Reset();
      }

      WriteDetailedStats(runs);
      return compiledCommandText;
    }

    protected string CompileRegular(ISqlCompileUnit statement)
    {
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      var compiledStatement = sqlDriver.Compile(statement);
      stopwatch.Stop();
      WriteDuration(stopwatch);
      return compiledStatement.GetCommandText();
    }

    private void WriteDuration(Stopwatch stopwatch) =>
      Console.WriteLine($"Compilation elapsed ticks: {stopwatch.Elapsed} ({stopwatch.ElapsedTicks} ticks)");

    private void WriteDetailedStats(TimeSpan[] values)
    {
      var runs = RunsPerGroup;
      var part1 = new ArraySegment<TimeSpan>(values, runs * 0, runs);
      var part2 = new ArraySegment<TimeSpan>(values, runs * 1, runs);
      var part3 = new ArraySegment<TimeSpan>(values, runs * 2, runs);
      var part4 = new ArraySegment<TimeSpan>(values, runs * 3, runs);
      var part5 = new ArraySegment<TimeSpan>(values, runs * 4, runs);

      var orderedTicks = values.Select(ts => ts.Ticks).OrderBy(t => t).ToArray(values.Length);
      var min = orderedTicks[0];
      var max = orderedTicks[^1];
      var avg = orderedTicks.Average();

      var p95barier = (int) (0.95 * values.Length);
      var p95Values = new ArraySegment<long>(orderedTicks, 0, p95barier);
      var p95Max = p95Values[^1];
      var p95Avg = p95Values.Average();
      var deltas = values.Select(d => Math.Abs(avg - d.Ticks));

      Console.WriteLine("-------------------------------------");
      Console.WriteLine($"Min: {min} ticks({new TimeSpan(min)})");
      Console.WriteLine($"Max: {max} ticks({new TimeSpan(max)})");
      Console.WriteLine($"P95Max {p95Max} ticks({new TimeSpan(p95Max)})");
      Console.WriteLine($"Avg: {avg} ticks");
      Console.WriteLine($"P95Avg: {p95Avg} ticks");

      Console.WriteLine("-------------------------------------");

      Console.WriteLine("--------------Raw values-------------");
      for (var i = 0; i < part1.Count; i++) {
        Console.WriteLine($"{part1[i].Ticks}    {part2[i].Ticks}    {part3[i].Ticks}    {part4[i].Ticks}    {part5[i].Ticks}");
      }
      Console.WriteLine("-------------------------------------");
    }
  }
}