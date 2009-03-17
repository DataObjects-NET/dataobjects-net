// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.05.28

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.StructureModel;

namespace Xtensive.Storage.Tests.Storage.StructureModel
{
  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    public Point()
    {
    }

    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }
  }

  [HierarchyRoot(typeof(KeyGenerator), "ID")]
  public class Ray : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public Point Vertex { get; set; }

    [Field]
    public Core.Direction Direction { get; set; }

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

namespace Xtensive.Storage.Tests.Storage
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
      using (Domain.OpenSession()) {
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

    [Test ]
    public void TransactionalTest()
    {
      using (Domain.OpenSession()) {

        Ray ray;
          using (var transactionScope = Transaction.Open()) {
          ray = new Ray {Vertex = new Point {X = 1, Y = 2}};
          transactionScope.Complete();
        }

        using (var transactionScope = Transaction.Open()) {
          ray.Vertex.X = 3;
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void RayTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
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