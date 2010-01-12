// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using System;
using NUnit.Framework;
using Xtensive.Core.Testing;
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

  [Serializable]
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