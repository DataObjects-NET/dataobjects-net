﻿// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.06

using System.Linq;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0416_NpgsqlLSegModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0416_NpgsqlLSegModel
  {
    [HierarchyRoot]
    public class EntityWithNpgsqlLSeg : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public NpgsqlLSeg LSeg { get; set; }

      [Field]
      public NpgsqlLSeg OtherLSeg { get; set; }
    }
  }

  [TestFixture]
  internal class IssueJira0416_NpgsqlLSeg : AutoBuildTest
  {
    private NpgsqlLSeg lSeg = new NpgsqlLSeg(new NpgsqlPoint(1, 1), new NpgsqlPoint(2, 2));
    private NpgsqlLSeg otherLSeg = new NpgsqlLSeg(new NpgsqlPoint(0, 0), new NpgsqlPoint(1, 1));

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNpgsqlLSeg));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new EntityWithNpgsqlLSeg {LSeg = lSeg, OtherLSeg = otherLSeg};
          t.Complete();
        }
      }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    #region Extractors

    [Test]
    public void ExtractStartPointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg.Start!=otherLSeg.Start);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractEndPointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg.End!=otherLSeg.End);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractPartOfStartPointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg.Start.X==lSeg.Start.X);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg.Start.Y==lSeg.Start.Y);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractPartOfEndPointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg.End.X==lSeg.End.X);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg.End.Y==lSeg.End.Y);

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

          var query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg==e.LSeg);

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

          var query = session.Query.All<EntityWithNpgsqlLSeg>()
            .Where(e => e.LSeg!=e.OtherLSeg);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    #endregion
  }
}
