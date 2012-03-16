// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.12.12

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.SelfContainedStructureModel;

namespace Xtensive.Orm.Tests.Model.SelfContainedStructureModel
{
  [Serializable]
  public class SelfContained : Structure
  {
    [Field]
    public SelfContained Value { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model
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
      catch (DomainBuilderException e) {
      }
      return domain;
    }
  }
}