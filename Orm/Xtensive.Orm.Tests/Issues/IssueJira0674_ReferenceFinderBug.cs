using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0674_ReferenceFinderBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0674_ReferenceFinderBugModel
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
    public void RemoveUnsavedReferenceClientTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testA = new TestA(session) { Text = "A" };
        var testB = new TestB(session) { Text = "B", TestA = testA };

        testB.Remove();
        testA.Remove();
      }
    }

    [Test]
    public void RemoveUnsavedReferenceServerTest()
    {
      using (var session = Domain.OpenSession())
      using (var trasaction = session.OpenTransaction()) {
        var testA = new TestA(session) { Text = "A" };
        var testB = new TestB(session) { Text = "B", TestA = testA };

        testB.Remove();
        testA.Remove();
      }
    }

    [Test]
    public void RemoveSavedReferenceClientTest()
    {
      Key keyA2, keyB1, keyB2;
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile)))
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA(session) {Text = "A1"};
        keyA2 = new TestA(session) {Text = "A2"}.Key;
        keyB1 = new TestB(session) {Text = "B1", TestA = testA1}.Key;
        keyB2 = new TestB(session) {Text = "B2", TestA = testA1}.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var testA2 = session.Query.Single<TestA>(keyA2);
        var testB1 = session.Query.Single<TestB>(keyB1);
        var testB2 = session.Query.Single<TestB>(keyB2);
        testB2.Remove();
        var testA1 = testB1.TestA;
        testB1.TestA = testA2;
        // Exception with TestB2!
        testA1.Remove();
      }
    }

    [Test]
    public void RemoveSavedReferenceServerTest()
    {
      Key keyA2, keyB1, keyB2;
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile)))
      using (var transaction = session.OpenTransaction()) {
        var testA1 = new TestA(session) {Text = "A1"};
        keyA2 = new TestA(session) {Text = "A2"}.Key;
        keyB1 = new TestB(session) {Text = "B1", TestA = testA1}.Key;
        keyB2 = new TestB(session) {Text = "B2", TestA = testA1}.Key;
        transaction.Complete();
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile)))
      using (var transaction = session.OpenTransaction()) {
        var testA2 = session.Query.Single<TestA>(keyA2);
        var testB1 = session.Query.Single<TestB>(keyB1);
        var testB2 = session.Query.Single<TestB>(keyB2);
        testB2.Remove();
        var testA1 = testB1.TestA;
        testB1.TestA = testA2;
        testA1.Remove();
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
