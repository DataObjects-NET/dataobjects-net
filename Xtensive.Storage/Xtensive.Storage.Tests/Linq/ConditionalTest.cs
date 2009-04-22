// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.21

using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Linq.ConditionalTestModel;
using Xtensive.Storage.Tests.Storage.ForeignKeys;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq.ConditionalTestModel
{
  [HierarchyRoot("Id1", "Id2", KeyGenerator = typeof(DualIntKeyGenerator))]
  public class Root1 : Entity
  {
    [Field]
    public int Id1 { get; private set; }

    [Field]
    public int Id2 { get; private set; }
  }

  public class A : Root1
  {
    [Field]
    public int FieldA { get; private set; }
  }

  public class B : Root1
  {
    [Field]
    public int FieldB { get; private set; }
  }

  [HierarchyRoot("Id", KeyGenerator = typeof(KeyGenerator))]
  public class Root2 : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public int Number{ get; set;}

    [Field]
    public Root1 Child1{ get; set;}

    [Field]
    public Root1 Child2{ get; set;}
  }
}

namespace Xtensive.Storage.Tests.Linq
{
  public class ConditionalTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Root1).Assembly, typeof (Root1).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var a1 = new A();
          var b1 = new B();
          var r1 = new Root2() {Number = 123, Child1 = a1, Child2=b1};
          Session.Current.Persist();
          var result = Query<Root2>.All.Select(r=> r.Number == 123 ? r.Child1 : r.Child2);
          QueryDumper.Dump(result);
          // Rollback
        }
      }
    }
  }
}