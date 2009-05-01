// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.16

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Model.ReferenceTestModel
{
  #region Cyclic referenced structures

  public class S1 : Structure
  {
    [Field]
    public S2 Value { get; set; }
  }

  public class S2 : Structure
  {
    [Field]
    public S3 Value { get; set; }
  }

  public class S3 : Structure
  {
    [Field]
    public S1 Value { get; set; }
  }

  #endregion

  #region Cyclic contained structures with inheritance

  public class Parent : Structure
  {
    [Field]
    public Child Value { get; set; }
  }

  public class Child : Parent
  {
  }

  #endregion
}

namespace Xtensive.Storage.Tests.Model
{
  public class CyclicContainedStructureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.ReferenceTestModel");
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain = null;
      try {
        domain = Domain.Build(configuration);
      }
      catch (AggregateException e) {
        Assert.AreEqual(2, e.Exceptions.Count());
      }
      return domain;
    }
  }
}