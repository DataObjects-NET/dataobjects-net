// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.05

using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0416_NpgsqlPointModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0416_NpgsqlPointModel
  {
    [HierarchyRoot]
    public class EntityWithNpgsqlPoint : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public NpgsqlPoint Point { get; set; }

      [Field]
      public NpgsqlPoint OtherPoint { get; set; }
    }
  }

  [TestFixture]
  internal class IssueJira0416_NpgsqlPoint : AutoBuildTest
  {
    private NpgsqlPoint point = new NpgsqlPoint(0, 1);
    private NpgsqlPoint otherPoint = new NpgsqlPoint(1, 0);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNpgsqlPoint));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new EntityWithNpgsqlPoint {Point = point, OtherPoint = otherPoint};
          t.Complete();
        }
      }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql, "Only PostgreSql supports the data type of NpgsqlPoint");
    }

    #region Extractors

    [Test]
    public void ExtractXPartTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPoint>()
            .Where(e => e.Point.X==point.X);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractYPartTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPoint>()
            .Where(e => e.Point.Y==point.Y);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    #endregion

    #region Operators

    [Test]
    public void EqualityTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPoint>()
            .Where(e => e.Point==e.Point);

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

          var query = session.Query.All<EntityWithNpgsqlPoint>()
            .Where(e => e.Point!=e.OtherPoint);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    #endregion
  }
}
