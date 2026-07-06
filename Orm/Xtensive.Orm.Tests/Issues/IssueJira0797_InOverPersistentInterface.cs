// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0797_InOverPersistentInterfaceModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0797_InOverPersistentInterfaceModel
{
  public interface IPersistentObject
  {
    [Field, Key]
    long Id { get; }

    [Field, Version]
    int VersionId { get; }
  }

  public abstract class PersistentObject : Entity, IPersistentObject
  {
    public long Id { get; private set; }

    public int VersionId { get; private set; }

    public PersistentObject(Session session)
      : base(session)
    {
    }
  }

  public interface IHasReasons : IEntity, IPersistentObject
  {
    IHasReasons ReasonParent { get; }

    [Field(Length = 150)]
    string Name { get; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear, PairTo = nameof(ImplemetorDependant.Owner))]
    EntitySet<ImplemetorDependant> Items { get; }
  }

  [HierarchyRoot]
  public class RootImplementor1 : PersistentObject, IHasReasons
  {
    [Field]
    public IHasReasons ReasonParent { get; set; }

    [Field(Length = 150)]
    public string Name { get; private set; }

    [Field]
    public EntitySet<ImplemetorDependant> Items { get; private set; }

    public RootImplementor1(Session session, string name)
      : base(session)
    {
      Name = name;
      ReasonParent = null;
    }
  }

  [HierarchyRoot]
  public class HierarchicalImplementor2 : PersistentObject, IHasReasons
  {
    [Field]
    public IHasReasons ReasonParent { get; private set; }

    [Field(Length = 150)]
    public string Name { get; set; }

    [Field]
    public EntitySet<ImplemetorDependant> Items { get; private set; }

    public HierarchicalImplementor2(Session session, string name, IHasReasons parent)
      : base(session)
    {
      Name = name;
      ReasonParent = parent;
    }
  }

  [HierarchyRoot]
  public class ImplemetorDependant : PersistentObject
  {
    [Field(Length = 150)]
    public string Reason { get; set; }

    [Field(Length = 500)]
    public string Description { get; set; }

    [Field]
    public IHasReasons Owner { get; set; }

    public ImplemetorDependant(Session session, string reason, string description)
      : base(session)
    {
      Reason = reason;
      Description = description;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0797_InOverPersistentInterface : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(IPersistentObject));
      config.Types.Register(typeof(PersistentObject));
      config.Types.Register(typeof(ImplemetorDependant));
      config.Types.Register(typeof(RootImplementor1));
      config.Types.Register(typeof(HierarchicalImplementor2));
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var enterprise1 = new RootImplementor1(session, "enterprise1");
        var site1 = new HierarchicalImplementor2(session, "site1", enterprise1);
        var site2 = new HierarchicalImplementor2(session, "site2", enterprise1);

        var reason1 = new ImplemetorDependant(session, $"reason1 {enterprise1.Name}", $"{enterprise1.Name} reason1 reason1") { Owner = enterprise1 };
        var reason2 = new ImplemetorDependant(session, $"reason2 {enterprise1.Name}", $"{enterprise1.Name} reason2 reason2") { Owner = enterprise1 };

        var reason3 = new ImplemetorDependant(session, $"reason1 {site1.Name}", $"{site1.Name} reason1 reason1") { Owner = site1 };
        var reason4 = new ImplemetorDependant(session, $"reason2 {site1.Name}", $"{site1.Name} reason2 reason2") { Owner = site1 };

        var reason5 = new ImplemetorDependant(session, $"reason1 {site2.Name}", $"{site2.Name} reason1 reason1") { Owner = site2 };
        var reason6 = new ImplemetorDependant(session, $"reason2 {site2.Name}", $"{site2.Name} reason2 reason2") { Owner = site2 };

        var enterprise2 = new RootImplementor1(session, "enterprise2");
        var site3 = new HierarchicalImplementor2(session, "anothersite1", enterprise2);
        var site4 = new HierarchicalImplementor2(session, "anothersite2", enterprise2);

        reason1 = new ImplemetorDependant(session, $"reason1 {enterprise2.Name}", $"{enterprise2.Name} reason1 reason1") { Owner = enterprise2 };
        reason2 = new ImplemetorDependant(session, $"reason2 {enterprise2.Name}", $"{enterprise2.Name} reason2 reason2") { Owner = enterprise2 };

        reason3 = new ImplemetorDependant(session, $"reason1 {site3.Name}", $"{site3.Name} reason1 reason1") { Owner = site3 };
        reason4 = new ImplemetorDependant(session, $"reason2 {site3.Name}", $"{site3.Name} reason2 reason2") { Owner = site3 };

        reason5 = new ImplemetorDependant(session, $"reason1 {site4.Name}", $"{site4.Name} reason1 reason1") { Owner = site4 };
        reason6 = new ImplemetorDependant(session, $"reason2 {site4.Name}", $"{site4.Name} reason2 reason2") { Owner = site4 };

        tx.Complete();
      }
    }

    [Test]
    public void InPersistentInterfaceComplexConditionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var enterprise = session.Query.All<RootImplementor1>().FirstOrDefault(e => e.Name == "enterprise1");
        var site = session.Query.All<HierarchicalImplementor2>().FirstOrDefault(s => s.Name == "anothersite2");
        var filter = new IHasReasons[] { enterprise, site };
        var reaons = session.Query.All<ImplemetorDependant>().Where(l => l.Owner.In(IncludeAlgorithm.ComplexCondition, filter)).ToArray();

        foreach (var reason in reaons) {
          Assert.That(reason.Owner, Is.EqualTo(enterprise).Or.EqualTo(site));
        }
      }
    }

    [Test]
    public void IsExectImplementationComplexConditionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var enterprise = session.Query.All<RootImplementor1>().FirstOrDefault(e => e.Name == "enterprise1");
        var filter1 = new RootImplementor1[] { enterprise };
        var reasons = session.Query.All<ImplemetorDependant>().Where(l => l.Owner.In(IncludeAlgorithm.ComplexCondition, filter1)).ToArray();

        foreach (var reason in reasons) {
          Assert.That(reason.Owner, Is.EqualTo(enterprise));
        }

        var site = session.Query.All<HierarchicalImplementor2>().FirstOrDefault(e => e.Name == "anothersite2");
        var filter2 = new HierarchicalImplementor2[] { site };
        reasons = session.Query.All<ImplemetorDependant>().Where(l => l.Owner.In(IncludeAlgorithm.ComplexCondition, filter2)).ToArray();

        foreach (var reason in reasons) {
          Assert.That(reason.Owner, Is.EqualTo(site));
        }
      }
    }

    [Test]
    public void InPersistentInterfaceTempTableTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var enterprise = session.Query.All<RootImplementor1>().FirstOrDefault(e => e.Name == "enterprise1");
        var site = session.Query.All<HierarchicalImplementor2>().FirstOrDefault(s => s.Name == "anothersite2");
        var filter = new IHasReasons[] { enterprise, site };
        var reaons = session.Query.All<ImplemetorDependant>().Where(l => l.Owner.In(IncludeAlgorithm.TemporaryTable, filter)).ToArray();

        foreach (var reason in reaons) {
          Assert.That(reason.Owner, Is.EqualTo(enterprise).Or.EqualTo(site));
        }
      }
    }

    [Test]
    public void InExectImplementationTempTableTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var enterprise = session.Query.All<RootImplementor1>().FirstOrDefault(e => e.Name == "enterprise1");
        var filter1 = new RootImplementor1[] { enterprise };
        var reasons = session.Query.All<ImplemetorDependant>().Where(l => l.Owner.In(IncludeAlgorithm.TemporaryTable, filter1)).ToArray();

        foreach (var reason in reasons) {
          Assert.That(reason.Owner, Is.EqualTo(enterprise));
        }

        var site = session.Query.All<HierarchicalImplementor2>().FirstOrDefault(e => e.Name == "anothersite2");
        var filter2 = new HierarchicalImplementor2[] { site };
        reasons = session.Query.All<ImplemetorDependant>().Where(l => l.Owner.In(IncludeAlgorithm.TemporaryTable, filter2)).ToArray();

        foreach (var reason in reasons) {
          Assert.That(reason.Owner, Is.EqualTo(site));
        }
      }
    }

    [Test]
    public void ContainsPersistentInterfaceComplexConditionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var enterprise = session.Query.All<RootImplementor1>().FirstOrDefault(e => e.Name == "enterprise1");
        var site = session.Query.All<HierarchicalImplementor2>().FirstOrDefault(s => s.Name == "anothersite2");
        var filter = new IHasReasons[] { enterprise, site };
        var reaons = session.Query.All<ImplemetorDependant>().Where(l => filter.Contains(l.Owner)).ToArray();

        foreach (var reason in reaons) {
          Assert.That(reason.Owner, Is.EqualTo(enterprise).Or.EqualTo(site));
        }
      }
    }

    [Test]
    public void ContainsExectImplementationComplexConditionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var enterprise = session.Query.All<RootImplementor1>().FirstOrDefault(e => e.Name == "enterprise1");
        var filter1 = new RootImplementor1[] { enterprise };
        var reasons = session.Query.All<ImplemetorDependant>().Where(l => filter1.Contains(l.Owner)).ToArray();

        foreach (var reason in reasons) {
          Assert.That(reason.Owner, Is.EqualTo(enterprise));
        }

        var site = session.Query.All<HierarchicalImplementor2>().FirstOrDefault(e => e.Name == "anothersite2");
        var filter2 = new HierarchicalImplementor2[] { site };
        reasons = session.Query.All<ImplemetorDependant>().Where(l => filter2.Contains(l.Owner)).ToArray();

        foreach (var reason in reasons) {
          Assert.That(reason.Owner, Is.EqualTo(site));
        }
      }
    }
  }
}

