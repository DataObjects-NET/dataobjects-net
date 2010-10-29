// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.UnexpectedBehaviorTestModel;

namespace Xtensive.Orm.Tests.Storage.UnexpectedBehaviorTestModel
{
  [Serializable]
  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
  public class UncreatableEntity : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    public UncreatableEntity(int id)
      : base(id)
    {
      throw new InvalidOperationException("Epic deferred success!");
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class UnexpectedBehaviorTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (UncreatableEntity).Assembly, typeof (UncreatableEntity).Namespace);
      return configuration;
    }

    [Test]
    public void ExceptionInCtorTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        try {
          new UncreatableEntity(42);
        }
        catch {
        }
        var wtf = session.Query.SingleOrDefault<UncreatableEntity>(42);
        Assert.IsNull(wtf);
      }
    }
  }
}