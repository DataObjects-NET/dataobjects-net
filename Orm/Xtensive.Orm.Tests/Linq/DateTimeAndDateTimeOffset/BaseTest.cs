// Copyright (C) 2016-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      using (var transaction = session.OpenTransaction()) {
        action();
      }
    }

    protected void ExecuteInsideSession(Action<Session> action)
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        action(session);
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

    protected void RunTest<T>(Session session, Expression<Func<T, bool>> filter, int rightCount = 1)
      where T : Entity
    {
      var count = session.Query.All<T>().Count(filter);
      Assert.AreEqual(rightCount, count);
    }

    protected void RunWrongTest<T>(Expression<Func<T, bool>> filter)
      where T : Entity
    {
      RunTest(filter, 0);
    }

    protected void RunWrongTest<T>(Session session, Expression<Func<T, bool>> filter)
      where T : Entity
    {
      RunTest(session, filter, 0);
    }
  }
}
