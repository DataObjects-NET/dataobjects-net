using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0612_ReferenceFinderBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0612_ReferenceFinderBugModel
{
  [HierarchyRoot]
  public class TestA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field, Association(PairTo = "TestA", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<TestB> TestBs { get; private set; }
  }

  [HierarchyRoot]
  public class TestB : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public TestA TestA { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class TestC : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    [Field]
    public string Text { get; set; }

    [Field]
    public string Text2 { get; set; }
    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public TestA TestA { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0612_NonPairedReferencesSearchBug : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation))) {
        var NewTestA = new TestA {Text = "TestA"};
        new TestB {TestA = NewTestA, Text = "TestB"};
        // Wrong ReferencingEntity "TestB" instead of "TestC" with Association "TestC-TestA-TestA"
        var references = ReferenceFinder.GetReferencesTo(NewTestA).ToList();
        // Exception
        Assert.DoesNotThrow(NewTestA.Remove);
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestA).Assembly, typeof (TestA).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
