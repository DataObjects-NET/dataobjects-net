// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System;
using System.Linq;
using System.Linq.Expressions;
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
    }
  }

  [TestFixture]
  internal class IssueJira0416_NpgsqlCircle : AutoBuildTest
  {
    private NpgsqlCircle circle = new NpgsqlCircle(new NpgsqlPoint(1, 1), 10);
    private NpgsqlCircle circleOther = new NpgsqlCircle(new NpgsqlPoint(0, 0), 11);

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
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    [Test]
    public void ExtractCenterPointTest()
    {
      RunTests(e => e.Circle.Center!=circleOther.Center);
    }

    [Test]
    public void ExtractRadiusTest()
    {
      RunTests(e => e.Circle.Radius==circle.Radius);
    }

    private void RunTests(Expression<Func<EntityWithNpgsqlCircle, bool>> filter)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var count = session.Query.All<EntityWithNpgsqlCircle>().Count(filter);
        Assert.IsNotNull(count);
        t.Complete();
      }
    }
  }
}
