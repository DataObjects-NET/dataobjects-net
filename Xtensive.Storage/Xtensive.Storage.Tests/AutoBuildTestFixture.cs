// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public abstract class AutoBuildTestFixture
  {
    private Domain domain;

    protected Domain Domain
    {
      get { return domain; }
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      DomainConfiguration config = BuildConfiguration();
      domain = BuildDomain(config);
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = new DomainConfiguration();
      config.ConnectionInfo = new UrlInfo(@"memory://localhost/Test_4.0");
      return config;
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }
  }
}