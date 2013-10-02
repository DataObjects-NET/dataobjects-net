// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.27

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.DiscardedAssociationsBugModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace DiscardedAssociationsBugModel
  {
    public class EntityBase : Entity
    {
      [Key, Field]
      public int Id { get; private set; }
    }

    public interface ISlave<T>
      where T : Entity
    {
      [Field(Indexed = false)]
      [Association(OnTargetRemove = OnRemoveAction.Cascade, OnOwnerRemove = OnRemoveAction.Clear)]
      T Owner { get; }
    }

    public abstract class Slave<T> : EntityBase, ISlave<T>
      where T : Entity
    {
      public T Owner { get; set; }
    }

    [HierarchyRoot]
    public class MasterBase : EntityBase
    {
    }

    [HierarchyRoot]
    public class SlaveBase : Slave<MasterBase>
    {
    }

    public class Master1 : MasterBase
    {
      public class Slave1 : SlaveBase
      {
      }
    }

    public class Master2 : MasterBase
    {
      [Field, Association(PairTo = "Owner")]
      public EntitySet<Slave2> Slaves { get; private set; }

      public class Slave2 : SlaveBase
      {
      }
    }
  }

  public class AssociationsOverrideTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      config.Types.Register(typeof (Slave<>).Assembly, typeof (Slave<>).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var m1 = new Master1();
        var m2 = new Master2();
        new Master1.Slave1 {Owner = m1};
        new Master2.Slave2 {Owner = m2};
        tx.Complete();
      }
    }

    [Test]
    public void Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var s1 = Query.All<Master1.Slave1>().Single();
        var s2 = Query.All<Master2.Slave2>().Single();

        Assert.That(s1.Owner, Is.Not.Null);
        Assert.That(s2.Owner, Is.Not.Null);
      }
    }
  }
}