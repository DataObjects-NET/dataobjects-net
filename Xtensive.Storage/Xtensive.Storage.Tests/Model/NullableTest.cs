// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.19

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Model.CorrectNullableModel
{
  [HierarchyRoot(typeof(DefaultGenerator), "ID")]
  public class X : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field]
    public int? Age { get; set; }

    [Field(IsNullable = true)]
    public string Name { get; set; }

    [Field(IsNullable = true)]
    public byte[] Image { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model.IncorrectNullableModel
{
  [HierarchyRoot(typeof(DefaultGenerator), "ID")]
  public class X : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field(IsNullable = true)]
    public int Age { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  [TestFixture]
  public class NullableTest : AutoBuildTest
  {
    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      return null;
    }

    [Test]
    public void CorrectModelTest()
    {
      DomainConfiguration config = BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.CorrectNullableModel");
      base.BuildDomain(config);
    }

    [Test]
    [ExpectedException(typeof(AggregateException))]
    public void IncorrectModelTest()
    {
      DomainConfiguration config = BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Model.IncorrectNullableModel");
      base.BuildDomain(config);
    }
  }
}