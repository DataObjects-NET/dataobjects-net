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
    public bool IsLimited { get; private set; }

    [Field]
    [Association(PairTo = nameof(Notification.Promotion))]
    public EntitySet<Notification> Notifications { get; private set; }

    public Promotion(Session session, string campainName, bool limited)
      : base(session)
    {
      CampainName = campainName;
      IsLimited = limited;
    }
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

    public Notification(Session session, Recipient recipient, Promotion promotion)
      : base(session)
    {
      Promotion = promotion;
      Recipient = recipient;
    }

    public Notification(Session session, Recipient recipient, Promotion promotion, Staff triggeredBy)
      : this(session, recipient, promotion)
    {
      TriggeredBy = triggeredBy;
    }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Recipient : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public User User { get; private set; }

    [Field]
    [Association(PairTo = nameof(Notification.Recipient), OnOwnerRemove = OnRemoveAction.Cascade)]
    public EntitySet<Notification> Notifications { get; private set; }

    public Recipient(Session session, User user)
      : base(session)
    {
      User = user;
    }
  }

  public class Customer : User
  {
    public Customer(Session session, string name)
      : base(session, name)
    {
    }
  }

  public class Staff : User
  {
    public Staff(Session session, string name)
      : base(session, name)
    {
    }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public abstract class User : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 50, Nullable = false)]
    public string Name { get; private set; }

    public User(Session session, string name)
      : base(session)
    {
      Name = name;
    }
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
      configuration.Types.Register(typeof(Customer));
      configuration.Types.Register(typeof(Staff));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      using var session = Domain.OpenSession();
      using var tx = session.OpenTransaction();

      var recipients = new List<Recipient>(50);
      for (var i = 0; i < 50; i++)
        recipients.Add(new Recipient(session, new Customer(session, Guid.NewGuid().ToString())));

      var staffs = new List<Staff>(10);
      for (var i = 0; i < 10; i++)
        staffs.Add(new Staff(session, Guid.NewGuid().ToString()));

      var promotions = new List<Promotion>(10);
      for (var i = 0; i < 10; i++)
        promotions.Add(new Promotion(session, Guid.NewGuid().ToString(), (i % 4) == 0));

      var noftifierChooser = new Random();

      foreach (var promotion in promotions)
        foreach (var user in recipients)
          _ = new Notification(session, user, promotion,
            promotion.IsLimited ? staffs[noftifierChooser.Next(0, staffs.Count)] : null);

      tx.Complete();
    }

    [Test]
    public void MembersAsAliasForSubqhery1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            anon,
            anon.promo,
            notificationsAlias1 = anon.notifications,
            notificationsAlias2 = anon.notifications,
          })
          .Select(anon => new {
            contacted = anon.notificationsAlias1.Select(c => c.Recipient.User.Id)
              .Union(anon.notificationsAlias2.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];
          var a1contacted = a1.contacted.ToArray();
          var a2contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1contacted.SequenceEqual(a2contacted));
          Assert.That(a1contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void MembersAsAliasForSubqhery2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            anon,
            anon.promo,
            notificationsAlias1 = anon.notifications,
            notificationsAlias2 = anon.notifications,
          })
          .Select(anon => new {
            contacted = anon.anon.notifications.Select(c => c.Recipient.User.Id)
              .Union(anon.notificationsAlias1.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];
          var a1contacted = a1.contacted.ToArray();
          var a2contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1contacted.SequenceEqual(a2contacted));
          Assert.That(a1contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void MembersAsAliasForSubqhery3()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            anon,
            anon.promo,
            notificationsAlias1 = anon.notifications,
            notificationsAlias2 = anon.notifications,
          })
          .Select(anon => new {
            contacted = anon.notificationsAlias1.Select(c => c.Recipient.User.Id)
              .Union(anon.anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];
          var a1contacted = a1.contacted.ToArray();
          var a2contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1contacted.SequenceEqual(a2contacted));
          Assert.That(a1contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithExcept()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql, "No support for Except");

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Except(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for(var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1contacted = a1.contacted.ToArray();
          var a2contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1contacted.SequenceEqual(a2contacted));
          Assert.That(a1contacted.Length, Is.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithIntersect()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql, "No support for Intersect");

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Intersect(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Intersect(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];
          var a1contacted = a1.contacted.ToArray();
          var a2contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1contacted.SequenceEqual(a2contacted));
          Assert.That(a1contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithUnion()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];
          var a1contacted = a1.contacted.ToArray();
          var a2contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1contacted.SequenceEqual(a2contacted));
          Assert.That(a1contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithConcat()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];
          var a1contacted = a1.contacted.ToArray();
          var a2contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1contacted.SequenceEqual(a2contacted));
          Assert.That(a1contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithAny()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Any(c => c.Recipient.User.Id > 25),
            less25 = anon.notifications.Any(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Any(c => c.Recipient.User.Id > 25),
            less25 = session.Query.All<Notification>().Any(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.more25, Is.True);
          Assert.That(a1.more25, Is.EqualTo(a2.more25));
          Assert.That(a1.less25, Is.True);
          Assert.That(a1.less25, Is.EqualTo(a2.less25));
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithAll()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more0 = anon.notifications.All(c => c.Recipient.User.Id > 0),
            less210 = anon.notifications.All(c => c.Recipient.User.Id < 210),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more0 = session.Query.All<Notification>().All(c => c.Recipient.User.Id > 0),
            less210 = session.Query.All<Notification>().All(c => c.Recipient.User.Id < 210),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.more0, Is.True);
          Assert.That(a1.more0, Is.EqualTo(a2.more0));
          Assert.That(a1.less210, Is.True);
          Assert.That(a1.less210, Is.EqualTo(a2.less210));
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithAverage()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            average1 = anon.notifications.Average(c => c.Recipient.User.Id),
            average2 = anon.notifications.Average(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            average1 = session.Query.All<Notification>().Average(c => c.Recipient.User.Id),
            average2 = session.Query.All<Notification>().Average(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.average1, Is.Not.Zero);
          Assert.That(a1.average2, Is.Not.Zero);
          Assert.That(a1.average1, Is.EqualTo(a2.average1));
          Assert.That(a1.average2, Is.EqualTo(a2.average2));
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithCount()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Count(c => c.Recipient.User.Id > 25),
            less25 = anon.notifications.Count(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Count(c => c.Recipient.User.Id > 25),
            less25 = session.Query.All<Notification>().Count(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.more25, Is.Not.Zero);
          Assert.That(a1.less25, Is.Not.Zero);
          Assert.That(a1.more25, Is.EqualTo(a2.more25));
          Assert.That(a1.less25, Is.EqualTo(a2.less25));
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereDistinct()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Where(c => c.Recipient.User.Id > 25).Distinct(),
            less25 = anon.notifications.Where(c => c.Recipient.User.Id < 25).Distinct(),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id > 25).Distinct(),
            less25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id < 25).Distinct(),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1more25 = a1.more25.ToArray();
          var a1less25 = a1.less25.ToArray();
          var a2more25 = a2.more25.ToArray();
          var a2less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1more25.SequenceEqual(a2more25), Is.True);
          Assert.That(a1more25.Length, Is.Not.Zero);
          Assert.That(a1less25.SequenceEqual(a2less25), Is.True);
          Assert.That(a1less25.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithDistinctWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Distinct().Where(c => c.Recipient.User.Id > 25).Select(c => c.Recipient.User.Id),
            less25 = anon.notifications.Distinct().Where(c => c.Recipient.User.Id < 25).Select(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Distinct().Where(c => c.Recipient.User.Id > 25).Select(c => c.Recipient.User.Id),
            less25 = session.Query.All<Notification>().Distinct().Where(c => c.Recipient.User.Id < 25).Select(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1more25 = a1.more25.ToArray();
          var a1less25 = a1.less25.ToArray();
          var a2more25 = a2.more25.ToArray();
          var a2less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1more25.Length, Is.Not.Zero);
          Assert.That(a1less25.Length, Is.Not.Zero);
          Assert.That(a1more25.SequenceEqual(a2more25), Is.True);
          Assert.That(a1less25.SequenceEqual(a2less25), Is.True);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithFirst()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.First(c => c.Recipient.User.Id > 25),
            less25 = anon.notifications.First(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().First(c => c.Recipient.User.Id > 25),
            less25 = session.Query.All<Notification>().First(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.more25, Is.EqualTo(a2.more25));
          Assert.That(a1.less25, Is.EqualTo(a2.less25));
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithFirstOrDefault()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.FirstOrDefault(c => c.Recipient.User.Id > 25),
            less25 = anon.notifications.FirstOrDefault(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().FirstOrDefault(c => c.Recipient.User.Id > 25),
            less25 = session.Query.All<Notification>().FirstOrDefault(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.more25, Is.Not.Null);
          Assert.That(a1.more25, Is.EqualTo(a2.more25));
          Assert.That(a1.less25, Is.Not.Null);
          Assert.That(a1.less25, Is.EqualTo(a2.less25));
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithGroupBy()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            group1 = anon.notifications.GroupBy(c => c.Recipient.User.Id),
            group2 = anon.notifications.GroupBy(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            group1 = session.Query.All<Notification>().GroupBy(c => c.Recipient.User.Id),
            group2 = session.Query.All<Notification>().GroupBy(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          var a1Group1 = a1.group1.ToArray();
          var a2Group1 = a2.group2.ToArray();
          var a1Group2 = a1.group1.ToArray();
          var a2Group2 = a2.group2.ToArray();

          Assert.That(a1Group1.Select(g => g.Key).SequenceEqual(a2Group1.Select(g => g.Key)), Is.True);
          Assert.That(a1Group2.Select(g => g.Key).SequenceEqual(a2Group2.Select(g => g.Key)), Is.True);

          for(var j = 0; j < a1Group1.Length; j++) {
            var grouping1 = a1Group1[j];
            var grouping2 = a2Group1[j];

            Assert.That(grouping1.Key, Is.EqualTo(grouping2.Key));

            var grouping1Content = grouping1.ToArray();
            var grouping2Content = grouping2.ToArray();
            Assert.That(grouping1Content.SequenceEqual(grouping2Content), Is.True);
            Assert.That(grouping1Content.Length, Is.Not.Zero);
          }
          for (var j = 0; j < a1Group1.Length; j++) {
            var grouping1 = a1Group2[j];
            var grouping2 = a2Group2[j];
            Assert.That(grouping1.Key, Is.EqualTo(grouping2.Key));

            var grouping1Content = grouping1.ToArray();
            var grouping2Content = grouping2.ToArray();
            Assert.That(grouping1Content.SequenceEqual(grouping2Content), Is.True);
            Assert.That(grouping1Content.Length, Is.Not.Zero);
          }
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithGroupJoin()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
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
              (not, us) => us),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
         .Select(promo => new { promo })
         .Select(anon => new {
           group1 = session.Query.All<Notification>().GroupJoin(
             session.Query.All<User>(),
             notification => notification.Recipient.User.Id,
             user => user.Id,
             (not, us) => us),
           group2 = session.Query.All<Notification>().GroupJoin(
             session.Query.All<User>(),
             notification => notification.Recipient.User.Id,
             user => user.Id,
             (not, us) => us),
           promo = anon.promo
         }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          var a1Group1 = a1.group1.ToArray();
          var a2Group1 = a2.group2.ToArray();
          var a1Group2 = a1.group1.ToArray();
          var a2Group2 = a2.group2.ToArray();

          Assert.That(a1Group1.Length, Is.Not.Zero);
          Assert.That(a1Group1.Length, Is.EqualTo(a2Group1.Length));
          Assert.That(a1Group2.Length, Is.Not.Zero);
          Assert.That(a1Group2.Length, Is.EqualTo(a2Group2.Length));

          for (int j = 0, count = a1Group1.Length; j < count; j++) {
            var g1 = a1Group1[j].ToArray();
            var g2 = a2Group1[j].ToArray();
            Assert.That(g1.Length, Is.EqualTo(g2.Length));
            Assert.That(g1.Length, Is.Not.Zero);
            Assert.That(g1.SequenceEqual(g2), Is.True);
          }
          for (int j = 0, count = a2Group1.Length; j < count; j++) {
            var g1 = a1Group2[j].ToArray();
            var g2 = a2Group2[j].ToArray();
            Assert.That(g1.Length, Is.EqualTo(g2.Length));
            Assert.That(g1.Length, Is.Not.Zero);
            Assert.That(g1.SequenceEqual(g2), Is.True);
          }
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithJoin1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
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
              (not, us) => us),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            group1 = session.Query.All<Notification>().Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => us),
            group2 = session.Query.All<Notification>().Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => us),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          var a1Group1 = a1.group1.ToArray();
          var a2Group1 = a2.group2.ToArray();
          var a1Group2 = a1.group1.ToArray();
          var a2Group2 = a2.group2.ToArray();

          Assert.That(a1Group1.Length, Is.Not.Zero);
          Assert.That(a1Group1.Length, Is.EqualTo(a2Group1.Length));
          Assert.That(a1Group2.Length, Is.Not.Zero);
          Assert.That(a1Group2.Length, Is.EqualTo(a2Group2.Length));

          Assert.That(a1Group1.SequenceEqual(a2Group1), Is.True);
          Assert.That(a1Group2.SequenceEqual(a2Group2), Is.True);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithJoin2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            group1 = anon.notifications.Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => new { us, not }),
            group2 = anon.notifications.Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => new { us, not }),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            group1 = session.Query.All<Notification>().Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => new { us, not }),
            group2 = session.Query.All<Notification>().Join(
              session.Query.All<User>(),
              notification => notification.Recipient.User.Id,
              user => user.Id,
              (not, us) => new { us, not }),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          var a1Group1 = a1.group1.ToArray();
          var a2Group1 = a2.group2.ToArray();
          var a1Group2 = a1.group1.ToArray();
          var a2Group2 = a2.group2.ToArray();

          Assert.That(a1Group1.Length, Is.Not.Zero);
          Assert.That(a1Group1.Length, Is.EqualTo(a2Group1.Length));
          Assert.That(a1Group2.Length, Is.Not.Zero);
          Assert.That(a1Group2.Length, Is.EqualTo(a2Group2.Length));

          Assert.That(a1Group1.SequenceEqual(a2Group1), Is.True);
          Assert.That(a1Group2.SequenceEqual(a2Group2), Is.True);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithIn()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            in1 = anon.notifications.Where(c => c.Recipient.User.Id.In(new long[] { 1, 2, 3 })),
            in2 = anon.notifications.Where(c => c.Recipient.User.Id.In(new long[] { 4, 5, 6 })),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            in1 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id.In(new long[] { 1, 2, 3 })),
            in2 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id.In(new long[] { 4, 5, 6 })),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));

          var a1In1 = a1.in1.ToArray();
          var a1In2 = a1.in2.ToArray();
          var a2In1 = a2.in1.ToArray();
          var a2In2 = a2.in2.ToArray();

          Assert.That(a1In1.Length, Is.Not.Zero);
          Assert.That(a1In1.Length, Is.EqualTo(a2In1.Length));
          Assert.That(a1In2.Length, Is.Not.Zero);
          Assert.That(a2In2.Length, Is.EqualTo(a2In2.Length));

          Assert.That(a1In1.SequenceEqual(a2In1), Is.True);
          Assert.That(a1In2.SequenceEqual(a2In2), Is.True);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithMinMax()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            max = anon.notifications.Max(c => c.Recipient.User.Id),
            min = anon.notifications.Min(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            max = session.Query.All<Notification>().Max(c => c.Recipient.User.Id),
            min = session.Query.All<Notification>().Min(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.max, Is.Not.Zero);
          Assert.That(a1.max, Is.EqualTo(a2.max));
          Assert.That(a1.min, Is.Not.Zero);
          Assert.That(a1.min, Is.EqualTo(a2.min));
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithOfTypeWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.OfType<Notification>().Where(c => c.Recipient.User.Id > 25),
            less25 = anon.notifications.OfType<Notification>().Where(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().OfType<Notification>().Where(c => c.Recipient.User.Id > 25),
            less25 = session.Query.All<Notification>().OfType<Notification>().Where(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1more25 = a1.more25.ToArray();
          var a1less25 = a1.less25.ToArray();
          var a2more25 = a2.more25.ToArray();
          var a2less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1more25.SequenceEqual(a2more25), Is.True);
          Assert.That(a1more25.Length, Is.Not.Zero);
          Assert.That(a1less25.SequenceEqual(a2less25), Is.True);
          Assert.That(a1less25.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereOfType()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Where(c => c.Recipient.User.Id > 25).OfType<Notification>(),
            less25 = anon.notifications.Where(c => c.Recipient.User.Id < 25).OfType<Notification>(),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id > 25).OfType<Notification>(),
            less25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id < 25).OfType<Notification>(),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1more25 = a1.more25.ToArray();
          var a1less25 = a1.less25.ToArray();
          var a2more25 = a2.more25.ToArray();
          var a2less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1more25.SequenceEqual(a2more25), Is.True);
          Assert.That(a1more25.Length, Is.Not.Zero);
          Assert.That(a1less25.SequenceEqual(a2less25), Is.True);
          Assert.That(a1less25.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithOrderBy()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            order1 = anon.notifications.OrderBy(c => c.Recipient.User.Id),
            order2 = anon.notifications.OrderBy(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            order1 = session.Query.All<Notification>().OrderBy(c => c.Recipient.User.Id),
            order2 = session.Query.All<Notification>().OrderBy(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1order1 = a1.order1.ToArray();
          var a1order2 = a1.order2.ToArray();
          var a2order1 = a2.order1.ToArray();
          var a2order2 = a2.order2.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1order1.SequenceEqual(a2order1), Is.True);
          Assert.That(a1order1.Length, Is.Not.Zero);
          Assert.That(a1order2.SequenceEqual(a2order2), Is.True);
          Assert.That(a1order2.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithOrderByDescending()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            order1 = anon.notifications.OrderByDescending(c => c.Recipient.User.Id),
            order2 = anon.notifications.OrderByDescending(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            order1 = session.Query.All<Notification>().OrderByDescending(c => c.Recipient.User.Id),
            order2 = session.Query.All<Notification>().OrderByDescending(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1order1 = a1.order1.ToArray();
          var a1order2 = a1.order2.ToArray();
          var a2order1 = a2.order1.ToArray();
          var a2order2 = a2.order2.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1order1.SequenceEqual(a2order1), Is.True);
          Assert.That(a1order1.Length, Is.Not.Zero);
          Assert.That(a1order2.SequenceEqual(a2order2), Is.True);
          Assert.That(a1order2.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithSum()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            sum1 = anon.notifications.Sum(c => c.Recipient.User.Id),
            sum2 = anon.notifications.Sum(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            sum1 = session.Query.All<Notification>().Sum(c => c.Recipient.User.Id),
            sum2 = session.Query.All<Notification>().Sum(c => c.Recipient.User.Id),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1.sum1, Is.EqualTo(a2.sum1));
          Assert.That(a1.sum1, Is.Not.Zero);
          Assert.That(a1.sum2, Is.EqualTo(a2.sum2));
          Assert.That(a1.sum2, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithTakeWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Take(2).Where(c => c.Recipient.User.Id > 25),
            less25 = anon.notifications.Take(3).Where(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Take(2).Where(c => c.Recipient.User.Id > 25),
            less25 = session.Query.All<Notification>().Take(3).Where(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1More25 = a1.more25.ToArray();
          var a1Less25 = a1.less25.ToArray();
          var a2More25 = a2.more25.ToArray();
          var a2Less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1More25.Length, Is.EqualTo(0));
          Assert.That(a1Less25.Length, Is.EqualTo(3));
          Assert.That(a1More25.SequenceEqual(a2More25), Is.True);
          Assert.That(a1Less25.SequenceEqual(a2Less25), Is.True);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereTake()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Where(c => c.Recipient.User.Id > 25).Take(2),
            less25 = anon.notifications.Where(c => c.Recipient.User.Id < 25).Take(3),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id > 25).Take(2),
            less25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id < 25).Take(3),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1More25 = a1.more25.ToArray();
          var a1Less25 = a1.less25.ToArray();
          var a2More25 = a2.more25.ToArray();
          var a2Less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1More25.Length, Is.EqualTo(2));
          Assert.That(a1Less25.Length, Is.EqualTo(3));
          Assert.That(a1More25.SequenceEqual(a2More25), Is.True);
          Assert.That(a1Less25.SequenceEqual(a2Less25), Is.True);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithSkipWhere()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expectedMore25Count = session.Query.All<Notification>().Skip(2).Where(c => c.Recipient.User.Id > 25).Count();
        var expectedLess25Count = session.Query.All<Notification>().Skip(3).Where(c => c.Recipient.User.Id < 25).Count();

        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Skip(2).Where(c => c.Recipient.User.Id > 25),
            less25 = anon.notifications.Skip(3).Where(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Skip(2).Where(c => c.Recipient.User.Id > 25),
            less25 = session.Query.All<Notification>().Skip(3).Where(c => c.Recipient.User.Id < 25),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1More25 = a1.more25.ToArray();
          var a1Less25 = a1.less25.ToArray();
          var a2More25 = a2.more25.ToArray();
          var a2Less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1More25.Length, Is.EqualTo(expectedMore25Count));
          Assert.That(a1Less25.Length, Is.EqualTo(expectedLess25Count));
          Assert.That(a1More25.SequenceEqual(a2More25), Is.True);
          Assert.That(a1Less25.SequenceEqual(a2Less25), Is.True);
        }
      }
    }

    [Test]
    public void BaseQueryReuseWithWhereSkip()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expectedMore25Count = session.Query.All<Notification>().Where(c => c.Recipient.User.Id > 25).Skip(2).Count();
        var expectedLess25Count = session.Query.All<Notification>().Where(c => c.Recipient.User.Id < 25).Skip(3).Count();

        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            more25 = anon.notifications.Where(c => c.Recipient.User.Id > 25).Skip(2),
            less25 = anon.notifications.Where(c => c.Recipient.User.Id < 25).Skip(3),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            more25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id > 25).Skip(2),
            less25 = session.Query.All<Notification>().Where(c => c.Recipient.User.Id < 25).Skip(3),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1More25 = a1.more25.ToArray();
          var a1Less25 = a1.less25.ToArray();
          var a2More25 = a2.more25.ToArray();
          var a2Less25 = a2.less25.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1More25.Length, Is.EqualTo(expectedMore25Count));
          Assert.That(a1Less25.Length, Is.EqualTo(expectedLess25Count));
          Assert.That(a1More25.SequenceEqual(a2More25), Is.True);
          Assert.That(a1Less25.SequenceEqual(a2Less25), Is.True);
        }
      }
    }

    [Test]
    public void CastQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().Cast<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Cast<Notification>().Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().Cast<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void DistinctQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().Distinct()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Distinct().Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().Distinct().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupByQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().GroupBy(n => n.Id)
          })
          .Select(anon => new {
            contacted = anon.notifications.SelectMany(g => g.Select(c =>c.Recipient.User.Id))
              .Union(anon.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().GroupBy(n => n.Id).SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(session.Query.All<Notification>().GroupBy(n => n.Id).SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void WhereGroupByQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().Where(n=> n.Id > 0).GroupBy(n => n.Id)
          })
          .Select(anon => new {
            contacted = anon.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(anon.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Where(n => n.Id > 0).GroupBy(n => n.Id).SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(session.Query.All<Notification>().Where(n => n.Id > 0).GroupBy(n => n.Id).SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupByWhereQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().GroupBy(n => n.Id).Where(g => g.Key > 0)
          })
          .Select(anon => new {
            contacted = anon.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(anon.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().GroupBy(n => n.Id).Where(g => g.Key > 0).SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(session.Query.All<Notification>().GroupBy(n => n.Id).Where(g => g.Key > 0).SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
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
          .Select(anon => new {
            contacted = anon.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(anon.notifications.SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();


        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .Where(n => n.Id > 0)
                .GroupBy(n => n.Id)
                .Where(g => g.Key > 0)
                .SelectMany(g => g.Select(c => c.Recipient.User.Id))
              .Union(session.Query.All<Notification>()
                .Where(n => n.Id > 0)
                .GroupBy(n => n.Id)
                .Where(g => g.Key > 0)
                .SelectMany(g => g.Select(c => c.Recipient.User.Id))),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
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
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n=>n.TriggeredBy.Id,
                u => u.Id,
                (n, u)=> new { n, u })
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u }).Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u }).Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuseAndOneDistinctApplied1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
          })
          .Select(anon => new {
            contacted = anon.notifications.Distinct().Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Distinct()
                .Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuseAndOneDistinctApplied2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Distinct().Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u }).Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Distinct()
                .Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuseAndOneDistinctApplied3()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
          })
          .Select(anon => new {
            contacted = anon.notifications.Distinct().Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Distinct().Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Distinct()
                .Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Distinct()
                .Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinWithDistinctQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
              .Distinct()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
            .Distinct().Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
            .Distinct().Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
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
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => n)
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => n)
                .Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => n)
                .Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuse2()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n })
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n })
                .Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n })
                .Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuse3()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => n)
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => n).Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => n).Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuse4()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { ng = new { n , u }, nu = u })
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.ng).Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.ng).Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { ng = new { n, u }, nu = u })
                .Select(c => c.ng)
                .Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { ng = new { n, u }, nu = u })
                .Select(c => c.ng)
                .Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuse5()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(n => n.n).Select(c => c.Recipient.User.Id)
              .Union(anon.notifications.Select(n => n.n).Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Select(n => n.n).Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Select(n => n.n)
                .Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void GroupJoinQueryReuse6()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
              .GroupJoin(session.Query.All<User>(),
                n => n.TriggeredBy.Id,
                u => u.Id,
                (n, u) => new { n, u })
          })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(n => new { n }).Select(c => c.n.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(n => new { n }).Select(c => c.n.Recipient.User.Id)
              .Union(session.Query.All<Notification>()
                .GroupJoin(session.Query.All<User>(),
                  n => n.TriggeredBy.Id,
                  u => u.Id,
                  (n, u) => new { n, u })
                .Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
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
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.n.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
              .  Join(session.Query.All<User>(), n => n.TriggeredBy.Id, u => u.Id, (n, u) => new { n, u }).Select(c => c.n.Recipient.User.Id)
              .Concat(session.Query.All<Notification>()
                .Join(session.Query.All<User>(), n => n.TriggeredBy.Id, u => u.Id, (n, u) => new { n, u }).Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
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
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.n.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .LeftJoin(session.Query.All<User>(), n => n.TriggeredBy.Id, u => u.Id, (n, u) => new { n, u }).Select(c => c.n.Recipient.User.Id)
              .Concat(session.Query.All<Notification>()
                .LeftJoin(session.Query.All<User>(), n => n.TriggeredBy.Id, u => u.Id, (n, u) => new { n, u }).Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void OfTypeQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().OfType<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().OfType<Notification>().Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().OfType<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void OrderByQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().OrderBy(n => n.Id)
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().OrderBy(n => n.Id).Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().OrderBy(n => n.Id).Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void OrderByDescendingQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().OrderByDescending(n => n.Id)
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().OrderByDescending(n => n.Id).Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().OrderByDescending(n => n.Id).Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void SelectQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().Select(n => new { n, n.Id })
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.n.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();


        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(n => new { n, n.Id }).Select(c => c.n.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().Select(n => new { n, n.Id }).Select(c => c.n.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void SkipQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().Skip(3)
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Skip(3).Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().Skip(3).Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void TakeQueryReuse()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>().Take(8)
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Take(8).Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>().Take(8).Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void IntersectOfWheresQueryReuse()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql, "No support for Intersect");

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n => n.Id > 25)
              .Intersect(session.Query.All<Notification>().Where(n => n.Id < 250))
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted =
              session.Query.All<Notification>()
                .Where(n => n.Id > 25)
                .Intersect(session.Query.All<Notification>().Where(n => n.Id < 250))
                .Select(c => c.Recipient.User.Id)
              .Concat(
                session.Query.All<Notification>()
                  .Where(n => n.Id > 25)
                  .Intersect(session.Query.All<Notification>().Where(n => n.Id < 250))
                  .Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void ExceptOfWheresQueryReuse()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.MySql, "No support for Except");

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n => n.Id > 250)
              .Except(session.Query.All<Notification>().Where(n => n.Id < 25))
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Where(n => n.Id > 250)
                .Except(session.Query.All<Notification>().Where(n => n.Id < 25))
                .Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>()
                .Where(n => n.Id > 250)
                .Except(session.Query.All<Notification>().Where(n => n.Id < 25))
                .Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
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
            notifications = session.Query.All<Notification>().Where(n => n.Id > 250)
              .Concat(session.Query.All<Notification>().Where(n => n.Id < 25))
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .Where(n => n.Id > 250)
                .Concat(session.Query.All<Notification>().Where(n => n.Id < 25))
                .Select(c => c.Recipient.User.Id)
              .Concat(session.Query.All<Notification>()
                .Where(n => n.Id > 250)
                .Concat(session.Query.All<Notification>().Where(n => n.Id < 25))
                .Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
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
            notifications = session.Query.All<Notification>().Where(n => n.Id > 250)
              .Union(session.Query.All<Notification>().Where(n => n.Id < 25))
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Concat(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        var expected = session.Query.All<Promotion>()
          .Select(promo => new { promo })
          .Select(anon => new {
            contacted = session.Query.All<Notification>()
                .Where(n => n.Id > 250)
                .Union(session.Query.All<Notification>().Where(n => n.Id < 25))
                .Select(c => c.Recipient.User.Id)
              .Concat(
                session.Query.All<Notification>()
                  .Where(n => n.Id > 250)
                  .Union(session.Query.All<Notification>().Where(n => n.Id < 25))
                  .Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();

        Assert.That(query.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++) {
          var a1 = expected[0];
          var a2 = query[0];

          var a1Contacted = a1.contacted.ToArray();
          var a2Contacted = a2.contacted.ToArray();

          Assert.That(a1.promo.Id, Is.EqualTo(a2.promo.Id));
          Assert.That(a1Contacted.SequenceEqual(a2Contacted));
          Assert.That(a1Contacted.Length, Is.Not.Zero);
        }
      }
    }

    [Test]
    public void OnePropOneTrueQueryRoot()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
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
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
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
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = anon.notifications.Select(c => c.Recipient.User.Id)
              .Union(session.Query.All<Notification>().Select(c => c.Recipient.User.Id)),
            promo = anon.promo
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
            promo, notifications = session.Query.All<Notification>()
          })
          .Select(anon => new {
            contacted = session.Query.All<Notification>().Select(c => c.Recipient.User.Id)
              .Union(anon.notifications.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
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
          .Select(anon => new {
            contacted = anon.notifications1.Select(c => c.Recipient.User.Id)
              .Union(anon.notifications2.Select(c => c.Recipient.User.Id)),
            promo = anon.promo
          }).ToArray();
      }
    }
  }
}
