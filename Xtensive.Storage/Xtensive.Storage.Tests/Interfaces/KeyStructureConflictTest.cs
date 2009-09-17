// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.10

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Interfaces.KeyStructureConflictTestModel;

namespace Xtensive.Storage.Tests.Interfaces.KeyStructureConflictTestModel
{
  public interface IChild : IEntity
  {
  }

  [HierarchyRoot]
  public class Root1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class Child1 : Root1, IChild
  {
    
  }

  [HierarchyRoot]
  public class Root2 : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  public class Child2 : Root2, IChild
  {
    
  }
}

namespace Xtensive.Storage.Tests.Interfaces
{
  public class KeyStructureConflictTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Root1).Assembly, typeof (Root1).Namespace);
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