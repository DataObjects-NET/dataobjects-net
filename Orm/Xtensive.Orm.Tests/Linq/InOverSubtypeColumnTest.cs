// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.07

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.InOverSubtypeColumnTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace InOverSubtypeColumnTestModel
  {
    [HierarchyRoot]
    public class MainEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public RefBase Reference { get; set; }

      public string Info
      {
        get
        {
          var refObj = Reference as Ref;
          if (refObj==null)
            return null;
          return refObj.Info;
        }
      }
    }

    [HierarchyRoot]
    public class RefBase : Entity
    {
      [Key, Field]
      public long Id { get; set; }
    }

    public class Ref : RefBase
    {
      [Field]
      public string Info { get; set; }
    }

    [CompilerContainer(typeof (Expression))]
    public static class Container
    {
      [Compiler(typeof (MainEntity), "Info", TargetKind.PropertyGet)]
      public static Expression Info(Expression _this)
      {
        Expression<Func<MainEntity, string>> expr = e => (e.Reference as Ref).Info;
        return expr.BindParameters(_this);
      }
    }
  }

  public class InOverSubtypeColumnTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MainEntity).Assembly, typeof (MainEntity).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new MainEntity {Reference = new RefBase()};
        new MainEntity {Reference = new Ref()};
        new MainEntity {Reference = new Ref {Info = "Don't know"}};
        new MainEntity {Reference = new Ref {Info = "IDDQD"}};
        tx.Complete();
      }
    }

    [Test]
    public void CalculatedFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<MainEntity>()
          .Where(e => e.Info.In("IDDQD", "IDFKA"))
          .ToList();
        Assert.That(result.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void DirectExpressionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<MainEntity>()
          .Where(e => (e.Reference as Ref).Info.In("IDDQD", "IDFKA"))
          .ToList();
        Assert.That(result.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NullCheck1Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<MainEntity>()
          .Where(e => (e.Reference as Ref)==null)
          .ToList();
        Assert.That(result.Count, Is.EqualTo(1));
      }
    }

    [Test]
    public void NullCheck2Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<MainEntity>()
          .Where(e => (e.Reference as Ref).Info==null)
          .ToList();
        Assert.That(result.Count, Is.EqualTo(2));
      }
    }
  }
}