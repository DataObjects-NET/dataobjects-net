// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Interfaces.UnusedTypeRemovalTestModel;

namespace Xtensive.Storage.Tests.Interfaces.UnusedTypeRemovalTestModel
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

  // No [HierarchyRoot] here
  public class Third : Entity, IThird
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Interfaces
{
  public class UnusedTypeRemovalTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (IFirst).Assembly, typeof (IFirst).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Assert.IsFalse(Domain.Model.Types.Contains(typeof(IFirst)));
      Assert.IsFalse(Domain.Model.Types.Contains(typeof(ISecond)));
      Assert.IsFalse(Domain.Model.Types.Contains(typeof(IThird)));
      Assert.IsFalse(Domain.Model.Types.Contains(typeof(Third)));
    }
  }
}