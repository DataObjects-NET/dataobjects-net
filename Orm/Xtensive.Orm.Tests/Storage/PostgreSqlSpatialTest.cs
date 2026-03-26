// Copyright (C) 2014-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.04.09

using System;
using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.PostgreSqlSpatialTestModel;

namespace Xtensive.Orm.Tests.Storage.PostgreSqlSpatialTestModel
{
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
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

    public Container(Session session) : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class IndexedPointContainer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Indexed = true)]
    public NpgsqlPoint Point { get; set; }


    public IndexedPointContainer(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class PostgreSqlSpatialTest : AutoBuildTest
  {
    private readonly NpgsqlPoint point = new NpgsqlPoint(1, 2);
    private readonly NpgsqlLSeg lSeg = new NpgsqlLSeg(new NpgsqlPoint(1, 2), new NpgsqlPoint(3, 4));
    private readonly NpgsqlBox box = new NpgsqlBox(new NpgsqlPoint(1, 1), new NpgsqlPoint(-1, -1));
    private readonly NpgsqlPath path = new NpgsqlPath(new[] { new NpgsqlPoint(1, 2), new NpgsqlPoint(3, 4) }) { Open = true };
    private readonly NpgsqlPolygon polygon = new NpgsqlPolygon(new[] { new NpgsqlPoint(1, 2), new NpgsqlPoint(3, 4), new NpgsqlPoint(5, 6) });
    private readonly NpgsqlCircle circle = new NpgsqlCircle(new NpgsqlPoint(1, 2), 3);


    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Container));
      if (StorageProviderInfo.Instance.CheckProviderVersionIsAtLeast(StorageProviderVersion.PostgreSql90)) {
        config.Types.Register(typeof(IndexedPointContainer));
      }
      return config;
    }

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {

        var defaultValueContainerId = 0;
        var customValueContainerId = 0;
        using (var tx = session.OpenTransaction()) {
          defaultValueContainerId = new Container(session) {
            Point = new NpgsqlPoint(),
            LSeg = new NpgsqlLSeg(),
            Box = new NpgsqlBox(),
            Path = new NpgsqlPath(),
            Polygon = new NpgsqlPolygon(),
            Circle = new NpgsqlCircle()
          }.Id;

          customValueContainerId = new Container(session) {
            Point = point,
            LSeg = lSeg,
            Box = box,
            Path = path,
            Polygon = polygon,
            Circle = circle
          }.Id;

          tx.Complete();
        }

        using (var tx = session.OpenTransaction()) {
          var record = session.Query.All<Container>().First(c => c.Id == defaultValueContainerId);

          Console.WriteLine("The record without the initial parameters:");
          OutputRecord(record);

          Assert.IsTrue(record.Point.Equals(new NpgsqlPoint()));
          Assert.IsTrue(record.LSeg.Equals(new NpgsqlLSeg()));
          Assert.IsTrue(record.Box.Equals(new NpgsqlBox()));
          Assert.IsTrue(record.Path.Equals(new NpgsqlPath(new[] {new NpgsqlPoint()})));
          Assert.IsTrue(record.Polygon.Equals(new NpgsqlPolygon(new[] {new NpgsqlPoint()})));
          Assert.IsTrue(record.Circle.Equals(new NpgsqlCircle()));

          record = session.Query.All<Container>().First(c => c.Id == customValueContainerId);

          Console.WriteLine("The record with the initial parameters:");
          OutputRecord(record);
          Assert.IsTrue(record.Point.Equals(point));
          Assert.IsTrue(record.LSeg.Equals(lSeg));
          Assert.IsTrue(record.Box.Equals(box));
          Assert.IsTrue(record.Path.Equals(path));
          Assert.IsTrue(record.Polygon.Equals(polygon));
          Assert.IsTrue(record.Circle.Equals(circle));

          tx.Complete();
        }
      }
    }

    [Test]
    public void AddtionalTest()
    {
      Require.ProviderVersionAtLeast(StorageProviderVersion.PostgreSql90);

      using (var session = Domain.OpenSession()) {

        var defaultValueContainerId = 0;
        var customValueContainerId = 0;
        using (var tx = session.OpenTransaction()) {
          defaultValueContainerId = new IndexedPointContainer(session) { Point = new NpgsqlPoint() }.Id;
          customValueContainerId = new IndexedPointContainer(session) { Point = point }.Id;
          tx.Complete();
        }

        using (var tx = session.OpenTransaction()) {
          var record = session.Query.All<IndexedPointContainer>().First(c => c.Id == defaultValueContainerId);

          Console.WriteLine("The record without the initial parameters:");
          OutputRecord(record);

          Assert.IsTrue(record.Point.Equals(new NpgsqlPoint()));

          record = session.Query.All<IndexedPointContainer>().First(c => c.Id == customValueContainerId);

          Console.WriteLine("The record with the initial parameters:");
          OutputRecord(record);
          Assert.IsTrue(record.Point.Equals(point));
          
          tx.Complete();
        }
      }
    }

    public void OutputRecord(Container record)
    {
      Console.WriteLine("Point   - ({0},{1})", record.Point.X, record.Point.Y);
      Console.WriteLine("LSeg    - [({0},{1}),({2},{3})]", record.LSeg.Start.X, record.LSeg.Start.Y, record.LSeg.End.X, record.LSeg.End.Y);
      Console.WriteLine("Box     - ({0},{1}),({2},{3})", record.Box.UpperRight.X, record.Box.UpperRight.Y, record.Box.LowerLeft.X, record.Box.LowerLeft.Y);

      Console.Write("Path    - (");
      foreach (var points in record.Path)
        Console.Write("({0},{1})", points.X, points.Y);
      Console.WriteLine(")");

      Console.Write("Polygon - (");
      foreach (var points in record.Polygon)
        Console.Write("({0},{1})", points.X, points.Y);
      Console.WriteLine(")");

      Console.WriteLine("Circle  - <({0},{1}),{2}>", record.Circle.Center.X, record.Circle.Center.Y, record.Circle.Radius);
    }

    public void OutputRecord(IndexedPointContainer record)
    {
      Console.WriteLine("Point   - ({0},{1})", record.Point.X, record.Point.Y);
    }
  }
}
