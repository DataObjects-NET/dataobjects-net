// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.08

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueA418_UnableToCompileModelWithPersistentInterface_Model;
using System.Linq;

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

  [Serializable]
  public class IssueA418_UnableToCompileModelWithPersistentInterface
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (IWithReference).Assembly, typeof (IWithReference).Namespace);
      var domain = Domain.Build(config);

      using (var session = Session.Open(domain))
      using (var t = Transaction.Open(session)) {
        new SomeWithReference {Reference = new Reference()};

        var result = Query.All<SomeWithReference>()
          .Prefetch(s => s.Reference)
          .ToList();

        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result.Single());
        Assert.IsNotNull(result.Single().Reference);

        t.Complete();
      }
    }
  }
}