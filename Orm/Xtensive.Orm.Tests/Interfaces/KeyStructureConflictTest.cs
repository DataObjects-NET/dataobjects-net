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

  [Serializable]
  [HierarchyRoot]
  public class Root1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  public class Child1 : Root1, IChild
  {
    
  }

  [Serializable]
  [HierarchyRoot]
  public class Root2 : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [Serializable]
  public class Child2 : Root2, IChild
  {
  }
}

namespace Xtensive.Orm.Tests.Interfaces
{
  public class KeyStructureConflictTest
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.RegisterCaching(typeof (Root1).Assembly, typeof (Root1).Namespace);
      var ex = Assert.Throws<DomainBuilderException>(() => Domain.Build(config));
      var message = ex.Message;
      Assert.That(message.Contains("IChild") && message.Contains("different key structure") && message.Contains("Root1 & Root2"),
        Is.True);
    }
  }
}