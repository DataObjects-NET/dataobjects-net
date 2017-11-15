// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Interfaces.UnusedTypeRemovalTestModel;

namespace Xtensive.Orm.Tests.Interfaces.UnusedTypeRemovalTestModel
{
  public interface IFirst : IEntity
  {
    
  }

  public interface ISecond : IFirst
  {
    
  }

  public interface IThird : IFirst
  {
    
  }

  [Serializable]
  // No [HierarchyRoot] here
  public class Third : Entity, IThird
  {
    [Field, Key]
    public int Id { get; private set; }
    [Field]
    public IFirst First { get; set; }
  }

  [HierarchyRoot]
  public class First : Entity, IFirst
  {
    [Field, Key]
    public int Id { get; private set; }
    [Field]
    public ISecond Second { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Interfaces
{
  public class UnusedTypeRemovalTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      Assert.IsNull(Domain);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (IFirst).Assembly, typeof (IFirst).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      try {
        base.BuildDomain(configuration);
        Assert.Fail();
      }
      catch (DomainBuilderException e) {
        Console.WriteLine(e);
      }
      return null;
    }
  }
}