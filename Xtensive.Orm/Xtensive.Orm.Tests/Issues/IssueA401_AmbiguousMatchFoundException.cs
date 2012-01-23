// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.07

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueA401_AmbiguousMatchFoundException_Model;
using System.Linq;
using Xtensive.Orm.Validation;

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

  [Serializable]
  public class IssueA401_AmbiguousMatchFoundException
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(ConcreteEntity).Assembly, typeof(ConcreteEntity).Namespace);
      var domain = Domain.Build(config);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var concrete = new ConcreteEntity(new Some());

        var result = Query.All<ConcreteEntity>().ToList();
        Assert.AreEqual(1, result.Count);
        t.Complete();
      }
    }
  }
}