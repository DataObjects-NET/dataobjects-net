// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.16

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Structures
{
  [Serializable]
  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    public Point() {}

    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Range : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Point Left { get; set; }

    [Field]
    public Point Right { get; set; }

    public Range(Session session)
      : base(session)
    {}
  }
}

namespace Xtensive.Orm.Manual.Structures
{
  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Range).Assembly, typeof (Range).Namespace);
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          // Example 1
          Range range = new Range(session);
          range.Left.X = 0;
          range.Left.Y = 0;

          // getting "range.Left" value
          Point point = range.Left;
          // "range.Left" & "point" are the same instance
          Assert.IsTrue(ReferenceEquals(point, range.Left));

          // Let's modify "point" instance. 
          // "range.Left" will be changed automatically
          point.X = 10;
          Assert.AreEqual(10, range.Left.X);

          // Example 2
          // Copy-on-assignment behavior
          range.Right = range.Left;
          // Instances are not equal although they have the same values
          Assert.IsFalse(ReferenceEquals(range.Right, range.Left));
          Assert.AreEqual(range.Right.X, range.Left.X);
          Assert.AreEqual(range.Right.Y, range.Left.Y);

          // Example 3
          var points = session.Query.All<Range>().Select(r => r.Left);
          // or
          var ranges = session.Query.All<Range>().Where(r => r.Left == new Point(0, 10));
        }
      }
    }
  }
}