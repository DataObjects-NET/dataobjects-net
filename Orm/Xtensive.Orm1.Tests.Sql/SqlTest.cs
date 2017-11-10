// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.21

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class SqlTest
  {
    protected string Url
    {
      get { return TestConnectionInfoProvider.GetConnectionUrl(); }
    }

    protected SqlConnection Connection { get; private set; }
    protected SqlDriver Driver { get; private set; }

#if NETCOREAPP
    [OneTimeSetUp]
#else
    [TestFixtureSetUp]
#endif
    public void RealTestFixtureSetUp()
    {
      CheckRequirements();
      try {
        TestFixtureSetUp();
      }
      catch (Exception e) {
        Console.WriteLine(Url);
        Console.WriteLine(e);
        throw;
      }
    }

#if NETCOREAPP
    [OneTimeTearDown]
#else
    [TestFixtureTearDown]
#endif
    public void RealTestFixtureTearDown()
    {
      TestFixtureTearDown();
    }

    [SetUp]
    public virtual void SetUp()
    {
    }

    [TearDown]
    public virtual void TearDown()
    {
    }

    protected virtual void TestFixtureSetUp()
    {
      var parsedUrl = UrlInfo.Parse(Url);
      Driver = TestSqlDriver.Create(parsedUrl);
      Connection = Driver.CreateConnection();
      Connection.Open();
    }

    protected virtual void TestFixtureTearDown()
    {
      if (Connection!=null && Connection.State==ConnectionState.Open)
        Connection.Close();
    }

    protected virtual void CheckRequirements()
    {
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
      using (var command = Connection.CreateCommand(commandText))
        return command.ExecuteNonQuery();
    }

    protected int ExecuteNonQuery(ISqlCompileUnit statement)
    {
      using (var command = Connection.CreateCommand(statement))
        return command.ExecuteNonQuery();
    }

    protected object ExecuteScalar(string commandText)
    {
      using (var command = Connection.CreateCommand(commandText))
        return command.ExecuteScalar();
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