// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.29

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.Oracle
{
  [TestFixture, Explicit]
  public class ExtractorTest : SqlTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Oracle);
    }

    [Test]
    public void BaseTest()
    {
      var schema = ExtractDefaultSchema();
    }

    [Test]
    public void TimeStampBasedTypes()
    {
      var dropTable = "DROP TABLE TableWithTimeStamps PURGE";
      try {
        var dropTableCommand = Connection.CreateCommand(dropTable);
        dropTableCommand.ExecuteNonQuery();
      }
      catch {

      }

      var createTable = "CREATE TABLE TableWithTimeStamps" +
                        "( Id             NUMBER (6)," +
                        ", DateTime       TIMESTAMP" +
                        ", DateTimeOffset TIMESTAMP WITH TIME ZONE)";
      var command = Connection.CreateCommand(createTable);
      command.ExecuteNonQuery();
      var schema = ExtractDefaultSchema();
      var table = schema.Tables["TableWithTimeStamps"];
      Assert.That(table, Is.Not.Null);
      Assert.That(table.Columns.Count, Is.EqualTo(3));

      var dateTimeColumn = table.TableColumns["DateTime"];
      Assert.That(dateTimeColumn, Is.Not.Null);
      Assert.That(dateTimeColumn.DataType.Type, Is.EqualTo(SqlType.DateTime));

      var dateTimeOffsetColumn = table.TableColumns["DateTimeOffset"];
      Assert.That(dateTimeOffsetColumn, Is.Not.Null);
      Assert.That(dateTimeColumn.DataType.Type, Is.EqualTo(SqlType.DateTimeOffset));
    }
  }
}