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
      config.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
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

          Assert.IsNotNull(c.CompositeObject);
          Assert.IsNotNull(c.CompositeObjectHandler);

          Assert.AreEqual(co.Name, c.CompositeObject.Name);
          Assert.AreEqual(co.Value, c.CompositeObject.Value);

          Assert.AreEqual(co.Name, c.CompositeObjectHandler.Name);
          Assert.AreEqual(co.Value, c.CompositeObjectHandler.Value);

          t.Complete();
        }
      }

      // Fetching
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var c = session.Query.All<Container>().First();

          Assert.IsNotNull(c.CompositeObject);
          Assert.IsNotNull(c.CompositeObjectHandler);

          var co = c.CompositeObject;

          Assert.AreEqual(co.Name, c.CompositeObject.Name);
          Assert.AreEqual(co.Value, c.CompositeObject.Value);

          Assert.AreEqual(co.Name, c.CompositeObjectHandler.Name);
          Assert.AreEqual(co.Value, c.CompositeObjectHandler.Value);

          // Rollback
        }
      }

      // Querying
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var c = session.Query.All<Container>().Where(i => i.CompositeObject.Name == "SomeName").FirstOrDefault();
          Assert.IsNotNull(c);
          Assert.AreEqual("SomeName", c.CompositeObject.Name);

          // Rollback
        }
      }
    }
  }
}