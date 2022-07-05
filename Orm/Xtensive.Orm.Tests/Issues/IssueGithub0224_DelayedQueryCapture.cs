// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub0224_DelayedQueryCapture_Model;
namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueGithub0224_DelayedQueryCapture_Model
  {
    [HierarchyRoot]
    public class Item : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int Tag { get; set; }
    }
  }

  [Serializable]
  public class IssueGithub0224_DelayedQueryCapture : AutoBuildTest
  {

    #region Services

    public class OtherService1
    {
      public static volatile int InstanceCount;

      public int N;

      public OtherService1()
      {
        _ = Interlocked.Increment(ref InstanceCount);
      }

      ~OtherService1()
      {
        _ = Interlocked.Decrement(ref InstanceCount);
      }
    }

    public class OtherService2
    {
      public static volatile int InstanceCount;

      public int N;

      public OtherService2()
      {
        _ = Interlocked.Increment(ref InstanceCount);
      }

      ~OtherService2()
      {
        _ = Interlocked.Decrement(ref InstanceCount);
      }
    }

    public class OtherService3
    {
      public static volatile int InstanceCount;

      public int N;

      public OtherService3()
      {
        _ = Interlocked.Increment(ref InstanceCount);
      }

      ~OtherService3()
      {
        _ = Interlocked.Decrement(ref InstanceCount);
      }
    }

    #endregion

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Item));
      return config;
    }

    [Test]
    public void DelayedQueryWithIncludeTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var item = new Item() { Tag = 10 };
        DelayedQueryWithInclude(session);
        t.Complete();
      }
      TestHelper.CollectGarbage(true);
      Assert.AreEqual(0, OtherService1.InstanceCount);
    }

    [Test]
    public void DelayedQueryWithContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var item = new Item() { Tag = 10 };
        DelayedQueryWithContains(session);
        t.Complete();
      }

      TestHelper.CollectGarbage(true);
      Assert.AreEqual(0, OtherService2.InstanceCount);
    }

    [Test]
    public void DelayedQueryWithEqualityTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var item = new Item() { Tag = 10 };
        DelayedQueryWithEquality(session);
        t.Complete();
      }

      TestHelper.CollectGarbage(true);
      Assert.AreEqual(0, OtherService3.InstanceCount);
    }


    public class SomeConfig
    {
      public int Id { get; set; }

      public bool HideItems { get; set; }

      public static volatile int InstanceCount;

      public SomeConfig()
      {
        _ = Interlocked.Increment(ref InstanceCount);
      }

      ~SomeConfig()
      {
        _ = Interlocked.Decrement(ref InstanceCount);
      }
    }

    public class ItemDto
    {
      public int ItemId { get; set; }

      public ItemDto(SomeConfig config)
      {
      }
    }

    public class Service4
    {
      public Session Session;

      private readonly SomeConfig configuration = new SomeConfig() { Id = 1 };

      public ItemDto DelayedQueryWithDTOCtorParamweter(int id)
      {
        ItemDto model = null;
        {  // This curly brace does matter. Without it the Closure is different
          var item2 = Session.Query.Single<Item>(id);
          var query = Session.Query.CreateDelayedQuery(
            q => {
              return (from inv in q.All<Item>().Where(o => o == item2)
                      select new ItemDto(configuration) {
                        ItemId = inv.Id
                      }).Single();
            }
          );
          model = query.Value;
        }
        var invoice2 = Session.Query.All<Item>().Single(i => i.Id == model.ItemId);
        return model;
      }
    }

    [Test]
    public void DelayedQueryWithDtoCtorParamTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var item = new Item { Tag = 10 };
        DelayedQueryWithDTOCtorParamweter(session);
        t.Complete();
      }
      TestHelper.CollectGarbage(true);
      Assert.AreEqual(0, SomeConfig.InstanceCount);
    }

    private void DelayedQueryWithDTOCtorParamweter(Session session)
    {
      _ = new Service4() { Session = session }.DelayedQueryWithDTOCtorParamweter(1);
    }

    private void DelayedQueryWithEquality(Session session)
    {
      var id = 1;
      var otherService = new OtherService3();

      var items = session.Query.CreateDelayedQuery("ABC", q =>
                from t in q.All<Item>()
                where t.Id == id
                select t).ToArray();

      var bb1 = items
          .Select(a => new {
            a.Id,
            A = new {
              B = otherService.N == a.Id
            },
          });
    }

    private void DelayedQueryWithInclude(Session session)
    {
      var ids = new[] { 1, 2 };
      var otherService = new OtherService1();

      var items = session.Query.CreateDelayedQuery(q =>
                from t in q.All<Item>()
                where t.Id.In(ids)
                select t).ToArray();

      var bb1 = items
          .Select(a => new {
            a.Id,
            A = new {
              B = otherService.N == a.Id
            },
          });
    }

    private void DelayedQueryWithContains(Session session)
    {
      var ids = new[] { 1, 2 };
      var otherService = new OtherService2();

      var items = session.Query.CreateDelayedQuery(q =>
                from t in q.All<Item>()
                where ids.Contains(t.Id)
                select t).ToArray();

      var bb1 = items
          .Select(a => new {
            a.Id,
            A = new {
              B = otherService.N == a.Id
            },
          });
    }
  }
}
