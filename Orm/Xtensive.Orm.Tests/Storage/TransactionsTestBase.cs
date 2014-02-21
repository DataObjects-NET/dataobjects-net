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
      configuration.Types.Register(typeof (Hexagon).Assembly, typeof (Hexagon).Namespace);
      return configuration;
    }

    protected static void AssertStateIsValid(Entity entity)
    {
      Assert.IsTrue(CheckLifetime(entity));
    }

    protected static void AssertStateIsInvalid(Entity entity)
    {
      Assert.IsFalse(CheckLifetime(entity));
    }

    private static bool CheckLifetime(Entity entity)
    {
      var token = entity.State.LifetimeToken;
      return token!=null && token.IsActive;
    }
  }
}