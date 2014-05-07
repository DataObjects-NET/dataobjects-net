// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.05.05

using System;
using System.Linq;
using System.Linq.Expressions;
using NpgsqlTypes;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0416_NpgsqlBoxModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0416_NpgsqlBoxModel
  {
    [HierarchyRoot]
    public class EntityWithNpgsqlBox : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public NpgsqlBox Box { get; set; }
    }
  }

  [TestFixture]
  internal class IssueJira0416_NpgsqlBox : AutoBuildTest
  {
    private NpgsqlBox box = new NpgsqlBox(new NpgsqlPoint(2, 3), new NpgsqlPoint(0, 1));
    private NpgsqlBox boxOther = new NpgsqlBox(new NpgsqlPoint(1, 2), new NpgsqlPoint(3, 4));

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNpgsqlBox));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new EntityWithNpgsqlBox {Box = box};
          t.Complete();
        }
      }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    [Test]
    public void ExtractUpperRightPointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.UpperRight!=boxOther.UpperRight);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractLowerLeftPointTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.LowerLeft!=boxOther.LowerLeft);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractRightTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.Right==box.Right);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractTopTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.Top==box.Top);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractLeftTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.Left==box.Left);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractBottomTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.Bottom==box.Bottom);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractHeightTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.Height==box.Height);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }

    [Test]
    public void ExtractWidthTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var query = session.Query.All<EntityWithNpgsqlBox>()
            .Where(e => e.Box.Width==box.Width);

          Assert.IsTrue(query.ToList().FirstOrDefault()!=null);

          t.Complete();
        }
      }
    }
  }
}
