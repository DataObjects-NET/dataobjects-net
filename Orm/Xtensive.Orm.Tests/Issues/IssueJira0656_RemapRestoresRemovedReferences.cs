using System;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0656_RemapRestoresRemovedReferencesModel;
using Xtensive.Orm.Tests.Linq;
using Xtensive.Orm.Tests.Storage.Validation;
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

  [Serializable]
  [HierarchyRoot]
  public class TestBase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }
  }

  [Serializable]
  public class TestC : TestBase
  {
    [Field, Association(PairTo = "TestC", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<TestD> TestDs { get; private set; }
  }


  [Serializable]
  [HierarchyRoot]
  public class TestD : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public TestC TestC { get; set; }
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

    [Test]
    public void RemapReferencesToRemapedEntity()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate()) {
        var testA = new TestA { Text = "A" };
        var TestB = new TestB { Text = "B" };

        //Everything is OK. There should be two assignment.
        testA.TestB = TestB;
        testA.TestB = TestB;

        Assert.DoesNotThrow(session.SaveChanges);
        Assert.That(testA.TestB, Is.Not.Null);
      }
    }

    [Test]
    public void SetReferenceToNewObject()
    {
      int nullReferenceId;
      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()) {
        var testA = new TestA {Text = "Test A without Reference"};
        nullReferenceId = testA.Id;
        transaction.Complete();
      }

      int anotherTestBTemporaryId;
      int anotherTestBRealId;
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate()) {
        var nullReferenceEntity = session.Query.All<TestA>().FirstOrDefault(el => el.Id==nullReferenceId);
        Assert.That(nullReferenceEntity, Is.Not.Null);
        Assert.That(nullReferenceEntity.TestB, Is.Null);

        var newTestB = new TestB{Text = "Another test B"};
        Assert.That(newTestB.Id, Is.LessThan(0));
        Assert.That(newTestB.Key.IsTemporary(Domain), Is.True);

        anotherTestBTemporaryId = newTestB.Id;
        nullReferenceEntity.TestB = newTestB;

        Assert.DoesNotThrow(session.SaveChanges);
        Assert.That(nullReferenceEntity.TestB, Is.Not.Null);
        Assert.That(nullReferenceEntity.TestB.Key.IsTemporary(Domain), Is.False);
        anotherTestBRealId = nullReferenceEntity.TestB.Id;
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate()) {
        var testA = session.Query.All<TestA>().FirstOrDefault(el => el.Id==nullReferenceId);
        Assert.That(testA, Is.Not.Null);

        var testBTemporary = session.Query.All<TestB>().FirstOrDefault(el => el.Id==anotherTestBTemporaryId);
        Assert.That(testBTemporary, Is.Null);

        var testBReal = session.Query.All<TestB>().FirstOrDefault(el => el.Id==anotherTestBRealId);
        Assert.That(testBReal, Is.Not.Null);

        Assert.That(testA.TestB, Is.EqualTo(testBReal));
      }
    }

    [Test]
    public void SetSyncObjectAsReferenceFormNewObject()
    {
      int nullReferenceId, referencingId, referencedId;
      using (var populateSession = Domain.OpenSession())
      using (var transaction = populateSession.OpenTransaction()){
        var testB = new TestB { Text = "Test B" };
        referencedId = testB.Id;
        transaction.Complete();
      }

      int temporaryId, realId;
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate()){
        var testB = session.Query.All<TestB>().FirstOrDefault(el => el.Id==referencedId);
        Assert.That(testB, Is.Not.Null);

        var testA = new TestA();
        temporaryId = testA.Id;
        testA.TestB = testB;
        Assert.That(testA.Key.IsTemporary(Domain), Is.True);
        Assert.DoesNotThrow(session.SaveChanges);

        Assert.That(testA.Key.IsTemporary(Domain), Is.False);
        realId = testA.Id;
      }

      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate()) {
        var testB = session.Query.All<TestB>().FirstOrDefault(el => el.Id==referencedId);
        Assert.That(testB, Is.Not.Null);

        var testATemporary = session.Query.All<TestA>().FirstOrDefault(el => el.Id==temporaryId);
        Assert.That(testATemporary, Is.Null);

        var testAReal = session.Query.All<TestA>().FirstOrDefault(el => el.Id==realId);
        Assert.That(testAReal, Is.Not.Null);
        Assert.That(testAReal.TestB, Is.Not.Null);
        Assert.That(testAReal.TestB.Id, Is.EqualTo(testB.Id));
      }
    }

    [Test]
    public void BaseTypeAccuracyReferencedKeyTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile)))
      using (session.Activate()) {
        var testC = new TestC { Text = "C" };
        session.SaveChanges();
        var testB = new TestD { Text = "B", TestC = testC };
        Assert.DoesNotThrow(session.SaveChanges);
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var confinguration = base.BuildConfiguration();
      confinguration.UpgradeMode = DomainUpgradeMode.Recreate;
      confinguration.Types.Register(typeof (TestA).Assembly, typeof (TestA).Namespace);
      return confinguration;
    }
  }
}
