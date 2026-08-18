// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.10

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Interfaces.KeyStructureConflictTestModel;

namespace Xtensive.Orm.Tests.Interfaces.KeyStructureConflictTestModel
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

namespace Xtensive.Orm.Tests.Interfaces
{
  public class KeyStructureConflictTest
  {
    [Test]
    public void DomainBuildTest()
    {
      var configuration = BuildConfiguration();
      _ = Assert.Throws<DomainBuilderException>(() => Domain.Build(configuration).Dispose());
    }

    [Test]
    public void DomainBuildAsyncTest()
    {
      var configuration = BuildConfiguration();
      _ = Assert.ThrowsAsync<DomainBuilderException>(async () => (await Domain.BuildAsync(configuration)).Dispose());
    }

    private static DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.RegisterCaching(typeof(Root1).Assembly, typeof(Root1).Namespace);
      return config;
    }
  }
}