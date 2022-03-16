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
    class Item : Entity
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
    public class OtherService
    {
      public static volatile int InstanceCount;

      public int N;

      public OtherService()
      {
        Interlocked.Increment(ref InstanceCount);
      }

      ~OtherService()
      {
        Interlocked.Decrement(ref InstanceCount);
      }
    }


    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
      return config;
    }

    [Test]
    public void DelayedQueryCapture()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var item = new Item() { Tag = 10 };
        DelayedQuery(session);
        t.Complete();
      }
      GC.Collect();
      Thread.Sleep(1000);
      GC.Collect();
      Assert.AreEqual(0, OtherService.InstanceCount);
    }

    private void DelayedQuery(Session session)
    {
      var ids = new[] { 1, 2 };
      var otherService = new OtherService();

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
  }
}
