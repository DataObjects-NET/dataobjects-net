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
    public int X
    {
      get { return GetValue<int>("X"); }
      set { SetValue("X", value); }
    }

    [Field]
    public int Y
    {
      get { return GetValue<int>("Y"); }
      set { SetValue("Y", value); }
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

  [HierarchyRoot(typeof (Generator), "ID")]
  public class  Ray : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field]
    public Point Vertex
    {
      get { return GetValue<Point>("Vertex"); }
      set { SetValue("Vertex", value); }
    }

    [Field]
    public Direction Direction
    {
      get { return GetValue<Direction>("Direction"); }
      set { SetValue("Direction", value); }
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

    [Test]
    public void RayTest()
    {
      using (Domain.OpenSession()) {
        Point p1 = new Point(1, 2);
        Ray ray1 = new Ray(p1);
        Assert.AreEqual(1, ray1.Vertex.X);
        Assert.AreEqual(2, ray1.Vertex.Y);
        ray1.Vertex.X = 10;
        Assert.AreEqual(10, ray1.Vertex.X);
        p1.X = 22;
        Assert.AreEqual(22, p1.X);
        Assert.AreEqual(10, ray1.Vertex.X);
        Point p2 = ray1.Vertex;
        Assert.AreEqual(10, p2.X);
        Assert.AreEqual(2, p2.Y);
        p2.X = 15;
        Assert.AreEqual(15, ray1.Vertex.X);
        Ray ray2 = new Ray();
        ray2.Vertex = ray1.Vertex;
        ray2.Vertex.X = 100;
        Assert.AreEqual(100, ray2.Vertex.X);
        Assert.AreEqual(15, ray1.Vertex.X);

        Point p3 = ray1.Vertex;
        Point p4 = ray1.Vertex;
        Assert.AreSame(p3, p4);
      }
    }
  }
}