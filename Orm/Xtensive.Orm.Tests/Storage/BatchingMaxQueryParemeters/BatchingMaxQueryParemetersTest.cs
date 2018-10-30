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
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.BatchingMaxQueryParemeters.Model;

namespace Xtensive.Orm.Tests.Storage.BatchingMaxQueryParemeters
{
  public class BatchingMaxQueryParemetersTest : AutoBuildTest
  {
    [Test]
    public void PersistTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        for (var i = 0; i < 100; i++) {
          new SimpleEntity();
        }

        t.Complete();
      }
    }

    [Test]
    public void LoadTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        Enumerable.Range(0, 100).Select(
          i => {
            var range = Enumerable.Range(0, 317).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<SimpleEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();
        session.Query.All<SimpleEntity>().ToArray();
      }
    }

    [Test]
    public void LastRequestTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer, "2100 limit");
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = 10}))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        Enumerable.Range(0, 12).Select(
          i => {
            var range = Enumerable.Range(0, 699).ToArray();
            return session.Query.ExecuteDelayed(query => query.All<SimpleEntity>().Where(x => x.Id.In(IncludeAlgorithm.ComplexCondition, range)));
          }).ToArray();

        int value1 = 1, value2 = 1, value3 = 1;
        session.Query.All<SimpleEntity>().Where(x => x.Id > value1 && x.Id > value2 && x.Id > value3).ToArray();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(SimpleEntity).Assembly, typeof(SimpleEntity).Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }
  }
}