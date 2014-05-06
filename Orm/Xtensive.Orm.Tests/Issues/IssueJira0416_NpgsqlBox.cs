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
    private NpgsqlBox box = new NpgsqlBox(new NpgsqlPoint(0, 1), new NpgsqlPoint(2, 3));
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
      RunTests(e => e.Box.UpperRight!=boxOther.UpperRight);
    }

    [Test]
    public void ExtractLowerLeftPointTest()
    {
      RunTests(e => e.Box.LowerLeft!=boxOther.LowerLeft);
    }

    [Test]
    public void ExtractRightTest()
    {
      RunTests(p => p.Box.Right==box.Right);
    }

    [Test]
    public void ExtractTopTest()
    {
      RunTests(p => p.Box.Top==box.Top);
    }

    [Test]
    public void ExtractLeftTest()
    {
      RunTests(p => p.Box.Left==box.Left);
    }

    [Test]
    public void ExtractBottomTest()
    {
      RunTests(p => p.Box.Bottom==box.Bottom);
    }

    [Test]
    public void ExtractHeightTest()
    {
      RunTests(p => p.Box.Height==box.Height);
    }

    [Test]
    public void ExtractWidthTest()
    {
      RunTests(p => p.Box.Width==box.Width);
    }

    private void RunTests(Expression<Func<EntityWithNpgsqlBox, bool>> filter)
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction())
      {
        var count = session.Query.All<EntityWithNpgsqlBox>().Count(filter);
        Assert.IsNotNull(count);
        t.Complete();
      }
    }
  }
}
