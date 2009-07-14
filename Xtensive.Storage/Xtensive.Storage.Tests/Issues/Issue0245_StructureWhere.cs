// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.06.25

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0245_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0245_Model
{
  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    public Point() { }

    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }
  }

  [HierarchyRoot]
  public class Range : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Point Left { get; set; }

    [Field]
    public Point Right { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  [Serializable]
  public class Issue0245_StructureWhere : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Range).Namespace);
      return config;
    }


    [Test]
    public void Test1()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var points = Query<Range>.All.Select(r => r.Left).Where(p => p == new Point(10, 1));
        var list = points.ToList();
      }
    }

    [Test]
    public void Test2()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var filter = new Point(10, 1);
        var points = Query<Range>.All.Where(r => r.Left == filter).Select(r => r.Left);
        var list = points.ToList();
      }
    }
  }
}