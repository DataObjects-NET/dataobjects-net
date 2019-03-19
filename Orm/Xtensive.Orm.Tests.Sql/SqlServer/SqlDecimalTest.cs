// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.03.19

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql.Drivers.SqlServer.Internals;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  public class SqlDecimalTest
  {
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
    public static void SqlDecimalUtilsDividerTest()
    {
      var scale = 37;
      for (int i = 0; i < scale; i++) {
        var chars = Enumerable.Repeat('0', scale).ToArray();
        chars[i] = '1';
        var str = "1." + new string(chars);
        var input = SqlDecimal.Parse(str);
        var output = SqlDecimalUtils.TruncateToNetDecimal(input);
        var dec = decimal.Parse(str);
        Assert.That(output, Is.EqualTo(dec));
      }
    }

    [Test]
    public static void SqlDecimalUtilsBigScaleTest()
    {
      var sourceSqlDecimal = SqlDecimal.Parse("9" + string.Join("", Enumerable.Repeat('0', 37)));
      foreach (byte scale in Enumerable.Range(10, 29)) {
        var sqlDecimal = new SqlDecimal(
          sourceSqlDecimal.Precision,
          scale,
          sourceSqlDecimal.IsPositive,
          sourceSqlDecimal.Data);
        var output = SqlDecimalUtils.TruncateToNetDecimal(sqlDecimal);
        var dec = decimal.Parse(sqlDecimal.ToString());
        Assert.That(output, Is.EqualTo(dec));
      }
    }

    [Test, Explicit]
    public static void SqlDecimalUtilsPerformanceTest()
    {
      var sourceSqlDecimal = SqlDecimal.Parse("9" + string.Join("", Enumerable.Repeat('0', 37)));
      var counter = new Stopwatch();
      foreach (byte scale in Enumerable.Range(10, 29)) {
        var sqlDecimal = new SqlDecimal(
          sourceSqlDecimal.Precision,
          scale,
          sourceSqlDecimal.IsPositive,
          sourceSqlDecimal.Data);

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
    }
  }
}
