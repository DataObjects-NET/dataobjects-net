// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.10.01

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.SingleTableTestModel;
using System.Linq;
using Node = Xtensive.Orm.Model.Node;

namespace Xtensive.Orm.Tests.Storage
{
  namespace SingleTableTestModel
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class Base : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    public class DerivedNode : Base
    {
      [Field]
      public string Description { get; set; }
    }

    public class Derived : Base
    {
      [Field]
      public string Name { get; set; }
    }

    public class Leaf : Derived
    {
      [Field]
      public string Description { get; set; }
    }
  }

  [TestFixture]
  public class SingleTableTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Base).Assembly, typeof (Base).Namespace);
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Base();
        new Derived() {Name = "Derived"};
        new Leaf() {Name = "Leaf", Description = "Green"};
        new DerivedNode() {Description = "Node"};

        var primaryIndex = Domain.Model.Types[typeof (Base)].Indexes.PrimaryIndex;
        var rs = primaryIndex.ToRecordQuery().ToRecordSet(session);
        var result = rs.ToEntities(0).ToList();
        t.Complete();
      }
    }
  }
}