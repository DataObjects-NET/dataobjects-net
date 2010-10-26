// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.21

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.ConditionalTestModel;
using Xtensive.Orm.Tests.Storage.ForeignKeys;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq.ConditionalTestModel
{
  [Serializable]
  [KeyGenerator(typeof(DualIntKeyGenerator), Name = "DualInt")]
  [HierarchyRoot]
  public class Root1 : Entity
  {
    [Field, Key(0)]
    public int Id1 { get; private set; }

    [Field, Key(1)]
    public int Id2 { get; private set; }
  }

  [Serializable]
  public class A : Root1
  {
    [Field]
    public int FieldA { get; private set; }
  }

  [Serializable]
  public class B : Root1
  {
    [Field]
    public int FieldB { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Root2 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Number{ get; set;}

    [Field]
    public Root1 Child1{ get; set;}

    [Field]
    public Root1 Child2{ get; set;}
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  public class ConditionalTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Root1).Assembly, typeof (Root1).Namespace);
      config.Types.Register(typeof (DualIntKeyGenerator));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var a1 = new A();
          var b1 = new B();
          var r1 = new Root2() {Number = 123, Child1 = a1, Child2=b1};
          Session.Current.SaveChanges();
          var result = session.Query.All<Root2>().Select(r=> r.Number == 123 ? r.Child1 : r.Child2);
          QueryDumper.Dump(result);
          // Rollback
        }
      }
    }
  }
}