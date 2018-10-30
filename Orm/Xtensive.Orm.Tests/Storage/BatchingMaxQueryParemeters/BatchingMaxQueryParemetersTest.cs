// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.10.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.BatchingMaxQueryParemeters.Model;

namespace Xtensive.Orm.Tests.Storage.BatchingMaxQueryParemeters
{
  public class BatchingMaxQueryParemetersTest : AutoBuildTest
  {
    [Test]
    public void PersistTest()
    {
      const int reqCount = 100;
      var commands = new List<DbCommandEventArgs>();
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        session.Events.DbCommandExecuted += (s, e) => commands.Add(e);
        for (var i = 0; i < reqCount; i++) {
          new SimpleEntity();
        }

        t.Complete();
      }

      Assert.That(commands.Count, Is.EqualTo(51));
      Assert.That(commands.Sum(x => x.Command.Parameters.Count), Is.EqualTo(reqCount * 900 + reqCount));
    }

    [Test]
    public void LoadTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var commands = new List<DbCommandEventArgs>();
        session.Events.DbCommandExecuted += (s, e) => commands.Add(e);
        const int reqCount = 100, paramCount = 317;
        Enumerable.Range(0, reqCount).Select(
          i => {
            var range = Enumerable.Range(0, paramCount).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<SimpleEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();
        session.Query.All<SimpleEntity>().ToArray();
        Assert.That(commands.Count, Is.EqualTo(17));
        Assert.That(commands.Sum(x => x.Command.Parameters.Count), Is.EqualTo(reqCount * paramCount));
      }
    }

    [Test]
    public void ManyParametersTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "2100 limit");

      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var range = Enumerable.Range(0, 2100).ToArray();
        session.Query.ExecuteDelayed(query => query.All<SimpleEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
        Assert.Throws<StorageException>(() => session.Query.All<SimpleEntity>().ToArray());
      }
    }

    [Test]
    public void LastRequestTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "2100 limit");
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var commands = new List<DbCommandEventArgs>();
        session.Events.DbCommandExecuted += (s, e) => commands.Add(e);
        const int reqCount = 12, paramCount = 699, lastReqparamCount = 3;
        Enumerable.Range(0, reqCount).Select(
          i => {
            var range = Enumerable.Range(0, paramCount).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<SimpleEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        int value1 = 1, value2 = 1, value3 = 1;
        session.Query.All<SimpleEntity>().Where(x => x.Id > value1 && x.Id > value2 && x.Id > value3).ToArray();
        Assert.That(commands.Count, Is.EqualTo(5));
        Assert.That(commands.Last().Command.Parameters.Count, Is.EqualTo(lastReqparamCount));
        Assert.That(commands.Sum(x => x.Command.Parameters.Count), Is.EqualTo(reqCount * paramCount + lastReqparamCount));
      }
    }

    [Test]
    public void AllowPartialExecutionTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        var commands = new List<DbCommandEventArgs>();
        session.Events.DbCommandExecuted += (s, e) => commands.Add(e);
        Enumerable.Range(0, 100).Select(
          i => {
            var range = Enumerable.Range(0, 500).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<SimpleEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        session.CreateEnumerationContext();
        Assert.That(commands.Count, Is.EqualTo(23));
        session.Handler.ExecuteQueryTasks(new QueryTask[0], false);
        Assert.That(commands.Count, Is.EqualTo(25));
      }
    }


    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (SimpleEntity).Assembly, typeof (SimpleEntity).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }
  }
}