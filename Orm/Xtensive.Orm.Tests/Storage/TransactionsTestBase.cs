// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.01.30

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public abstract class TransactionsTestBase : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (Hexagon).Assembly, typeof (Hexagon).Namespace);
      return configuration;
    }

    protected static void AssertStateIsValid(Entity entity)
    {
      Assert.That(CheckLifetime(entity), Is.True);
    }

    protected static void AssertStateIsInvalid(Entity entity)
    {
      Assert.That(CheckLifetime(entity), Is.False);
    }

    private static bool CheckLifetime(Entity entity)
    {
      var token = entity.State.LifetimeToken;
      return token!=null && token.IsActive;
    }
  }
}