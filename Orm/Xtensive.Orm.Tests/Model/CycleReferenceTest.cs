// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.16

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.CycleReferenceTestModel;

namespace Xtensive.Orm.Tests.Model.CycleReferenceTestModel
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

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture, Category("Model")]
  public class CycleReferenceTest : DomainBuildabilityTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Parent));
      config.Types.Register(typeof(Child));
      config.Types.Register(typeof(Neighbor));
      return config;
    }
  }
}