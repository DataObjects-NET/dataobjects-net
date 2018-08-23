// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.14

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.Multinode
{
  public abstract class MultinodeTest
  {
    protected const string TestNodeId2 = "{4657EED5-AD12-4E3A-9324-BE570C897452}";
    protected const string TestNodeId3 = "{5854CFC3-B153-411C-8090-E0369034BDEF}";

    protected Domain Domain { get; private set; }

    protected virtual void CheckRequirements()
    {
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      return DomainConfigurationFactory.Create();
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }

    protected virtual void PopulateNodes()
    {
    }

    protected virtual void PopulateData()
    {
    }

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      CheckRequirements();
      var configuration = BuildConfiguration();
      Domain = BuildDomain(configuration);
      PopulateNodes();
      PopulateData();
    }

    [OneTimeTearDown]
    public void TestFixtureTearDown()
    {
      if (Domain!=null)
        Domain.Dispose();
    }
  }
}