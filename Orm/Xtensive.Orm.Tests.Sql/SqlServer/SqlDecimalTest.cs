// Copyright (C) 2019-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2019.03.19

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Sql.Drivers.SqlServer;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  public class SqlDecimalTest : SqlTest
  {
    private readonly static byte[] TestScalesFull = Enumerable.Range(0, 37).Select(i => (byte) i).ToArray();
    private readonly static byte[] TestScaleShort = Enumerable.Range(10, 29).Select(i => (byte) i).ToArray();
    private readonly static byte[][] TestValuesParametersFast = GetTestCases(true).ToArray();
    private readonly static byte[][] TestValuesParametersFull = GetTestCases(false).ToArray();

    private readonly SqlDecimal sourceDecimal = SqlDecimal.Parse("9" + string.Join("", Enumerable.Repeat('0', 37)));

    protected override void CheckRequirements() =>
      Require.ProviderIs(StorageProvider.SqlServer);

    [Test]
    public void SqlDecimalUtilsTest()
    {
      var input = SqlDecimal.Parse("100000000000000000000000000.115000");
      var output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(100000000000000000000000000.11M));

      input = SqlDecimal.Parse("100000000000000000000000000.11");
      output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(100000000000000000000000000.11M));

      input = SqlDecimal.Parse("1000000000000000000000000000.11");
      output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(1000000000000000000000000000.1M));

      input = SqlDecimal.Parse("10000000000000000000000000011");
      output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(10000000000000000000000000011M));

      input = SqlDecimal.Parse("100000000000000000000000000000");
      _ = Assert.Throws<OverflowException>(() => InternalHelpers.TruncateToNetDecimal(input));

      input = new SqlDecimal(decimal.MaxValue) + 1;
      _ = Assert.Throws<OverflowException>(() => InternalHelpers.TruncateToNetDecimal(input));

      input = new SqlDecimal(decimal.MinValue) - 1;
      _ = Assert.Throws<OverflowException>(() => InternalHelpers.TruncateToNetDecimal(input));

      input = new SqlDecimal(decimal.MaxValue);
      output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(decimal.MaxValue));

      input = new SqlDecimal(decimal.MinValue);
      output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(decimal.MinValue));

      input = SqlDecimal.Parse("1.0000000000000000000000000000000");
      output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(1.0000M));

      input = SqlDecimal.Parse("1.0000000000000000000000000000");
      output = InternalHelpers.TruncateToNetDecimal(input);
      Assert.That(output, Is.EqualTo(1.0000000000000000000000000000M));
    }

    [Test]
    [TestCaseSource(nameof(TestScalesFull))]
    public void SqlDecimalUtilsDividerTest(byte scale)
    {
      var chars = Enumerable.Repeat('0', 37).ToArray();
      chars[scale] = '1';
      var str = "1." + new string(chars);
      var input = SqlDecimal.Parse(str);
      var output = InternalHelpers.TruncateToNetDecimal(input);
      var dec = decimal.Parse(str, CultureInfo.InvariantCulture);
      Assert.That(output, Is.EqualTo(dec));
    }

    [Test]
    [TestCaseSource(nameof(TestScaleShort))]
    public void SqlDecimalUtilsBigScaleTest(byte scale)
    {
      var sourceValue = sourceDecimal;

      var sqlDecimal = new SqlDecimal(sourceValue.Precision, scale, sourceValue.IsPositive, sourceValue.Data);
      var output = InternalHelpers.TruncateToNetDecimal(sqlDecimal);
      var dec = decimal.Parse(sqlDecimal.ToString(), CultureInfo.InvariantCulture);
      Assert.That(output, Is.EqualTo(dec));
    }

    [Test, Explicit]
    [TestCaseSource(nameof(TestScaleShort))]
    public void SqlDecimalUtilsPerformanceTest(byte scale)
    {
      var sourceValue = sourceDecimal;
      var counter = new Stopwatch();

      var sqlDecimal = new SqlDecimal(sourceValue.Precision, scale, sourceValue.IsPositive, sourceValue.Data);

      var list = new List<long>();
      for (var i = 0; i < 100; i++) {
        counter.Restart();

        for (var j = 0; j < 100000; j++) {
          _ = InternalHelpers.TruncateToNetDecimal(sqlDecimal);
        }

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
    [TestCaseSource(nameof(TestValuesParametersFast))]
    public void TypeMapperFastTest(byte precision, byte scale, byte zerosInPrecision, byte zerosInScale)
    {
      var datacount = 5000;
      var runsCount = 50;

      MapperTestRun(datacount, runsCount, precision, scale, zerosInPrecision, zerosInScale);
    }

    [Test]
    [Explicit]
    [TestCaseSource(nameof(TestValuesParametersFull))]
    public void TypeMapperFulltest(byte precision, byte scale, byte zerosInPrecision, byte zerosInScale)
    {
      var datacount = 5000;
      var runsCount = 100;

      MapperTestRun(datacount, runsCount, precision, scale, zerosInPrecision, zerosInScale);
    }


    private void MapperTestRun(int dataCount,
      int runsCount,
      byte precision, byte scale,
      byte zerosInPrecision, byte zerosInScale)
    {
      var runResult = new double[runsCount];
      var counter = new Stopwatch();

      var decimalValue = GetDecimalValue(precision, scale, zerosInPrecision, zerosInScale);
      Console.WriteLine(decimalValue);

      var mapping = Driver.TypeMappings[typeof(decimal)];
      using (var connection = Driver.CreateConnection()) {
        connection.Open();

        for (var i = 0; i < runsCount; i++) {
          var command = GetValuesProviderCommand(connection, dataCount, decimalValue, precision, scale);
          var reader = (SqlDataReader) command.ExecuteReader();
          counter.Reset();

          var rowsCount = 0;
          while (reader.Read()) {
            rowsCount++;
            counter.Start();
            _ = mapping.ReadValue(reader, 1);
            counter.Stop();
          }

          runResult[i] = counter.ElapsedTicks / (double) rowsCount;
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

      return (SqlCommand) connection.CreateCommand(commandText);
    }

    private string GetDecimalValue(int precision, int scale, int zerosInPrecision, int zerosInScale)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(precision, 1, "precision");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(precision, 38, "precision");
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(scale, 0, "scale");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(scale, precision - 1, "scale");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(zerosInPrecision, precision - scale, "zerosInPrecision");
      ArgumentValidator.EnsureArgumentIsLessThanOrEqual(zerosInScale, scale, "zerosInScale");

      if (zerosInScale > scale) {
        throw new ArgumentException("", "zerosInScale");
      }

      var meaningfulfloor = string.Join("", Enumerable.Repeat('1', scale - zerosInScale));
      var irrelevantFloor = string.Join("", Enumerable.Repeat('0', zerosInScale));
      var floor = meaningfulfloor + irrelevantFloor;
      var ceiling = string.Join("", Enumerable.Repeat('1', precision - scale - zerosInPrecision));

      return floor.Length + zerosInPrecision + ceiling.Length != precision
        ? throw new Exception("Precision and scale mismatch")
        : ceiling + "." + floor;
    }

    private static List<byte[]> GetTestCases(bool forFast)
    {
      byte basePresicion = 38;
      byte maxScale = 28;
      byte basePrecisionZeros = 0;

      var list = forFast ? new List<byte[]>(200) : new List<byte[]>(7500);

      for (byte scale = 0; scale < maxScale; scale++) {
        for (byte zerosInScale = 0; zerosInScale < scale; zerosInScale += 3) {
          if (!forFast) {
            for (byte precisionZeros = 0; precisionZeros < basePresicion - scale - 1; precisionZeros += 3) {
              list.Add(new[] { basePresicion, scale, precisionZeros, zerosInScale });
            }
          }
          else {
            list.Add(new[] { basePresicion, scale, basePrecisionZeros, zerosInScale });
          }
        }
      }

      return list;
    }

    private void OutputResults(double[] rawData)
    {
      Console.WriteLine("       Statistics        ");
      Console.WriteLine("-------------------------");

      var ordered = rawData.OrderBy(i => i).ToArray(rawData.Length);
      var length = rawData.Length;
      var median = (length % 2 == 0)
        ? (ordered[length / 2] + ordered[(length / 2) - 1]) / 2
        : ordered[length / 2];

      Console.WriteLine("Runs   : {0}", length);
      Console.WriteLine("Min    : {0}", ordered[0]);
      Console.WriteLine("Max    : {0}", ordered[length - 1]);
      Console.WriteLine("Average: {0}", rawData.Average());
      Console.WriteLine("Median : {0}", median);


      Console.WriteLine();

      Console.WriteLine("       Run results       ");
      Console.WriteLine("-------------------------");

      for (var i = 0; i < rawData.Length; i++) {
        Console.WriteLine("#{0} : {1}", (i + 1).ToString("0000"), rawData[i]);
      }
    }
  }
}
