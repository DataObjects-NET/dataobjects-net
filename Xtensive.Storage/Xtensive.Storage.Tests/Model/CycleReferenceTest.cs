// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.16

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Model.CycleReferenceTestModel
{
  [HierarchyRoot("Id")]
  public class Parent : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Neighbor Neighbor { get; set; }
  }

  public class Child : Parent
  {
  }

  [HierarchyRoot("Id")]
  public class Neighbor : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field(PairTo = "Neighbor")]
    public EntitySet<Child> As { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class CycleReferenceTest
  {
    [Test]
    [ExpectedException(typeof(DomainBuilderException))]
    public void CombinedTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.CycleReferenceTestModel");

      using (var domain = Domain.Build(config))
      using (domain.OpenSession())
      using (var t = Transaction.Open()) {
        t.Complete();
      }
    }
  }
}