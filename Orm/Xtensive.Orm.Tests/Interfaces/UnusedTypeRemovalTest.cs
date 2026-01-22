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
  public class UnusedTypeRemovalTest
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(IFirst).Assembly, typeof(IFirst).Namespace);

      var ex = Assert.Throws<DomainBuilderException>(() => Domain.Build(config));
      var message = ex.Message;
      Assert.That(message.Contains("ISecond") && message.Contains("don't belong") && message.Contains("hierarchy"),
        Is.True);
    }
  }
}