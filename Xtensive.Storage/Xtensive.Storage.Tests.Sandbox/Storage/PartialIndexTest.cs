// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.06

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Sandbox.Storage.PartialIndexTestModel;

namespace Xtensive.Storage.Tests.Sandbox.Storage.PartialIndexTestModel
{
  public class TestBase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot, Index("Field01", Filter = "Index")]
  public class Test01 : TestBase
  {
    public static Expression<Func<Test01, bool>> Index()
    {
      return test => test.Field01.GreaterThan("hello world");
    }

    [Field]
    public string Field01 { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Sandbox.Storage
{
  [TestFixture]
  public class PartialIndexTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      Require.AllFeaturesSupported(ProviderFeatures.PartialIndexes);
    }

    [TearDown]
    public void TearDown()
    {
      if (domain==null)
        return;
      try {
        domain.Dispose();
      }
      finally {
        domain = null;
      }
    }

    private void BuildDomain(params Type[] entities)
    {
      var config = DomainConfigurationFactory.Create();
      foreach (var entity in entities)
        config.Types.Register(entity);
      domain = Domain.Build(config);
    }

    [Test]
    public void Test01()
    {
      BuildDomain(typeof (Test01));
    }
  }
}