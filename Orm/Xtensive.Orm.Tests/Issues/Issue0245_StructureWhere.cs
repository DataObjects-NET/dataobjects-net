// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.06.25

using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0245_Model;
using Range = Xtensive.Orm.Tests.Issues.Issue0245_Model.Range;

namespace Xtensive.Orm.Tests.Issues.Issue0245_Model
{
  [Serializable]
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
  }
}

namespace Xtensive.Orm.Tests.Issues
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
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var points = session.Query.All<Range>().Select(r => r.Left).Where(p => p == new Point(10, 1));
        var list = points.ToList();
      }
    }

    [Test]
    public void Test2()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var filter = new Point(10, 1);
        var points = session.Query.All<Range>().Where(r => r.Left == filter).Select(r => r.Left);
        var list = points.ToList();
      }
    }
  }
}
