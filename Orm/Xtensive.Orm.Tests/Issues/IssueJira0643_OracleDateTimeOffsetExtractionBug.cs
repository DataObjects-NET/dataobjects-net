// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.04.29

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0643_OracleDateTimeOffsetExtractionBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0643_OracleDateTimeOffsetExtractionBugModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public DateTime DateTimeField { get; set; }

    [Field]
    public DateTimeOffset DateTimeOffsetField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0643_OracleDateTimeOffsetExtractionBug : AutoBuildTest
  {
    private DateTime[] dates;
    private DateTimeOffset[] dateTimeOffsets;

    [Test]
    public void MainTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      Assert.DoesNotThrow(() => Domain = BuildDomain(configuration));
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.That(session.Query.All<TestEntity>().Count(), Is.EqualTo(5));
        foreach (var source in session.Query.All<TestEntity>()) {
          Assert.That(dates.Contains(source.DateTimeField));
          Assert.That(dateTimeOffsets.Contains(source.DateTimeOffsetField));
        }
      }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Oracle);
      Require.AllFeaturesSupported(ProviderFeatures.DateTimeOffset);
    }

    protected override void PopulateData()
    {
      dates = new DateTime[5];
      dateTimeOffsets = new DateTimeOffset[5];
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 5; i++) {
          new TestEntity {
            DateTimeField = dates[i] = DateTime.Now.AddHours(i),
            DateTimeOffsetField = dateTimeOffsets[i] = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(i))
          };
        }
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
