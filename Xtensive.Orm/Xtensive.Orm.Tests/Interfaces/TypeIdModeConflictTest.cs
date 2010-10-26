// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Interfaces.TypeIdModeConflictTestModel;

namespace Xtensive.Orm.Tests.Interfaces.TypeIdModeConflictTestModel
{
  public interface IRoot : IEntity
  {
  }

  [Serializable]
  [HierarchyRoot]
  public class Root1 : Entity, IRoot
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot(IncludeTypeId = true)]
  public class Root2 : Entity, IRoot
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Interfaces
{
  public class TypeIdModeConflictTest : AutoBuildTest
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