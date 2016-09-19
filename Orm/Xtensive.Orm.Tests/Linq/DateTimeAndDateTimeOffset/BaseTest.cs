// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.09.15

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public abstract class BaseTest : AutoBuildTest
  {
    protected abstract void RegisterTypes(DomainConfiguration configuration);

    protected virtual void PopulateNonPersistentData()
    {
    }

    protected virtual void PopulateEntities(Session session)
    {
    }

    protected virtual void InitializeCustomSettings(DomainConfiguration configuration)
    {
    }

    protected void ExecuteInsideSession(Action action)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        action();
      }
    }

    protected override void PopulateData()
    {
      PopulateNonPersistentData();

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        PopulateEntities(session);

        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      RegisterTypes(configuration);
      InitializeCustomSettings(configuration);
      return configuration;
    }

    protected void RunTest<T>(Expression<Func<T, bool>> filter, int rightCount = 1)
      where T : Entity
    {
      var count = Query.All<T>().Count(filter);
      Assert.AreEqual(rightCount, count);
    }

    protected void RunWrongTest<T>(Expression<Func<T, bool>> filter)
      where T : Entity
    {
      RunTest(filter, 0);
    }
  }
}
