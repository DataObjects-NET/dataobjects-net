// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.09

using System;
using System.Drawing;
using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet;
using Xtensive.Orm.Tests.Storage.PostgreSqlSpatialTestModel;

namespace Xtensive.Orm.Tests.Storage.PostgreSqlSpatialTestModel
{
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Indexed = true)]
    public NpgsqlPoint Point { get; set; }

    [Field]
    public NpgsqlLSeg LSeg { get; set; }

    [Field(Indexed = true)]
    public NpgsqlBox Box { get; set; }

    [Field]
    public NpgsqlPath Path { get; set; }

    [Field(Indexed = true)]
    public NpgsqlPolygon Polygon { get; set; }

    [Field(Indexed = true)]
    public NpgsqlCircle Circle { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class PostgreSqlSpatialTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
      return config;
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    [Test]
    public void MainTest()
    {
      NpgsqlPoint point = new NpgsqlPoint(1, 2);
      NpgsqlLSeg lSeg = new NpgsqlLSeg(new NpgsqlPoint(1, 2), new NpgsqlPoint(3, 4));
      NpgsqlBox box = new NpgsqlBox(new NpgsqlPoint(1, 1), new NpgsqlPoint(-1, -1));
      NpgsqlPath path = new NpgsqlPath(new[] {new NpgsqlPoint(1, 2), new NpgsqlPoint(3, 4)});
      path.Open = true;
      NpgsqlPolygon polygon = new NpgsqlPolygon(new[] {new NpgsqlPoint(1, 2), new NpgsqlPoint(3, 4), new NpgsqlPoint(5, 6)});
      NpgsqlCircle circle = new NpgsqlCircle(new NpgsqlPoint(1, 2), 3);

      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
          new Container
          {
            Point = point,
            LSeg = lSeg,
            Box = box,
            Path = path,
            Polygon = polygon,
            Circle = circle
          };
          new Container
          {
            Point = new NpgsqlPoint(),
            LSeg = new NpgsqlLSeg(),
            Box = new NpgsqlBox(),
            Path = new NpgsqlPath(),
            Polygon = new NpgsqlPolygon(),
            Circle = new NpgsqlCircle()
          };
          t.Complete();
        }

        using (var t = Session.Current.OpenTransaction()) {
          var c = Query.All<Container>().First();

          Console.WriteLine("Point   - ({0},{1})", c.Point.X, c.Point.Y);
          Console.WriteLine("LSeg    - [({0},{1}),({2},{3})]", c.LSeg.Start.X, c.LSeg.Start.Y, c.LSeg.End.X, c.LSeg.End.Y);
          Console.WriteLine("Box     - ({0},{1}),({2},{3})", c.Box.UpperRight.X, c.Box.UpperRight.Y, c.Box.LowerLeft.X, c.Box.LowerLeft.Y);

          Console.Write("Path    - (");
          foreach (var points in c.Path)
            Console.Write("({0},{1})", points.X, points.Y);
          Console.WriteLine(")");

          Console.Write("Polygon - (");
          foreach (var points in c.Polygon)
            Console.Write("({0},{1})", points.X, points.Y);
          Console.WriteLine(")");
          Console.WriteLine("Circle  - <({0},{1}),{2}>", c.Circle.Center.X, c.Circle.Center.Y, c.Circle.Radius);


          Assert.IsTrue(c.Point.Equals(point));
          Assert.IsTrue(c.LSeg.Equals(lSeg));
          Assert.IsTrue(c.Box.Equals(box));
          Assert.IsTrue(c.Path.Equals(path));
          Assert.IsTrue(c.Polygon.Equals(polygon));
          Assert.IsTrue(c.Circle.Equals(circle));

          t.Complete();
        }
      }
    }
  }
}
