// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.02

using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Xtensive.Sql;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  [TestFixture]
  public class DateTimeIntervalTest : Sql.DateTimeIntervalTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    public override void DateTimeSubtractIntervalTest()
    {
      Assert.Ignore("MSSQL DateTime precision issue");
    }

    [Test]
    public void DateTimeLiteralPrecisionTest()
    {
      const long ticksValue = 1234567;
      var literalValue = new DateTime(ticksValue);
      using (var connection = Driver.CreateConnection()) {
        var sql = SqlDml.Select(SqlDml.Literal(literalValue));
        var cmd = connection.CreateCommand(sql);

        var match = Regex.Match(cmd.CommandText, @"'[\w\W]*'");
        var sqlLiteralValue = DateTime.Parse(match.Value.Replace("\'", ""));
        Assert.That(sqlLiteralValue, Is.EqualTo(literalValue));
      }
    }
  }
}