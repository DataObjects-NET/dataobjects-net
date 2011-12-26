// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.09

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Interfaces.DoubleIndexedInterfaceTestModel;

namespace Xtensive.Orm.Tests.Interfaces.DoubleIndexedInterfaceTestModel
{
  [HierarchyRoot]
  public class Victim : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
  }

  public interface ITest : IEntity
  {
    [Field]
    Victim Victim1 { get; }
    [Field]
    Victim Victim2 { get; }
  }

  [HierarchyRoot]
  public class TestImpl1 : Entity, ITest
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Victim Victim1 { get; private set; }

    [Field]
    public Victim Victim2 { get; private set; }
  }
  
  [HierarchyRoot]
  public class TestImpl2 : Entity, ITest
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Victim Victim1 { get; private set; }

    [Field]
    public Victim Victim2 { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Interfaces
{
  public class DoubleIndexedInterfaceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (ITest).Assembly, typeof (ITest).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      // Just ensure that domain builds
    }
  }
}