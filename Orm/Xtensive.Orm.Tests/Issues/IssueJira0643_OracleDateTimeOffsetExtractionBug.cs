// Copyright (C) 2016-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    }

    protected override void PopulateData()
    {
      var baseDateTime = new DateTime(2020, 08, 18, 19, 20, 21, 22);
      var baseDateTimeOffset = new DateTimeOffset(baseDateTime, new TimeSpan(5, 0, 0));

      dates = new DateTime[5];
      dateTimeOffsets = new DateTimeOffset[5];
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 5; i++) {
          _ = new TestEntity {
            DateTimeField = dates[i] = baseDateTime.AddHours(i),
            DateTimeOffsetField = dateTimeOffsets[i] = baseDateTimeOffset.ToOffset(TimeSpan.FromHours(i))
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
