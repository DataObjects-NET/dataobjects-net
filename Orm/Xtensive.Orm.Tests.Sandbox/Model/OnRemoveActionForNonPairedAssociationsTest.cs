// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.04.26

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.OnRemoveActionForNonPairedAssociationsTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace OnRemoveActionForNonPairedAssociationsTestModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field, Association(OnTargetRemove = OnRemoveAction.Cascade)]
      public Target T1 { get; set; }

      [Field]
      public EntitySet<Target> T2 { get; set; }
    }

    [HierarchyRoot]
    public class Target : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }
  }

  public class OnRemoveActionForNonPairedAssociationsTest : AutoBuildTest
  {
    private Key o1Key;
    private Key o2Key;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var o1 = new Owner {T1 = new Target()};
        var o2 = new Owner {T2 = {new Target()}};
        o1Key = o1.Key;
        o2Key = o2.Key;
        tx.Complete();
      }
    }

    [Test]
    public void RemoveO1Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var o1 = Query.Single<Owner>(o1Key);
        o1.Remove();
        tx.Complete();
      }
    }

    [Test]
    public void RemoveO2Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var o2 = Query.Single<Owner>(o2Key);
        o2.Remove();
        tx.Complete();
      }
    }
  }
}