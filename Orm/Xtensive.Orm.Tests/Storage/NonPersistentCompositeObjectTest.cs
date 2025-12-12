// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.10.13

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.NonPersistentCompositeObjectTestModel;

namespace Xtensive.Orm.Tests.Storage.NonPersistentCompositeObjectTestModel
{
  [HierarchyRoot]
  public class Container : Entity
  {
    private CompositeObject compositeObject;

    [Field, Key]
    public int Id { get; private set; }

    public CompositeObject CompositeObject
    {
      get {
        if (compositeObject == null)
          compositeObject = CompositeObjectHandler.Read();

        return compositeObject;
      }
      set {
        compositeObject = value;
        CompositeObjectHandler.Update(compositeObject);
      }
    }

    [Field]
    internal CompositeObjectHandler CompositeObjectHandler { get; set; }
  }

  public class CompositeObject
  {
    public string Name { get; set; }

    public double Value { get; set; }
  }

  internal class CompositeObjectHandler : Structure
  {
    [Field]
    internal string Name { get; set; }

    [Field]
    internal double Value { get; set; }

    internal CompositeObject Read()
    {
      return new CompositeObject
                     {
                       Name = Name,
                       Value = Value
                     };
    }

    internal void Update(CompositeObject origin)
    {
      Name = origin.Name;
      Value = origin.Value;
    }
  }

  [CompilerContainer(typeof (Expression))]
  internal static class CustomLinqCompilerContainer
  {
    [Compiler(typeof (Container), "CompositeObject", TargetKind.PropertyGet)]
    public static Expression CompositeObject(Expression containerExpression)
    {
      Expression<Func<Container, CompositeObjectHandler>> ex = container => container.CompositeObjectHandler;
      return ex.BindParameters(containerExpression);
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class NonPersistentCompositeObjectTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (Container).Assembly, typeof (Container).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      // Persisting
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var c = new Container();
          var co = new CompositeObject();
          co.Name = "SomeName";
          co.Value = 12d;
          c.CompositeObject = co;

          Assert.That(c.CompositeObject, Is.Not.Null);
          Assert.That(c.CompositeObjectHandler, Is.Not.Null);

          Assert.That(c.CompositeObject.Name, Is.EqualTo(co.Name));
          Assert.That(c.CompositeObject.Value, Is.EqualTo(co.Value));

          Assert.That(c.CompositeObjectHandler.Name, Is.EqualTo(co.Name));
          Assert.That(c.CompositeObjectHandler.Value, Is.EqualTo(co.Value));

          t.Complete();
        }
      }

      // Fetching
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var c = session.Query.All<Container>().First();

          Assert.That(c.CompositeObject, Is.Not.Null);
          Assert.That(c.CompositeObjectHandler, Is.Not.Null);

          var co = c.CompositeObject;

          Assert.That(c.CompositeObject.Name, Is.EqualTo(co.Name));
          Assert.That(c.CompositeObject.Value, Is.EqualTo(co.Value));

          Assert.That(c.CompositeObjectHandler.Name, Is.EqualTo(co.Name));
          Assert.That(c.CompositeObjectHandler.Value, Is.EqualTo(co.Value));

          // Rollback
        }
      }

      // Querying
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var c = session.Query.All<Container>().Where(i => i.CompositeObject.Name == "SomeName").FirstOrDefault();
          Assert.That(c, Is.Not.Null);
          Assert.That(c.CompositeObject.Name, Is.EqualTo("SomeName"));

          // Rollback
        }
      }
    }
  }
}