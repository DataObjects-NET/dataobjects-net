// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0416_NpgsqlPathModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0416_NpgsqlPathModel
  {
    [HierarchyRoot]
    public class EntityWithNpgsqlPath : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public NpgsqlPath Path { get; set; }
    }
  }

  [TestFixture]
  internal class IssueJira0416_NpgsqlPath : AutoBuildTest
  {
    private NpgsqlPath path = new NpgsqlPath(new[] {new NpgsqlPoint(0, 1), new NpgsqlPoint(2, 3), new NpgsqlPoint(4, 5), new NpgsqlPoint(6, 7)}) {Open = true};

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNpgsqlPath));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new EntityWithNpgsqlPath {Path = path};
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

          var query = session.Query.All<EntityWithNpgsqlPath>()
            .Where(e => e.Path.Count==path.Count);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void OpenTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlPath>()
            .Where(e => e.Path.Open==path.Open);

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

          var query = session.Query.All<EntityWithNpgsqlPath>()
            .Where(e => e.Path.Contains(pointContains));

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          query = session.Query.All<EntityWithNpgsqlPath>()
            .Where(e => e.Path.Contains(pointNotContains));
          Assert.IsTrue(query.ToList().FirstOrDefault()==null);

          t.Complete();
        }
      }
    }
  }
}
