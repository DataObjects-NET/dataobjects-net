// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Model.ReferenceTestModel
{
  #region Cyclic referenced structures

  [Serializable]
  public class S1 : Structure
  {
    [Field]
    public S2 Value { get; set; }
  }

  [Serializable]
  public class S2 : Structure
  {
    [Field]
    public S3 Value { get; set; }
  }

  [Serializable]
  public class S3 : Structure
  {
    [Field]
    public S1 Value { get; set; }
  }

  #endregion

  #region Cyclic contained structures with inheritance

  [Serializable]
  public class Parent : Structure
  {
    [Field]
    public Child Value { get; set; }
  }

  [Serializable]
  public class Child : Parent
  {
  }

  #endregion
}

namespace Xtensive.Orm.Tests.Model
{
  public class CyclicContainedStructureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Model.ReferenceTestModel");
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