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
using Xtensive.Orm.Tests.Linq.InOverCalculatedFieldTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace InOverCalculatedFieldTestModel
  {
    [HierarchyRoot]
    public class EntityWithCalculatedField : Entity
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
      [Compiler(typeof (EntityWithCalculatedField), "Info", TargetKind.PropertyGet)]
      public static Expression Info(Expression _this)
      {
        Expression<Func<EntityWithCalculatedField, string>> expr = e => (e.Reference as Ref).Info;
        return expr.BindParameters(_this);
      }
    }
  }

  public class InOverCalculatedFieldTest : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithCalculatedField).Assembly, typeof (EntityWithCalculatedField).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        new EntityWithCalculatedField {Reference = new RefBase()};
        new EntityWithCalculatedField {Reference = new Ref {Info = "Don't know"}};
        new EntityWithCalculatedField {Reference = new Ref {Info = "IDDQD"}};

        var result = session.Query.All<EntityWithCalculatedField>()
          .Where(e => e.Info.In("IDDQD", "IDFKA"))
          .ToList();

        Assert.That(result.Count, Is.EqualTo(1));
      }
    }
  }
}