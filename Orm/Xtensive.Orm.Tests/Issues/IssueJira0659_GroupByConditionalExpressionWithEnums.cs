using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0659_GroupByConditionalExpressionWithEnumsModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0659_GroupByConditionalExpressionWithEnumsModel
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field]
    [Key]
    public Guid Id { get; set; }

    [Field]
    public Gender? Gender { get; set; }
  }

  public enum Gender
  {
    None = 0,
    Male = 1,
    Female = 2
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0659_GroupByConditionalExpressionWithEnums : AutoBuildTest
  {
    [Test]
    public void GroupByConditionalOperatorTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var genderGroups = session.Query.All<MyEntity>()
          
          .GroupBy(x => x.Gender.HasValue ? x.Gender.Value : Gender.None)
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g=>g.Key==Gender.None));
        Assert.That(genderGroups.Any(g=>g.Key==Gender.Female));
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new MyEntity();
        new MyEntity { Gender = Gender.Female };
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration() {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(MyEntity).Assembly, typeof(MyEntity).Namespace);
      return configuration;
    }
  }
}
