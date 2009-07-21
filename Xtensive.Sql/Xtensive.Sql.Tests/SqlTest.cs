// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.21

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests
{
  public abstract class SqlTest
  {
    protected abstract string Url { get; }

    protected SqlConnection Connection { get; private set; }
    protected SqlDriver Driver { get; private set; }

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      var parsedUrl = new UrlInfo(Url);
      // temporary workaround until all oracle swarm is installed on build agents.
      if (parsedUrl.Protocol=="oracle" && TestInfo.IsBuildServer)
        Assert.Ignore("no oracle on build server");
      try {
        Driver = SqlDriver.Create(parsedUrl);
        Connection = Driver.CreateConnection(parsedUrl);
        Connection.Open();
      }
      catch (Exception e) {
        Console.WriteLine(Url);
        Console.WriteLine(e);
      }
    }

    [TestFixtureTearDown]
    public virtual void TestFixtureTearDown()
    {
      if (Connection!=null && Connection.State==ConnectionState.Open)
        Connection.Close();
    }

    protected Catalog ExtractModel()
    {
      Catalog model;
      using (var transaction = Connection.BeginTransaction()) {
        model = Driver.ExtractModel(Connection, transaction);
        transaction.Commit();
      }
      return model;
    }
  }
}