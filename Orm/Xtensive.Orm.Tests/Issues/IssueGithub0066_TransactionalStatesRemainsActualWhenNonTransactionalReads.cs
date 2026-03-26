// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueGithub0066_TransactionalStatesRemainsActualWhenNonTransactionalReadsModel;

namespace Xtensive.Orm.Tests.Issues.IssueGithub0066_TransactionalStatesRemainsActualWhenNonTransactionalReadsModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public sealed class IssueGithub0066_TransactionalStatesRemainsActualWhenNonTransactionalReads : AutoBuildTest
  {
    private readonly SessionConfiguration clientProfileConfiguration = new SessionConfiguration(SessionOptions.ClientProfile);
    private readonly SessionConfiguration serverProfileConfiguration = new SessionConfiguration(SessionOptions.ServerProfile);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void ClientProfileCase1()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        var sessionToken = session.GetLifetimeToken();
        var sessionEntity = new TestEntity(session);
        Assert.That(sessionEntity.State.LifetimeToken, Is.EqualTo(sessionToken));
        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using(var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);
          Assert.That(sessionToken.IsActive, Is.True);
          Assert.That(sessionToken, Is.Not.EqualTo(txToken));

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.IsActual, Is.True);
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);

          outOfScopeEntity = txEntity;
          outOfScopeTxToken = txToken;
          tx.Complete();
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.True);

        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        Assert.That(outOfScopeEntity.State.IsActual, Is.True);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.True);
      }
    }

    [Test]
    public void ClientProfileCase2()
    {
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        var sessionToken = session.GetLifetimeToken();
        var sessionEntity = new TestEntity(session);
        Assert.That(sessionEntity.State.LifetimeToken, Is.EqualTo(sessionToken));
        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using (var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);
          Assert.That(sessionToken.IsActive, Is.True);
          Assert.That(sessionToken, Is.Not.EqualTo(txToken));

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);
          Assert.That(txEntity.State.IsActual, Is.True);

          outOfScopeEntity = txEntity;
          outOfScopeTxToken = txToken;
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.False);

        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        Assert.That(outOfScopeEntity.State.IsActual, Is.False);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.False);
      }
    }

    [Test]
    public void ClientProfileCase3()
    {
      TestEntity outOfSessionRef;
      TestEntity outOfSessionAndTxRef;
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        var sessionToken = session.GetLifetimeToken();
        var sessionEntity = new TestEntity(session);
        Assert.That(sessionEntity.State.LifetimeToken, Is.EqualTo(sessionToken));
        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using (var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);
          Assert.That(sessionToken.IsActive, Is.True);
          Assert.That(sessionToken, Is.Not.EqualTo(txToken));

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.IsActual, Is.True);
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);

          outOfScopeEntity = txEntity;
          outOfScopeTxToken = txToken;
          tx.Complete();
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.True);

        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        Assert.That(outOfScopeEntity.State.IsActual, Is.True);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.True);

        outOfSessionRef = sessionEntity;
        outOfSessionAndTxRef = outOfScopeEntity;
      }

      Assert.That(outOfSessionRef.State.IsActual, Is.False);
      Assert.That(outOfSessionRef.State.LifetimeToken.IsActive, Is.False);

      Assert.That(outOfSessionAndTxRef.State.IsActual, Is.False);
      Assert.That(outOfSessionAndTxRef.State.LifetimeToken.IsActive, Is.False);
    }

    [Test]
    public void ClientProfileCase4()
    {
      TestEntity outOfSessionRef;
      TestEntity outOfSessionAndTxRef;
      using (var session = Domain.OpenSession(clientProfileConfiguration)) {
        var sessionToken = session.GetLifetimeToken();
        var sessionEntity = new TestEntity(session);
        Assert.That(sessionEntity.State.LifetimeToken, Is.EqualTo(sessionToken));
        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using (var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);
          Assert.That(sessionToken.IsActive, Is.True);
          Assert.That(sessionToken, Is.Not.EqualTo(txToken));

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.IsActual, Is.True);
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);

          outOfScopeEntity = txEntity;
          outOfScopeTxToken = txToken;
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.False);

        Assert.That(sessionEntity.State.IsActual, Is.True);
        Assert.That(sessionEntity.State.LifetimeToken.IsActive, Is.True);

        Assert.That(outOfScopeEntity.State.IsActual, Is.False);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.False);

        outOfSessionRef = sessionEntity;
        outOfSessionAndTxRef = outOfScopeEntity;
      }

      Assert.That(outOfSessionRef.State.IsActual, Is.False);
      Assert.That(outOfSessionRef.State.LifetimeToken.IsActive, Is.False);
      
      Assert.That(outOfSessionAndTxRef.State.IsActual, Is.False);
      Assert.That(outOfSessionAndTxRef.State.LifetimeToken.IsActive, Is.False);
    }


    [Test]
    public void ServerProfileCase1()
    {
      using (var session = Domain.OpenSession(serverProfileConfiguration)) {

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using (var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.IsActual, Is.True);
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);

          outOfScopeEntity = txEntity;
          outOfScopeTxToken = txToken;
          tx.Complete();
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.False);

        Assert.That(outOfScopeEntity.State.IsActual, Is.False);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.False);
      }
    }

    [Test]
    public void ServerProfileCase2()
    {
      using (var session = Domain.OpenSession(serverProfileConfiguration)) {

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using (var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);
          Assert.That(txEntity.State.IsActual, Is.True);

          outOfScopeEntity = txEntity;
          outOfScopeTxToken = txToken;
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.False);

        Assert.That(outOfScopeEntity.State.IsActual, Is.False);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.False);
      }
    }

    [Test]
    public void ServerProfileCase3()
    {
      TestEntity outOfSessionAndTxRef;
      using (var session = Domain.OpenSession(serverProfileConfiguration)) {

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using (var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.IsActual, Is.True);
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);

          outOfScopeEntity = new TestEntity(session);
          outOfScopeTxToken = txToken;
          tx.Complete();
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.False);

        Assert.That(outOfScopeEntity.State.IsActual, Is.False);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.False);

        outOfSessionAndTxRef = outOfScopeEntity;
      }

      Assert.That(outOfSessionAndTxRef.State.IsActual, Is.False);
      Assert.That(outOfSessionAndTxRef.State.LifetimeToken.IsActive, Is.False);
    }

    [Test]
    public void ServerProfileCase4()
    {
      TestEntity outOfSessionAndTxRef;
      using (var session = Domain.OpenSession(serverProfileConfiguration)) {

        TestEntity outOfScopeEntity;
        StateLifetimeToken outOfScopeTxToken;
        using (var tx = session.OpenTransaction()) {
          var txToken = session.GetLifetimeToken();
          Assert.That(txToken.IsActive, Is.True);

          var txEntity = new TestEntity(session);
          Assert.That(txEntity.State.LifetimeToken, Is.EqualTo(txToken));
          Assert.That(txEntity.State.IsActual, Is.True);
          Assert.That(txEntity.State.LifetimeToken.IsActive, Is.True);

          outOfScopeEntity = new TestEntity(session);
          outOfScopeTxToken = txToken;
        }

        Assert.That(outOfScopeTxToken.IsActive, Is.False);

        Assert.That(outOfScopeEntity.State.IsActual, Is.False);
        Assert.That(outOfScopeEntity.State.LifetimeToken.IsActive, Is.False);

        outOfSessionAndTxRef = outOfScopeEntity;
      }

      Assert.That(outOfSessionAndTxRef.State.IsActual, Is.False);
      Assert.That(outOfSessionAndTxRef.State.LifetimeToken.IsActive, Is.False);
    }
  }
}
