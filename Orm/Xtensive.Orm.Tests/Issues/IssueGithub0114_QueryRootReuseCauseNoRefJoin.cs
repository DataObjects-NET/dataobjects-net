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
    public void PureRootQueryReuse1()
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
    public void PureRootQueryReuseWithAllAnonTypeProps()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>()
          })
          .Select(anon1 => new {
            promo1 = anon1.promo,
            contacted = anon1.notifications.Select(c => c.Recipient.User.Id)
              .Except(anon1.notifications.Select(c => c.Recipient.User.Id))
          }).ToArray();
      }
    }

    [Test]
    public void SubqueryReuse1()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Promotion>()
          .Select(promo => new {
            promo,
            notifications = session.Query.All<Notification>().Where(n => n.Id < promo.Id)
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
