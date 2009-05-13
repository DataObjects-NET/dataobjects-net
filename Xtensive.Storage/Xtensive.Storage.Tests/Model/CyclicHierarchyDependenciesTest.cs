// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.CyclicHierarchyDependenciesModel;

namespace Xtensive.Storage.Tests.CyclicHierarchyDependenciesModel
{
  [HierarchyRoot("First", "Second")]
  public class H1 : Entity
  {
    [Field]
    public H2 First { get; private set; }

    [Field]
    public H3 Second { get; private set; }
  }

  [HierarchyRoot("First", "Second")]
  public class H2 : Entity
  {
    [Field]
    public H1 First { get; private set; }

    [Field]
    public H3 Second { get; private set; }
  }

  [HierarchyRoot("First", "Second")]
  public class H3 : Entity
  {
    [Field]
    public H1 First { get; private set; }

    [Field]
    public H2 Second { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Model
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
      }
      catch (DomainBuilderException e) {
      }
      return domain;
    }
  }
}