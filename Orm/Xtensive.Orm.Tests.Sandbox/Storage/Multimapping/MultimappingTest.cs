// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.14

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  [TestFixture]
  public abstract class MultimappingTest
  {
    protected const string DefaultSchemaName = "dbo";


    protected virtual void CheckRequirements()
    {
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.DefaultSchema = DefaultSchemaName;
      return configuration;
    }

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      CheckRequirements();
    }
  }
}