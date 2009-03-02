// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.27

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Tests
{
  [TestFixture]
  public abstract class DateTimeIntervalTest
  {
    protected abstract string Url { get; }

    private SqlConnectionProvider provider;
    private SqlConnection connection;

    [TestFixtureSetUp]
    public void SetUp()
    {
      provider = new SqlConnectionProvider();
      connection = provider.CreateConnection(Url) as SqlConnection;
      connection.Open();
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      if (connection != null && connection.State == ConnectionState.Open)
        connection.Close();
    }

    [Test]
    public void ExtractDayOfWeekTest()
    {
      PerformTest(
        Sql.Extract(SqlDateTimePart.DayOfWeek, new DateTime(2009, 3, 2)) == (int)DayOfWeek.Monday
        );
    }

    [Test]
    public void ExtractDayOfYearTest()
    {
      PerformTest(
        Sql.Extract(SqlDateTimePart.DayOfYear, new DateTime(2005, 2, 2)) == 33
        );
    }
  
    [Test]
    public void DateTimeAddIntervalTest()
    {
      PerformTest(
        Sql.DateTimeAddInterval(new DateTime(2001, 1, 1, 1, 1, 1, 1), new TimeSpan(10, 10, 10, 10, 10))
          ==Sql.Literal(new DateTime(2001, 1, 11, 11, 11, 11, 11))
        );
    }

    [Test]
    public void DateTimeAddMonthsTest()
    {
      PerformTest(
        Sql.DateTimeAddMonths(new DateTime(2001, 1, 1), 15) == Sql.Literal(new DateTime(2002, 4, 1))
        );
    }

    [Test]
    public void DateTimeAddYearsTest()
    {
      PerformTest(
        Sql.DateTimeAddYears(new DateTime(2001, 1, 1), 5) == Sql.Literal(new DateTime(2006, 1, 1))
        );
    }

    [Test]
    public void DateTimeConstructTest()
    {
      PerformTest(
        Sql.DateTimeConstruct(2005, 5, 5)==Sql.Literal(new DateTime(2005, 5, 5))
        );
    }

    [Test]
    public void DateTimeSubtractDateTimeTest()
    {
      PerformTest(
        Sql.DateTimeSubtractDateTime(new DateTime(2005, 5, 5, 5, 5, 5), new DateTime(2005, 5, 6, 6, 6, 6))
          ==Sql.Literal(new TimeSpan(1, 1, 1, 1).Negate())
        );
    }

    [Test]
    public void DateTimeSubtractIntervalTest()
    {
      PerformTest(
        Sql.DateTimeSubtractInterval(new DateTime(2005, 5, 5, 5, 5, 5, 5), new TimeSpan(4, 4, 4, 4, 4))
          == Sql.Literal(new DateTime(2005, 5, 1, 1, 1, 1, 1))
        );
    }

    [Test]
    public void DateTimeTruncateTest()
    {
      PerformTest(
        Sql.DateTimeTruncate(new DateTime(2005, 1, 1, 1, 1, 1, 1)) == Sql.Literal(new DateTime(2005, 1, 1))
        );
    }

    [Test]
    public void IntervalConstructTest()
    {
      PerformTest(
        Sql.IntervalConstruct(500)==Sql.Literal(new TimeSpan(0, 0, 0, 0, 500))
        );
    }

    [Test]
    public void IntervalExtractDayTest()
    {
      PerformTest(
        Sql.IntervalExtract(SqlIntervalPart.Day, new TimeSpan(6, 5, 4, 3, 2)) == 6
        );
    }

    [Test]
    public void IntervalExtractHourTest()
    {
      PerformTest(
        Sql.IntervalExtract(SqlIntervalPart.Hour, new TimeSpan(6, 5, 4, 3, 2)) == 5
        );
    }

    [Test]
    public void IntervalExtractMinuteTest()
    {
      PerformTest(
        Sql.IntervalExtract(SqlIntervalPart.Minute, new TimeSpan(6, 5, 4, 3, 2)) == 4
        );
    }

    [Test]
    public void IntervalExtractSecondTest()
    {
      PerformTest(
        Sql.IntervalExtract(SqlIntervalPart.Second, new TimeSpan(6, 5, 4, 3, 2)) == 3
        );
    }

    [Test]
    public void IntervalExtractMillisecondTest()
    {
      PerformTest(
        Sql.IntervalExtract(SqlIntervalPart.Millisecond, new TimeSpan(6, 5, 4, 3, 2))==2
        );
    }

    [Test]
    public void IntervalToMillisecondsTest()
    {
      PerformTest(
        Sql.IntervalToMilliseconds(new TimeSpan(0, 0, 8, 5, 5))
          == (int)new TimeSpan(0, 0, 8, 5, 5).TotalMilliseconds
        );
    }

    private void PerformTest(SqlExpression expression)
    {
      var select = Sql.Select();
      select.Columns.Add(expression);

      using (var command = connection.CreateCommand(select))
        using (var reader = command.ExecuteReader()) {
          reader.Read();
          if (!reader.GetBoolean(0))
            Assert.Fail(string.Format("expression \"{0}\" evaluated to false", command.CommandText));
      }
    }
  }
}
