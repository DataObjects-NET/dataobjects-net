// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.12

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0188_ModelBuilderError_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0188_ModelBuilderError_Model
{
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    
    [Field]
    public B B { get; set; }
  }

  public class B : A
  {
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0188_ModelBuilderError
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
      config.Types.Register(typeof (A));
      config.Types.Register(typeof (B));
      return config;
    }
  }
}