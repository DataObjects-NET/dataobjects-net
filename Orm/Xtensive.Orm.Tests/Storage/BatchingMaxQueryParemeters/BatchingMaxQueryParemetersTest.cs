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
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.BatchingMaxQueryParemeters.Model;

namespace Xtensive.Orm.Tests.Storage.BatchingMaxQueryParemeters
{
  public class BatchingMaxQueryParemetersTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = Domain.Build(this.BuildConfiguration()))
      using (var session = domain.OpenSession(SessionType.Default))
      using (session.Activate())
      using (var t = session.OpenTransaction()) {
        for (var i = 0; i < 100; i++) {
          new SimpleEntity(Guid.NewGuid());
        }

        t.Complete();
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
