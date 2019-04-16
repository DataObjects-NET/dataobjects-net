// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.03.19

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Sql.Drivers.SqlServer.Internals;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  public class SqlDecimalTest : SqlTest
  {
    private readonly SqlDecimal SourceDecimal = SqlDecimal.Parse("9" + string.Join("", Enumerable.Repeat('0', 37)));
    private readonly byte[] TestScalesFull = Enumerable.Range(0, 37).Select(i=> (byte) i).ToArray();
    private readonly byte[] TestScaleShort = Enumerable.Range(10, 29).Select(i => (byte)i).ToArray();

    public byte[][] TestValuesParametersFast { get { return GetTestCases(true).ToArray(); } }

    public byte[][] TestValuesParametersFull { get { return GetTestCases(false).ToArray(); } }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    [Test]
    public void SqlDecimalUtilsTest()
    {
      var input = SqlDecimal.Parse("100000000000000000000000000.115000");
      var output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(100000000000000000000000000.11M));

      input = SqlDecimal.Parse("100000000000000000000000000.11");
      output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(100000000000000000000000000.11M));

      input = SqlDecimal.Parse("1000000000000000000000000000.11");
      output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(1000000000000000000000000000.1M));

      input = SqlDecimal.Parse("10000000000000000000000000011");
      output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(10000000000000000000000000011M));

      input = SqlDecimal.Parse("100000000000000000000000000000");
      Assert.Throws<OverflowException>(() => SqlDecimalUtils.TruncateToNetDecimal(input));

      input = new SqlDecimal(decimal.MaxValue) + 1;
      Assert.Throws<OverflowException>(() => SqlDecimalUtils.TruncateToNetDecimal(input));

      input = new SqlDecimal(decimal.MinValue) - 1;
      Assert.Throws<OverflowException>(() => SqlDecimalUtils.TruncateToNetDecimal(input));

      input = new SqlDecimal(decimal.MaxValue);
      output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(decimal.MaxValue));

      input = new SqlDecimal(decimal.MinValue);
      output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(decimal.MinValue));

      input = SqlDecimal.Parse("1.0000000000000000000000000000000");
      output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(1.0000M));

      input = SqlDecimal.Parse("1.0000000000000000000000000000");
      output = SqlDecimalUtils.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(1.0000000000000000000000000000M));
    }

    [Test]
    [TestCaseSource("TestScalesFull")]
    public void SqlDecimalUtilsDividerTest(byte scale)
    {
      var chars = Enumerable.Repeat('0', 37).ToArray();
      chars[scale] = '1';
      var str = "1." + new string(chars);
      var input = SqlDecimal.Parse(str);
      var output = SqlDecimalUtils.TruncateToNetDecimal(input);
      var dec = decimal.Parse(str, CultureInfo.InvariantCulture);
      Assert.That(output, Is.EqualTo(dec));
    }

    [Test]
    [TestCaseSource("TestScaleShort")]
    public void SqlDecimalUtilsBigScaleTest(byte scale)
    {
      var sourceValue = SourceDecimal;

      var sqlDecimal = new SqlDecimal( sourceValue.Precision, scale, sourceValue.IsPositive, sourceValue.Data);
      var output = SqlDecimalUtils.TruncateToNetDecimal(sqlDecimal);
      var dec = decimal.Parse(sqlDecimal.ToString(), CultureInfo.InvariantCulture);
      Assert.That(output, Is.EqualTo(dec));
    }

    [Test, Explicit]
    [TestCaseSource("TestScaleShort")]
    public void SqlDecimalUtilsPerformanceTest(byte scale)
    {
      var sourceValue = SourceDecimal;
      var counter = new Stopwatch();

      var sqlDecimal = new SqlDecimal(sourceValue.Precision, scale, sourceValue.IsPositive, sourceValue.Data);

      var list = new List<long>();
      for (var i = 0; i < 100; i++) {
        counter.Restart();

        for (var j = 0; j < 100000; j++)
          SqlDecimalUtils.TruncateToNetDecimal(sqlDecimal);

        counter.Stop();
        list.Add(counter.ElapsedMilliseconds);
      }

      Console.WriteLine(
        "Val:{0}  Scale:{1}  Min:{2}  Avg:{3}",
        sqlDecimal,
        sqlDecimal.Scale,
        list.Min(),
        list.Average());
    }

    [Test]
    [Explicit]
    [TestCaseSource("TestValuesParametersFast")]
    public void TypeMapperFastTest(byte precision, byte scale, byte zerosInPrecision, byte zerosInScale)
    {
      var datacount = 5000;
      var runsCount = 50;

      MapperTestRun(datacount, runsCount, precision, scale, zerosInPrecision, zerosInScale);
    }

    [Test]
    [Explicit]
    [TestCaseSource("TestValuesParametersFull")]
    public void TypeMapperFulltest(byte precision, byte scale, byte zerosInPrecision, byte zerosInScale)
    {
      var datacount = 5000;
      var runsCount = 100;

      MapperTestRun(datacount, runsCount, precision, scale, zerosInPrecision, zerosInScale);
    }


    private void MapperTestRun(int dataCount, int runsCount, byte precision, byte scale, byte zerosInPrecision, byte zerosInScale)
    {
      double[] runResult = new double[runsCount];
      Stopwatch counter = new Stopwatch();

      var decimalValue = GetDecimalValue(precision, scale, zerosInPrecision, zerosInScale);
      Console.WriteLine(decimalValue);

      var mapping = Driver.TypeMappings[typeof (decimal)];
      using (var connection = Driver.CreateConnection()) {
        connection.Open();

        for (int i = 0; i < runsCount; i++) {
          var command = GetValuesProviderCommand(connection, dataCount, decimalValue, precision, scale);
          var reader = (SqlDataReader)command.ExecuteReader();
          counter.Reset();

          int rowsCount = 0;
          while (reader.Read()) {
            rowsCount++;
            counter.Start();
            mapping.ReadValue(reader, 1);
            counter.Stop();
          }

          runResult[i] = counter.ElapsedTicks / (double)rowsCount;
        }
      }

      OutputResults(runResult);
    }


    private SqlCommand GetValuesProviderCommand(Xtensive.Sql.SqlConnection connection, int valuesCount, string repeatableValue, int precision, int scale)
    {
      const string randomizer = @"with randowvalues
        as(
      select 1 id, cast({1} as decimal({2},{3})) as randomnumber
        union  all
        select id + 1, cast({1} as decimal({2},{3})) as randomnumber
        from randowvalues
        where 
      id < {0}
        )
 
  
      select *
        from randowvalues
      OPTION(MAXRECURSION 0)";

      var commandText = string.Format(randomizer, valuesCount, repeatableValue, precision, scale);

      return (SqlCommand)connection.CreateCommand(commandText);
    }

    private string GetDecimalValue(int precision, int scale, int zerosInPrecision, int zerosInScale)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(precision, 1, "precision");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(precision, 38, "precision");
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(scale, 0, "scale");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(scale, precision - 1, "scale");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(zerosInPrecision, precision - scale, "zerosInPrecision");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(zerosInScale, scale, "zerosInScale");

      if (zerosInScale > scale)
        throw new ArgumentException("", "zerosInScale");

      var meaningfulfloor = string.Join("", Enumerable.Repeat('1', scale - zerosInScale));
      var irrelevantFloor = string.Join("", Enumerable.Repeat('0', zerosInScale));
      var floor = meaningfulfloor + irrelevantFloor;
      var ceiling = string.Join("", Enumerable.Repeat('1', precision - scale - zerosInPrecision));

      if (floor.Length + zerosInPrecision + ceiling.Length != precision)
        throw new Exception("Precision and scale mismatch");
      return ceiling + "." + floor;
    }

    private List<byte[]> GetTestCases(bool forFast)
    {
      byte basePresicion = 38;
      byte maxScale = 28;
      byte basePrecisionZeros = 0;
      byte baseScaleZeros = 0;

      var list = (forFast) ? new List<byte[]>(200) : new List<byte[]>(7500);

      for (byte scale = 0; scale < maxScale; scale++)
        for (byte zerosInScale = 0; zerosInScale < scale; zerosInScale += 3)
          if (!forFast)
            for (byte precisionZeros = 0; precisionZeros < basePresicion - scale - 1; precisionZeros += 3)
              list.Add(new[] { basePresicion, scale, precisionZeros, zerosInScale });
          else
            list.Add(new[] { basePresicion, scale, basePrecisionZeros, zerosInScale });

      return list;
    }

    private void OutputResults(double[] rawData)
    {
      Console.WriteLine("       Statistics        ");
      Console.WriteLine("-------------------------");

      var ordered = rawData.OrderBy(i => i).ToArray();
      var length = rawData.Length;
      double median = (length % 2 == 0)
        ? (ordered[length / 2] + ordered[length / 2 - 1]) / 2
        : ordered[length / 2];

      Console.WriteLine("Runs   : {0}", length);
      Console.WriteLine("Min    : {0}", ordered[0]);
      Console.WriteLine("Max    : {0}", ordered[length - 1]);
      Console.WriteLine("Average: {0}", rawData.Average());
      Console.WriteLine("Median : {0}", median);


      Console.WriteLine();

      Console.WriteLine("       Run results       ");
      Console.WriteLine("-------------------------");

      for (int i = 0; i < rawData.Length; i++)
        Console.WriteLine("#{0} : {1}", (i + 1).ToString("0000"), rawData[i]);
    }
  }
}
