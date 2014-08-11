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
    public Zone Zone { get; set; }
  }

  [HierarchyRoot]
  public class Zone : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Zone")]
    public EntitySet<Location> Locations { get; set; } 
  }
}

namespace Xtensive.Orm.Tests.Issues
{ 
  internal class BaseTask
  {
    public int StartValue { get; set; }

    public BaseTask(int startValue)
    {
      StartValue = startValue;
    }
  }
  internal class Task: BaseTask
  {
    public Session Session { get; private set; }
    
    public Zone NullZone { get; private set; }

    public int GetMinimalLocationId()
    {
      Func<Session, int> func = (s) => {
        var locations = Session.Query.ExecuteDelayed(endpoint => from location in s.Query.All<Location>()
          where location.Active && location.Id.In((
            from loc in s.Query.All<Location>()
              where loc.Id > StartValue && loc.Zone==NullZone
              orderby loc.Id
              select loc.Id))
            select location);
        var minId = locations.Min(entity => entity.Id);
        return minId;
      };
      int result = 0;
      Action action = () => {
        using (var tx = Session.OpenTransaction()) {
          result = func.Invoke(Session);
          tx.Complete();
        }
      };
      action.Invoke();
      return result;
    }

    public Task(Session session, int startValue)
      : base(startValue)
    {
      this.Session = session;
      NullZone = null;
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

        new Location {Active = true, Zone = new Zone {Name = "Zone"}};
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
        Assert.AreEqual(1, cachedQueriesCountAfter - cachedQueriesCountBefore);
      }
    }

    public void LocalReferenceTypeVariableTest()
    {
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var cachedQueriesCountBefore = Domain.QueryCache.Count;
        var zone = session.Query.All<Zone>().First();
        for (int i = 0; i < 80; i++)
          GetMinimalIdWithZone(session, zone);
        var cachedQueriesCountAfter = Domain.QueryCache.Count;
        Assert.Greater(cachedQueriesCountAfter, cachedQueriesCountBefore);
        Assert.AreEqual(1, cachedQueriesCountAfter - cachedQueriesCountBefore);
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
      Assert.AreEqual(1, cachedQueriesCountAfter - cachedQueriesCountBefore);
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

    private int GetMinimalIdWithZone(Session session, Zone zone)
    {
      var locations = session.Query.ExecuteDelayed(endpoint => from location in Query.All<Location>()
        where location.Active && location.Id.In((
          from loc in session.Query.All<Location>()
          where loc.Zone==zone
          orderby loc.Id
          select loc.Id))
        select location);
      session.Query.All<Location>();
      return locations.Min(field => field.Id);
    }
  }
}
