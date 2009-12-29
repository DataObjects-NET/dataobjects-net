// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.21

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests
{
  public abstract class SqlTest
  {
    protected abstract string Url { get; }

    protected SqlConnection Connection { get; private set; }
    protected SqlDriver Driver { get; private set; }

    [TestFixtureSetUp]
    public void RealTestFixtureSetUp()
    {
      try {
        TestFixtureSetUp();
      }
      catch (Exception e) {
        Console.WriteLine(Url);
        Console.WriteLine(e);
        throw;
      }
    }

    [TestFixtureTearDown]
    public void RealTestFixtureTearDown()
    {
      TestFixtureTearDown();
    }

    protected virtual void TestFixtureSetUp()
    {
      var parsedUrl = new UrlInfo(Url);
      Driver = SqlDriver.Create(parsedUrl);
      Connection = Driver.CreateConnection();
      Connection.Open();
    }

    protected virtual void TestFixtureTearDown()
    {
      if (Connection!=null && Connection.State==ConnectionState.Open)
        Connection.Close();
    }

    protected Catalog ExtractCatalog()
    {
      Catalog model;
      try {
        Connection.BeginTransaction();
        model = Driver.ExtractCatalog(Connection);
      }
      finally {
        Connection.Rollback(); 
      }
      return model;
    }

    protected Schema ExtractSchema(string schemaName)
    {
      Schema schema;
      try {
        Connection.BeginTransaction();
        schema = Driver.ExtractSchema(Connection, schemaName);
      }
      finally {
        Connection.Rollback();
      }
      return schema;
    }

    protected Schema ExtractDefaultSchema()
    {
      Schema schema;
      try {
        Connection.BeginTransaction();
        schema = Driver.ExtractDefaultSchema(Connection);
      }
      finally {
        Connection.Rollback();
      }
      return schema;
    }

    protected int ExecuteNonQuery(string commandText)
    {
      using (var command = Connection.CreateCommand()) {
        command.CommandText = commandText;
        return command.ExecuteNonQuery();
      }
    }

    protected int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      using (var command = Connection.CreateCommand(statement)) {
        Console.WriteLine(command.CommandText);
        return command.ExecuteNonQuery();
      }
    }

    protected object ExecuteScalar(string commandText)
    {
      using (var command = Connection.CreateCommand()) {
        command.CommandText = commandText;
        return command.ExecuteScalar();
      }
    }

    protected object ExecuteScalar(ISqlCompileUnit statement)
    {
      using (var command = Connection.CreateCommand(statement))
        return command.ExecuteScalar();
    }

    protected void EnsureTableNotExists(Schema schema, string tableName)
    {
      var table = schema.Tables[tableName];
      if (table==null)
        return;
      ExecuteNonQuery(SqlDdl.Drop(table));
      schema.Tables.Remove(table);
    }
  }
}