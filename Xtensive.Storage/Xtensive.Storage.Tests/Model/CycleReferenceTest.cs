// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.16

using System.Reflection;
using NUnit.Framework;

namespace Xtensive.Storage.Tests.Model.CycleReferenceTestModel
{
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

  public class Child : Parent
  {
  }

  [HierarchyRoot]
  public class Neighbor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, Association(PairTo = "Neighbor")]
    public EntitySet<Child> Persons { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class CycleReferenceTest
  {
    [Test]
    // [ExpectedException(typeof(InvalidOperationException))]
    public void CombinedTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.CycleReferenceTestModel");

      using (var domain = Domain.Build(config))
      using (Session.Open(domain))
      using (var t = Transaction.Open()) {
        t.Complete();
      }
    }
  }
}