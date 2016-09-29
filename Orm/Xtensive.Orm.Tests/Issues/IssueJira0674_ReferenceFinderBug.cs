using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueReferenceFinderBug;

namespace Xtensive.Orm.Tests.Issues.IssueReferenceFinderBug
{
  [Serializable]
  [HierarchyRoot]
  public class TestA : Entity
  {
    public TestA(Session session) : base(session) { }

    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class TestB : Entity
  {
    public TestB(Session session) : base(session) { }

    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public TestA TestA { get; set; }
  }

}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0674_ReferenceFinderBug : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testA = new TestA(session) { Text = "A" };
        var testB = new TestB(session) { Text = "B", TestA = testA };
        testB.Remove();
        // No problem with Server Session!
        // Exception: Referential integrity violation on attempt to remove ...
        testA.Remove();
      }
    }

    [Test]
    public void ServerTest()
    {
      using (var session = Domain.OpenSession())
      using (var trasaction = session.OpenTransaction()) {
        var testA = new TestA(session) { Text = "A" };
        var testB = new TestB(session) { Text = "B", TestA = testA };
        testB.Remove();
        // No problem with Server Session!
        // Exception: Referential integrity violation on attempt to remove ...
        testA.Remove();
      }
    }


    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestA).Assembly, typeof (TestA).Namespace);
      return configuration;
    }
  }
}
