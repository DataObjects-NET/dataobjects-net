// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0546_IncorrectCachingOfQueriesModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0546_IncorrectCachingOfQueriesModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }

  [HierarchyRoot]
  public class Location : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public bool Active { get; set; }

    [Field]
    public string Zone { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{ 
  internal class Task
  {
    private Session session;
    private int startValue;

    public int GetMinimalLocationId()
    {
      Func<Session, int> func = (s) => {
        var locations = session.Query.Execute(endpoint => from location in s.Query.All<Location>()
          where location.Active && location.Id.In((
            from loc in s.Query.All<Location>()
            where loc.Id > startValue && loc.Zone==null
            orderby loc.Id
            select loc.Id))
          select location);
        var minId = locations.Min(entity => entity.Id);
        return minId;
      };
      int result = 0;
      Action<IsolationLevel?> action = (isolationLevel) => {
        using (var tx = session.OpenTransaction()) {
          result = func.Invoke(session);
          tx.Complete();
        }
      };
      action.Invoke(null);
      return result;
    }

    public Task(Session session, int startValue)
    {
      this.session = session;
      this.startValue = startValue;
    }
  }

  [TestFixture]
  public class IssueJira0546_IncorrectCachingOfQueries : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) 
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 100; i++) {
          var location = new Location {Active = true};
        }
        transaction.Complete();
      }
    }

    [Test]
    public void LoopIndexTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var upperLimit = 80;
        List<IEnumerable<Location>> queries = new List<IEnumerable<Location>>();
        var cachedQueriesCountBefore = Domain.QueryCache.Count;
        for (int i = 0; i < upperLimit; i++) {
          var locations = session.Query.ExecuteDelayed(endpoint => from location in Query.All<Location>()
            where location.Active && location.Id.In((
              from loc in session.Query.All<Location>()
              where loc.Id > i && loc.Zone==null
              orderby loc.Id
              select loc.Id))
            select location);
          queries.Add(locations);
        }
        var cachedQueriesCountAfter = Domain.QueryCache.Count;
        Assert.Greater(cachedQueriesCountAfter, cachedQueriesCountBefore);
        Assert.AreEqual(1, cachedQueriesCountAfter - cachedQueriesCountBefore);
        var result = session.Query.All<TestEntity>().ToList();
        var id = upperLimit + 1;
        foreach (var query in queries)
          Assert.AreEqual(id, query.Min(entity => entity.Id));
      }
    }

    [Test]
    public void LocalValueTypeVariableTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var cachedQueriesCountBefore = Domain.QueryCache.Count;
        var currentValue = 1;
        var iterationsCount = 0;
        while (currentValue<80) {
          var oldValue = currentValue;
          currentValue = GetMinimalId(session, currentValue);
          Assert.AreEqual(oldValue+1, currentValue);
          iterationsCount++;
        }
        var cachedQueriesCountAfter = Domain.QueryCache.Count;
        Assert.Greater(cachedQueriesCountAfter, cachedQueriesCountBefore);
        Assert.AreEqual(iterationsCount, cachedQueriesCountAfter - cachedQueriesCountBefore);
      }
    }

    [Test]
    public void GlobalFieldOfClassTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.Activate()) {
          TaskSequencesGenerator(session);
        }
      }
    }

    private void TaskSequencesGenerator(Session session)
    {
      var cachedQueriesCountBefore = Domain.QueryCache.Count;
      var currentId = 1;
      var iterationsCount = 0;
      while (currentId<80) {
        var task = new Task(session, currentId);
        var previousId = currentId;
        currentId = task.GetMinimalLocationId();
        Assert.AreEqual(previousId+1, currentId);
        iterationsCount++;
      }
      var cachedQueriesCountAfter = Domain.QueryCache.Count;
      Assert.Greater(cachedQueriesCountAfter, cachedQueriesCountBefore);
      Assert.AreEqual(iterationsCount, cachedQueriesCountAfter - cachedQueriesCountBefore);
    }

    private int GetMinimalId(Session session, int startId)
    {
      var locations = session.Query.ExecuteDelayed(endpoint => from location in Query.All<Location>()
        where location.Active && location.Id.In((
          from loc in session.Query.All<Location>()
          where loc.Id > startId && loc.Zone==null
          orderby loc.Id
          select loc.Id))
        select location);
      session.Query.All<Location>();
      return locations.Min(field => field.Id);
    }
  }
}
