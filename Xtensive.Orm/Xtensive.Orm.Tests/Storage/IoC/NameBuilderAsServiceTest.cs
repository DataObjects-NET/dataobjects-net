// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.02

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.IoC.NameBuilderAsServiceTest_Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Orm.Tests.Storage.IoC.NameBuilderAsServiceTest_Model
{
  [HierarchyRoot]
  public class FakeClass : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IoC
{
  public class NameBuilderAsServiceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (FakeClass).Assembly, typeof (FakeClass).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var ns = Domain.Services.Get<NameBuilder>();
      Assert.IsNotNull(ns);
    }
  }
}