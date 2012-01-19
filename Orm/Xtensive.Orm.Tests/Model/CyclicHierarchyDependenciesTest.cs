// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.CyclicHierarchyDependenciesModel;

namespace Xtensive.Orm.Tests.CyclicHierarchyDependenciesModel
{
  [Serializable]
  [HierarchyRoot]
  public class H1 : Entity
  {
    [Field, Key(0)]
    public H2 First { get; private set; }

    [Field, Key(1)]
    public H3 Second { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class H2 : Entity
  {
    [Field, Key(0)]
    public H1 First { get; private set; }

    [Field, Key(1)]
    public H3 Second { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class H3 : Entity
  {
    [Field, Key(0)]
    public H1 First { get; private set; }

    [Field, Key(1)]
    public H2 Second { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class CyclicHierarchyDependenciesTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (H1).Assembly, typeof (H1).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain = null;
      try {
        domain = Domain.Build(configuration);
        Assert.Fail("Epic");
      }
      catch (DomainBuilderException e) {
        Console.WriteLine(e);
      }
      return domain;
    }
  }
}