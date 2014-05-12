// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0416_NpgsqlPolygonModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0416_NpgsqlPolygonModel
  {
    [HierarchyRoot]
    public class EntityWithNpgsqlPolygon : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public NpgsqlPolygon Polygon { get; set; }

      [Field]
      public NpgsqlPolygon OtherPolygon { get; set; }
    }
  }

  [TestFixture]
  internal class IssueJira0416_NpgsqlPolygon : AutoBuildTest
  {
    private NpgsqlPolygon polygon = new NpgsqlPolygon(new[] { new NpgsqlPoint(0, 1), new NpgsqlPoint(2, 3), new NpgsqlPoint(4, 5), new NpgsqlPoint(6, 7) });
    private NpgsqlPolygon otherPolygon = new NpgsqlPolygon(new[] {new NpgsqlPoint(0, 1), new NpgsqlPoint(2, 3), new NpgsqlPoint(4, 5), new NpgsqlPoint(6, 8)});

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNpgsqlPolygon));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new EntityWithNpgsqlPolygon {Polygon = polygon, OtherPolygon = otherPolygon};
          t.Complete();
        }
      }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    [Test]
    public void CountTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPolygon>()
            .Where(e => e.Polygon.Count==polygon.Count);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ContainsTest()
    {
      NpgsqlPoint pointContains = new NpgsqlPoint(4, 5);
      NpgsqlPoint pointNotContains = new NpgsqlPoint(4, 6);

      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPolygon>()
            .Where(e => e.Polygon.Contains(pointContains));

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          query = session.Query.All<EntityWithNpgsqlPolygon>()
            .Where(e => e.Polygon.Contains(pointNotContains));
          Assert.IsTrue(query.ToList().FirstOrDefault()==null);

          t.Complete();
        }
      }
    }

    #region Operators

    [Test]
    public void EqualityTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPolygon>()
            .Where(e => e.Polygon==e.Polygon);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void InequalityTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPolygon>()
            .Where(e => e.Polygon!=e.OtherPolygon);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    #endregion
  }
}
