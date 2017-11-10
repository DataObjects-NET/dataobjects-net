// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.22

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.MySQL
{
  [TestFixture, Explicit]
  public class DateTimeIntervalTests : Sakila
  {
    private DbCommand sqlCommand;


    #region Internals

    private void CompareColumnEquality(ISqlCompileUnit statement)
    {
      sqlCommand.CommandText = SqlDriver.Compile(statement).GetCommandText();
      sqlCommand.Prepare();

      using (var command = sqlCommand.Connection.CreateCommand())
      {
        Console.WriteLine(sqlCommand.CommandText);
        using (var reader = sqlCommand.ExecuteReader()) {
          Assert.IsTrue(reader.Read());
          Assert.AreEqual(reader[0], reader[1], "The columns are not equal!");
        }
      }
    }

    #endregion

    #region Setup and TearDown

#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public override void SetUp()
    {
      CheckRequirements();
      SqlDriver = TestSqlDriver.Create(ConnectionInfo.ConnectionUrl.Url);
      SqlConnection = SqlDriver.CreateConnection();
      SqlConnection.Open();
      sqlCommand = SqlConnection.CreateCommand();
    }

    #endregion

    [Test]
    public virtual void DateTimeAddIntervalTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimePlusInterval(new DateTime(2001, 1, 1, 1, 1, 1, 1), new TimeSpan(10, 10, 10, 10, 10)));
      select.Columns.Add(new DateTime(2001, 1, 11, 11, 11, 11, 11));
      CompareColumnEquality(select);
    }

    [Test]
    public virtual void DateTimeAddYearsTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeAddYears(new DateTime(2001, 1, 1), 5));
      select.Columns.Add(new DateTime(2006, 1, 1));
      CompareColumnEquality(select);
    }

    [Test]
    public virtual void DateTimeAddMonthsTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeAddMonths(new DateTime(2001, 1, 1), 15));
      select.Columns.Add(new DateTime(2002, 4, 1));
      CompareColumnEquality(select);
    }

    [Test]
    public virtual void DateTimeConstructTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.DateTimeConstruct(2005, 5, 5));
      select.Columns.Add(new DateTime(2005, 5, 5));
      CompareColumnEquality(select);
    }

    [Test]
    public virtual void DateTimeSubtractDateTimeTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.DateTimeMinusDateTime(new DateTime(2005, 5, 5, 5, 5, 5), new DateTime(2005, 5, 6, 6, 6, 6)));
      select.Columns.Add(new TimeSpan(1, 1, 1, 1).Negate());
      CompareColumnEquality(select);
    }


    [Test]
    public virtual void DateTimeSubtractIntervalTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
          SqlDml.DateTimeMinusInterval(new DateTime(2005, 5, 5, 5, 5, 5, 5), new TimeSpan(4, 4, 4, 4, 4)));
      select.Columns.Add(new DateTime(2005, 5, 1, 1, 1, 1, 1));
      CompareColumnEquality(select);
    }

    [Test]
    public virtual void DateTimeTruncateTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
          SqlDml.DateTimeTruncate(new DateTime(2005, 1, 1, 1, 1, 1, 1)));
      select.Columns.Add(new DateTime(2005, 1, 1));
      CompareColumnEquality(select);
    }

    [Test]
    public virtual void DateTimeExtractYearTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.Year, new DateTime(2006, 5, 4)));
      select.Columns.Add(2006);
      CompareColumnEquality(select);
    }

    [Test]
    public virtual void DateTimeExtractDayTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.Day, new DateTime(2006, 5, 4)));
      select.Columns.Add(4);
    }

    [Test]
    public virtual void DateTimeExtractHourTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.Hour, new DateTime(2006, 5, 4, 3, 2, 1, 333)));
      select.Columns.Add(3);
    }

    [Test]
    public virtual void DateTimeExtractMinuteTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.Minute, new DateTime(2006, 5, 4, 3, 2, 1, 333)));
      select.Columns.Add(2);
    }

    [Test]
    public virtual void DateTimeExtractSecondTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.Second, new DateTime(2006, 5, 4, 3, 2, 1, 333)));
      select.Columns.Add(1);
    }

    [Test]
    public virtual void DateTimeExtractMillisecondTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.Millisecond, new DateTime(2006, 5, 4, 3, 2, 1, 333)));
      select.Columns.Add(333);
    }

    [Test]
    public virtual void DateTimeExtractDayOfWeekTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.DayOfWeek, new DateTime(2009, 3, 2)));
      select.Columns.Add((int)DayOfWeek.Monday);
    }

    [Test]
    public virtual void DateTimeExtractDayOfYearTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.DayOfYear, new DateTime(2005, 2, 2)));
      select.Columns.Add(33);
    }

    [Test]
    public virtual void IntervalConstructTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.IntervalConstruct(500000000));
      select.Columns.Add(new TimeSpan(0, 0, 0, 0, 500));
    }

    [Test]
    public virtual void IntervalExtractDayTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlIntervalPart.Day, new TimeSpan(6, 5, 4, 3, 2)));
      select.Columns.Add(6);
    }

    [Test]
    public virtual void IntervalExtractHourTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlIntervalPart.Hour, new TimeSpan(6, 5, 4, 3, 2)));
      select.Columns.Add(5);
    }

    [Test]
    public virtual void IntervalExtractMinuteTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlIntervalPart.Minute, new TimeSpan(6, 5, 4, 3, 2)));
      select.Columns.Add(4);
    }

    [Test]
    public virtual void IntervalExtractSecondTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlIntervalPart.Second, new TimeSpan(6, 5, 4, 3, 2)));
      select.Columns.Add(3);
    }

    [Test]
    public virtual void IntervalExtractMillisecondTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.Extract(SqlIntervalPart.Millisecond, new TimeSpan(6, 5, 4, 3, 2)));
      select.Columns.Add(2);
    }

    [Test]
    public virtual void IntervalToMillisecondsTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.IntervalToMilliseconds(new TimeSpan(0, 0, 8, 5, 5)));
      select.Columns.Add((int)new TimeSpan(0, 0, 8, 5, 5).TotalMilliseconds);
    }

    [Test]
    public virtual void IntervalAbsTest()
    {
      var select = SqlDml.Select();
      select.Columns.Add(
        SqlDml.IntervalAbs(new TimeSpan(10, 0, 0, 0).Negate()));
      select.Columns.Add(new TimeSpan(10, 0, 0, 0));
    }

  }
}
