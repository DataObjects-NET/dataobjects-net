// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.16

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  [TestFixture]
  public abstract class MultimappingTest
  {
    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      CheckRequirements();
    }

    [TestFixtureTearDown]
    public virtual void TestFixtureTearDown()
    {
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      return DomainConfigurationFactory.Create();
    }

    protected virtual void CheckRequirements()
    {
    }
  }
}