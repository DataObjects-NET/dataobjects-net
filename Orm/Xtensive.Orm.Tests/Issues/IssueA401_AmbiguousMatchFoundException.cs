// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.07

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Tests.Issues.IssueA401_AmbiguousMatchFoundException_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueA401_AmbiguousMatchFoundException_Model
  {
    [HierarchyRoot]
    public class ConcreteEntity : Entity 
    {
      [Field,Key]
      public long Id { get; private set; }
      [Field, NotNullConstraint]
      public AbstractEntity Foo { get; set; }

      public ConcreteEntity(AbstractEntity foo)
      {
        Foo = foo;
      }
    }

    [HierarchyRoot]
    public abstract class AbstractEntity : Entity 
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    public class Some : AbstractEntity
    {

    }
  }

  public class IssueA401_AmbiguousMatchFoundException : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(ConcreteEntity).Assembly, typeof(ConcreteEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var concrete = new ConcreteEntity(new Some());

        var result = session.Query.All<ConcreteEntity>().ToList();
        Assert.That(result.Count, Is.EqualTo(1));
        t.Complete();
      }
    }
  }
}