using System;
using System.Runtime;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0656_RemapRestoresRemovedReferencesModel;
using Xtensive.Tuples;

namespace Xtensive.Orm.Tests.Issues.IssueJira0656_RemapRestoresRemovedReferencesModel
{
  [Serializable]
  [HierarchyRoot]
  public class TestA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public TestB TestB { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class TestB : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0656_RemapRestoresRemovedReferences : AutoBuildTest
  {
    private readonly SessionConfiguration clientProfile = new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation);

    [Test]
    public void UnableToSaveLocalChanges()
    {
      using (var session = Domain.OpenSession(clientProfile)) {
        var testB = new TestB { Text = "B" };
        var testA = new TestA { Text = "A", TestB = testB };
        testB.Remove();
        var aTuple = testA.Tuple;
        var aKey = testA.Key;

        Assert.DoesNotThrow(session.SaveChanges);
        testA = session.Query.Single<TestA>(aKey);
        Assert.That(testA.TestB, Is.Null);
      }
    }

    [Test]
    public void WrongAssociationAfterSaveChanges()
    {
      Key keyA;
      using (var session = Domain.OpenSession(clientProfile)) {
        var testB = new TestB { Text = "B" };
        var testA = new TestA { Text = "A", TestB = testB };
        
        Assert.DoesNotThrow(session.SaveChanges);

        keyA = testA.Key;
        var newTestB = new TestB { Text = "B2" };
        testA.TestB = newTestB;
        testB.Remove();
        newTestB.Remove();
        Assert.DoesNotThrow(session.SaveChanges);
        Assert.That(testA.TestB, Is.Null);
      }
      using (var session = Domain.OpenSession(clientProfile)) {
        var testA = session.Query.Single<TestA>(keyA);
        Assert.That(testA.TestB, Is.Null);
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var confinguration = base.BuildConfiguration();
      confinguration.UpgradeMode = DomainUpgradeMode.Recreate;
      confinguration.Types.Register(typeof (TestA));
      return confinguration;
    }
  }
}
