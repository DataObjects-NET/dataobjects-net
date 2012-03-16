// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.16

using System;
using System.Reflection;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Model.CycleReferenceTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Neighbor Neighbor { get; set; }
  }

  [Serializable]
  public class Child : Parent
  {
  }

  [Serializable]
  [HierarchyRoot]
  public class Neighbor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, Association(PairTo = "Neighbor")]
    public EntitySet<Child> Persons { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class CycleReferenceTest
  {
    [Test]
    // [ExpectedException(typeof(InvalidOperationException))]
    public void CombinedTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Model.CycleReferenceTestModel");

      using (var domain = Domain.Build(config))
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        t.Complete();
      }
    }
  }
}