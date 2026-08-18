// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.24

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0351_NameBuilderProblem_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0351_NameBuilderProblem_Model
{
  [HierarchyRoot]
  public class Master : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Slave> Slaves { get; private set; }
  }

  [HierarchyRoot]
  public class Slave : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0351_NameBuilderProblem
  {
    [Test]
    public void DomainBuildTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrow(() => Domain.Build(configuration).Dispose());
    }

    [Test]
    public void DomainBuildAsyncTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrowAsync(async () => (await Domain.BuildAsync(configuration)).Dispose());
    }

    private static DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Master));
      config.Types.Register(typeof(Slave));
      config.NamingConvention.NamespacePolicy = NamespacePolicy.AsIs;
      return config;
    }
  }
}