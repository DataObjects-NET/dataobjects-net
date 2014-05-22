// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0416_NpgsqlCircleModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0416_NpgsqlCircleModel
  {
    [HierarchyRoot]
    public class EntityWithNpgsqlCircle : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public NpgsqlCircle Circle { get; set; }

      [Field]
      public NpgsqlCircle OtherCircle { get; set; }
    }
  }

  [TestFixture]
  internal class IssueJira0416_NpgsqlCircle : AutoBuildTest
  {
    private NpgsqlCircle circle = new NpgsqlCircle(new NpgsqlPoint(1, 1), 10);
    private NpgsqlCircle otherCircle = new NpgsqlCircle(new NpgsqlPoint(0, 0), 11);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNpgsqlCircle));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new EntityWithNpgsqlCircle {Circle = circle};
          t.Complete();
        }
      }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql, "Only PostgreSql supports the data type of NpgsqlCircle");
    }

    #region Extractors

    [Test]
    public void ExtractCenterPointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlCircle>()
            .Where(e => e.Circle.Center!=otherCircle.Center);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractRadiusTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlCircle>()
            .Where(e => e.Circle.Radius==circle.Radius);

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

          var query = session.Query.All<EntityWithNpgsqlCircle>()
            .Where(e => e.Circle==e.Circle);

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

          var query = session.Query.All<EntityWithNpgsqlCircle>()
            .Where(e => e.Circle!=e.OtherCircle);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    #endregion
  }
}
