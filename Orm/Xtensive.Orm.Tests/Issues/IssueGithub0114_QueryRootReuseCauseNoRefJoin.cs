// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueGithub0114_QueryRootReuseCauseNoRefJoinModel;

namespace Xtensive.Orm.Tests.Issues.IssueGithub0114_QueryRootReuseCauseNoRefJoinModel
{
  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Promotion : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string CampainName { get; set; }

    [Field]
    [Association(PairTo = nameof(Notification.Promotion))]
    public EntitySet<Notification> Notifications { get; private set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Notification : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Nullable = false)]
    public Promotion Promotion { get; private set; }

    [Field(Nullable = false)]
    public Recipient Recipient { get; private set; }

    [Field]
    public User TriggeredBy { get; private set; }

  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Recipient : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public User User { get; set; }

    [Field]
    [Association(PairTo = nameof(Notification.Recipient), OnOwnerRemove = OnRemoveAction.Cascade)]
    public EntitySet<Notification> Notifications { get; private set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class User : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50, Nullable = false)]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public sealed class IssueGithub0114_QueryRootReuseCauseNoRefJoin : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Promotion));
      configuration.Types.Register(typeof(Notification));
      configuration.Types.Register(typeof(Recipient));
      configuration.Types.Register(typeof(User));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void BaseQueryReuseWithExcept()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithIntersect()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Intersect(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithUnion()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Union(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithConcat()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithAny()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Any(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.Any(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithAll()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.All(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.All(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithAverage()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            average1 = anon.notifications.Average(c => c.Recipient.User.Id),
            average2 = anon.notifications.Average(c => c.Recipient.User.Id)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithCount()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Count(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.Count(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereDistinct()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Where(c => c.Recipient.User.Id > 1000).Distinct(),
            less1000 = anon.notifications.Where(c => c.Recipient.User.Id < 1000).Distinct()
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithDistinctWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Distinct().Where(c => c.Recipient.User.Id > 1000).Select(c => c.Recipient.User.Id),
            less1000 = anon.notifications.Distinct().Where(c => c.Recipient.User.Id < 1000).Select(c => c.Recipient.User.Id)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithFirst()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.First(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.First(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithFirstOrDefault()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.FirstOrDefault(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.FirstOrDefault(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithGroupBy()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            group1 = anon.notifications.GroupBy(c => c.Recipient.User.Id),
            group2 = anon.notifications.GroupBy(c => c.Recipient.User.Id)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithGroupJoin()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            group1 = anon.notifications.GroupJoin(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user=>user.Id,
              (not,us) => us),
            group2 = anon.notifications.GroupJoin(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => us)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithJoin()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            group1 = anon.notifications.Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => us),
            group2 = anon.notifications.Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => us)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithIn()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            in1 = anon.notifications.Where(c => c.Recipient.User.Id.In(new long[] { 1, 2, 3 })),
            in2 = anon.notifications.Where(c => c.Recipient.User.Id.In(new long[] { 4, 5, 6 }))
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithMinMax()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Max(c => c.Recipient.User.Id),
            less1000 = anon.notifications.Min(c => c.Recipient.User.Id)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithOfTypeWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.OfType<Notification>().Where(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.OfType<Notification>().Where(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereOfType()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Where(c => c.Recipient.User.Id > 1000).OfType<Notification>(),
            less1000 = anon.notifications.Where(c => c.Recipient.User.Id < 1000).OfType<Notification>()
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithOrderBy()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            order1 = anon.notifications.OrderBy(c => c.Recipient.User.Id),
            order2 = anon.notifications.OrderBy(c => c.Recipient.User.Id)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithOrderByDescending()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            order1 = anon.notifications.OrderByDescending(c => c.Recipient.User.Id),
            order2 = anon.notifications.OrderByDescending(c => c.Recipient.User.Id)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithSum()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Sum(c => c.Recipient.User.Id),
            less1000 = anon.notifications.Sum(c => c.Recipient.User.Id)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithTakeWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Take(2).Where(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.Take(3).Where(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereTake()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Where(c => c.Recipient.User.Id > 1000).Take(2),
            less1000 = anon.notifications.Where(c => c.Recipient.User.Id < 1000).Take(3)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithSkipWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Skip(2).Where(c => c.Recipient.User.Id > 1000),
            less1000 = anon.notifications.Skip(3).Where(c => c.Recipient.User.Id < 1000)
          }).ToArray();
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereSkip()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more1000 = anon.notifications.Where(c => c.Recipient.User.Id > 1000).Skip(2),
            less1000 = anon.notifications.Where(c => c.Recipient.User.Id < 1000).Skip(3)
          }).ToArray();
      }
    }

    [Test]
    public void CastQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Cast<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void DistinctQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Distinct()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void GroupByQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().GroupBy(n => n.Id)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.SelectMany(g => g.Select(c =>c.Recipient.User.Id))
              .Union(anon1.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id)))
          }).ToArray();
      }
    }

    [Test]
    public void WhereGroupByQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n=> n.Id > 0).GroupBy(n => n.Id)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(anon1.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id)))
          }).ToArray();
      }
    }

    [Test]
    public void GroupByWhereQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().GroupBy(n => n.Id).Where(g => g.Key > 0)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(anon1.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id)))
          }).ToArray();
      }
    }

    [Test]
    public void WhereGroupByWhereQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n=> n.Id > 0).GroupBy(n => n.Id).Where(g => g.Key > 0)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(anon1.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id)))
          }).ToArray();
      }
    }

    [Test]
    public void GroupJoinQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().GroupJoin(
              session.Query.All<User>(),
              n=>n.TriggeredBy.Id,
              u => u.Id,
              (n, u)=> new { n, u })
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.n.Recipient.User.Id)
              .Union(anon1.notifications.Select(c => c.n.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void GroupJoinQueryReuse1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().GroupJoin(
              session.Query.All<User>(),
              n => n.TriggeredBy.Id,
              u => u.Id,
              (n, u) => new { n, u })
          })
          .Select(anon1 => new {
            contacted = session.Query.All<Notification>().Select(n => new { n }).Select(c => c.n.Recipient.User.Id)
              .Union(anon1.notifications.Select(c => c.n.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void JoinQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .Join(session.Query.All<User>(), n => n.TriggeredBy.Id, u=>u.Id, (n, u) => new { n , u })
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.n.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.n.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void LeftJoinQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .LeftJoin(session.Query.All<User>(), n => n.TriggeredBy.Id, u => u.Id, (n, u) => new { n, u })
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.n.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.n.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void OfTypeQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().OfType<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void OrderByQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().OrderBy(n => n.Id)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void OrderByDescendingQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().OrderByDescending(n => n.Id)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void SelectQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Select(n => new { n, n.Id })
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.n.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.n.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void SkipQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Skip(3)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void TakeQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Take(8)
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void IntersectOfWheresQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n => n.Id > 500)
              .Intersect(session.Query.All<Notification>().Where(n => n.Id < 1000))
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void ExceptOfWheresQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n => n.Id > 500)
              .Except(session.Query.All<Notification>().Where(n => n.Id < 1000))
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void ConcatOfWheresQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n => n.Id > 500)
              .Concat(session.Query.All<Notification>().Where(n => n.Id < 1000))
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void UnionOfWheresQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n => n.Id > 500)
              .Union(session.Query.All<Notification>().Where(n => n.Id < 1000))
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void OnePropOneTrueQueryRoot()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(session.Query.All<Notification>().Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void NoPropertyQueryRootReuse1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Except(session.Query.All<Notification>().Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void PropertyQueryRootOneUse1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(session.Query.All<Notification>().Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void PropertyQueryRootOneUse2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void SeparatePropertyForQueryRoot()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications1 = session.Query.All<Notification>(),
            notifications2 = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            contacted = anon1.notifications1.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications2.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }
  }
}
