using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.NanosecondsInIntervalsModel;

namespace Xtensive.Orm.Tests.Issues.NanosecondsInIntervalsModel
{
  [HierarchyRoot]
  public class MaintenanceTimingRule : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public TimeSpan Periodicity { get; set; }

    [Field]
    public MaintenanceRuleType RuleType { get; set; }

  }

  public enum MaintenanceRuleType
  {
    Type0,
    Type1,
    Type2,
  }
}

namespace Xtensive.Orm.Tests.Issues
{


  public class NanosecondsInIntervals : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(MaintenanceTimingRule));
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type0,
          Periodicity = TimeSpan.FromTicks(6456764540)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type0,
          Periodicity = TimeSpan.FromTicks(878445678780)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type0,
          Periodicity = TimeSpan.FromTicks(9754487970)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type0,
          Periodicity = TimeSpan.FromTicks(78984560)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type0,
          Periodicity = TimeSpan.FromTicks(8798545490)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type1,
          Periodicity = TimeSpan.FromTicks(4854564890)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type1,
          Periodicity = TimeSpan.FromTicks(78797854560)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type1,
          Periodicity = TimeSpan.FromTicks(4854564890)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type1,
          Periodicity = TimeSpan.FromTicks(4854564890)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type2,
          Periodicity = TimeSpan.FromTicks(7985465475650)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type2,
          Periodicity = TimeSpan.FromTicks(51655465464570)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type2,
          Periodicity = TimeSpan.FromTicks(1215468654650)
        };

        _ = new MaintenanceTimingRule() {
          RuleType = MaintenanceRuleType.Type2,
          Periodicity = TimeSpan.FromTicks(13548945450)
        };

        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var xClient = session.Query.All<MaintenanceTimingRule>().AsEnumerable()
          .GroupBy(c => c.RuleType)
          .Select(c => new {
            Sum = TimeSpan.FromTicks(c.Sum(r => r.Periodicity.Ticks)),
            Avg = TimeSpan.FromMilliseconds(c.Average(r => r.Periodicity.TotalMilliseconds)),
            Rule = c.Key,
          })
          .ToDictionary(r => r.Rule);

        var xServer = session.Query.All<MaintenanceTimingRule>()
          .GroupBy(c => c.RuleType)
          .Select(c => new {
            Sum = TimeSpan.FromTicks(c.Sum(r => r.Periodicity.Ticks)),
            Avg = TimeSpan.FromMilliseconds(c.Average(r => r.Periodicity.TotalMilliseconds)),
            Rule = c.Key,
          })
          .ToDictionary(r => r.Rule);

        Assert.That(xServer.Count, Is.EqualTo(3));
      }
    }
  }
}
