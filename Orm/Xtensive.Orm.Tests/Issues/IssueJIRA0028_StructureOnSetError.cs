// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.04

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJIRA0028_StructureOnSetError_Model;

namespace Xtensive.Orm.Tests.Issues.IssueJIRA0028_StructureOnSetError_Model
{
  [HierarchyRoot]
  public class Interval : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Point Left { get; set; }

    [Field]
    public Point Right { get; set; }

    protected override void OnSetFieldValue(Orm.Model.FieldInfo field, object oldValue, object newValue)
    {
      base.OnSetFieldValue(field, oldValue, newValue);
      if (field.Name == "Left" || field.Name == "Right") {
        Point o, n;
        o = oldValue as Point;
        n = newValue as Point;
        if (o == null || n == null)
          return;
        Assert.AreNotEqual(o.X, n.X);
        Assert.AreNotEqual(o.Y, n.Y);
      }
    }
  }

  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Sandbox.Issues
{
  public class IssueJIRA0028_StructureOnSetError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Interval).Assembly, typeof (Interval).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var interval = new Interval();
          var point = new Point {X = 10, Y = 15};
          interval.Left = point;
          // Rollback
        }
      }
    }
  }
}