using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0449_TimeSpanMinMaxValueModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0449_TimeSpanMinMaxValueModel
  {
    [HierarchyRoot]
    public class EntityWithTimeSpan : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public TimeSpan Value { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0449_TimeSpanMinMaxValue : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithTimeSpan));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var created = new EntityWithTimeSpan {Value = TimeSpan.MinValue};
        var fetched = session.Query.All<EntityWithTimeSpan>().Single(e => e.Value==TimeSpan.MinValue);
        Assert.That(fetched, Is.EqualTo(created));
        created = new EntityWithTimeSpan {Value = TimeSpan.MaxValue};
        fetched = session.Query.All<EntityWithTimeSpan>().Single(e => e.Value==TimeSpan.MaxValue);
        Assert.That(fetched, Is.EqualTo(created));
        tx.Complete();
      }
    }
  }
}