// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2008.05.28

using System;
using NUnit.Framework;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.StructureModel;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Tests.Storage.StructureModel
{
  [Serializable]
  public class Point : Structure
  {
    [Field(Indexed = true)]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    protected override void OnSettingFieldValue(FieldInfo field, object value)
    {
      Log.Debug("Structure field setting. Field: {0}; Value: {1}", field.Name, value);
    }

    protected override void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
      Log.Debug("Structure field set. Field: {0}; Value: {1}", field.Name, newValue);
    }

    public Point()
    {
    }

    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }
  }

  [Serializable]
  [Index("Vertex.Y")]
  [HierarchyRoot]
  public class Ray : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public Point Vertex { get; set; }

    [Field]
    public Direction Direction { get; set; }

    protected override void OnSettingFieldValue(FieldInfo field, object value)
    {
      Log.Debug("Entity field setting. Field: {0}; Value: {1}", field.Name, value);
    }

    protected override void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
      Log.Debug("Entity field set. Field: {0}; Value: {1}", field.Name, newValue);
    }


    public Ray()
    {
    }

    public Ray(Point vertex)
      : this(vertex, Direction.Positive)
    {
    }

    public Ray(Point vertex, Direction direction)
    {
      Vertex = vertex;
      Direction = direction;
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class StructureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.StructureModel");
      return config;
    }

    [Test]
    public void PointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          Point p1 = new Point();
          p1.X = 1;
          p1.Y = 2;
          Assert.AreEqual(1, p1.X);
          Assert.AreEqual(2, p1.Y);

          Point p2 = new Point(p1.X, p1.Y);
          Assert.AreEqual(p1.X, p2.X);
          Assert.AreEqual(p1.Y, p2.Y);
          Assert.IsTrue(p1.Equals(p2));
        }
      }
    }

    [Test]
    public void NotificationTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var ray = new Ray();
        ray.Vertex = new Point(10,20);
        ray.Vertex.X = 30;
        ray.Vertex.Y = 40;
        t.Complete();
      }
    }

    [Test]
    public void TransactionalTest()
    {
      using (var session = Domain.OpenSession()) {
        Ray ray;
        using (var transactionScope = session.OpenTransaction()) {
          ray = new Ray { Vertex = new Point {X = 1, Y = 2}};
          transactionScope.Complete();
        }        
        using (var transactionScope = session.OpenTransaction()) {
          ray.Vertex.X = 3;
          transactionScope.Complete();
        }
        using (session.OpenTransaction()) {
          Assert.AreEqual(3, ray.Vertex.X);
        }
      }
    } 

    [Test]
    public void RayTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          // Creating new Ray entity from Point. Values should be copied from "p".
          Point p1 = new Point(1, 2);
          Ray ray1 = new Ray(p1);
          Assert.AreEqual(1, ray1.Vertex.X);
          Assert.AreEqual(2, ray1.Vertex.Y);

          // Updating values in both Ray & Point. Values should differ.
          ray1.Vertex.X = 10;
          Assert.AreEqual(10, ray1.Vertex.X);
          p1.X = 22;
          Assert.AreEqual(22, p1.X);
          Assert.AreEqual(10, ray1.Vertex.X);

          // Getting reference to ray1.Vertex. Values should be equal.
          Point p2 = ray1.Vertex;
          Assert.AreEqual(10, p2.X);
          Assert.AreEqual(2, p2.Y);
          p2.X = 15;
          Assert.AreEqual(15, ray1.Vertex.X);

          // Copying Vertex from one ray to another. Values will be be copied.
          Ray ray2 = new Ray();
          ray2.Vertex = ray1.Vertex;

          // Assigning new value to ray2.Vertex.X. ray1.Vertex.X will be unchanged.
          ray2.Vertex.X = 100;
          Assert.AreEqual(100, ray2.Vertex.X);
          Assert.AreEqual(15, ray1.Vertex.X);

          // Checking reference equality
          Point p3 = ray1.Vertex;
          Point p4 = ray1.Vertex;
          Assert.AreSame(p3, p4);
        }
      }
    }
  }
}