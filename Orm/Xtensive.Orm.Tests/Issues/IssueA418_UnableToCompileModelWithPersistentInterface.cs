// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.08

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueA418_UnableToCompileModelWithPersistentInterface_Model;


namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueA418_UnableToCompileModelWithPersistentInterface_Model
  {
    public interface IWithReference : IEntity
    {
      [Association(OnTargetRemove = OnRemoveAction.Cascade)]
      [Field(Nullable = false)]
      Reference Reference { get; set; }
    }

    [HierarchyRoot]
    public class SomeWithReference : Entity, IWithReference
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field(NullableOnUpgrade = true, Nullable = false)]
      public Reference Reference { get; set; }
    }

    [HierarchyRoot]
    public class Reference : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }
  }

  public class IssueA418_UnableToCompileModelWithPersistentInterface : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(IWithReference).Assembly, typeof(IWithReference).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new SomeWithReference { Reference = new Reference() };

        var result = session.Query.All<SomeWithReference>()
          .Prefetch(s => s.Reference)
          .ToList();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.Single(), Is.Not.Null);
        Assert.That(result.Single().Reference, Is.Not.Null);

        t.Complete();
      }
    }
  }
}