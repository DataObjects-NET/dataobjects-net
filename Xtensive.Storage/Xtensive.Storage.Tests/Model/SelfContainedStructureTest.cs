// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.SelfContainedStructureModel;

namespace Xtensive.Storage.Tests.Model.SelfContainedStructureModel
{
  public class SelfContained : Structure
  {
    [Field]
    public SelfContained Value { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  public class SelfContainedStructureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (SelfContained).Assembly, typeof (SelfContained).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain = null;
      try {
        domain = Domain.Build(configuration);
      }
      catch (AggregateException e) {
        Assert.AreEqual(1, e.Exceptions.Count);
      }
      return domain;
    }
  }
}