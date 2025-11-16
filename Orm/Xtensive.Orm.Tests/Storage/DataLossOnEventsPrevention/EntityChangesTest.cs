// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DataLossOnEventsPrevention.EntityChangeDuringPersistTestModel;

namespace Xtensive.Orm.Tests.Storage.DataLossOnEventsPrevention
{
  [TestFixture]
  public sealed class EntityChangesTest : AutoBuildTest
  {
    private const string None = "None";
    private const string ManualPersistOnPersisting        = nameof(ManualPersistOnPersistingEventTest);
    private const string ManualPersistOnPersistingSystem  = nameof(ManualPersistOnPersistingSystemEventTest);
    private const string ManualPersistOnPersisted         = nameof(ManualPersistOnPersistedEventTest);
    private const string ManualPersistOnPersistedSystem   = nameof(ManualPersistOnPersistedSystemEventTest);
    private const string PersistingOnCommitting           = nameof(PersistOnCommittingEventTest);
    private const string PersistingOnCommittingSystem     = nameof(PersistOnCommittingSystemEventTest);
    private const string PersistedOnCommitting            = nameof(ChangesOnPersistedEventTest);
    private const string PersistedOnCommittingSystem      = nameof(ChangesOnPersistedSystemEventTest);
    private const string TransactionOnPrecommitting       = nameof(ChangesOnTransactionPreCommittingEventTest);
    private const string TransactionOnPrecommittingSystem = nameof(ChangesOnTransactionPreCommittingSystemEventTest);
    private const string TransactionOnCommitting          = nameof(ChangesOnTransactionCommittingEventTest);
    private const string TransactionOnCommittingSystem    = nameof(ChangesOnTransactionCommittingSystemEventTest);
    private const string TransactionOnCommitted           = nameof(ChangesOnTransactionCommittedEventTest);
    private const string TransactionOnCommittedSystem     = nameof(ChangesOnTransactionCommittedSystemEventTest);

    private const string ExceptionMessageIdentifier = "possibility of changes not saved";

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof(SimpleEntity));

      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new SimpleEntity(session, None) { Value = 0 };

        _ = new SimpleEntity(session, ManualPersistOnPersisting) { Value = 0 };
        _ = new SimpleEntity(session, ManualPersistOnPersistingSystem) { Value = 0 };

        _ = new SimpleEntity(session, ManualPersistOnPersisted) { Value = 0 };
        _ = new SimpleEntity(session, ManualPersistOnPersistedSystem) { Value = 0 };

        _ = new SimpleEntity(session, PersistingOnCommitting) { Value = 0 };
        _ = new SimpleEntity(session, PersistingOnCommittingSystem) { Value = 0 };

        _ = new SimpleEntity(session, PersistedOnCommitting) { Value = 0 };
        _ = new SimpleEntity(session, PersistedOnCommittingSystem) { Value = 0 };

        _ = new SimpleEntity(session, TransactionOnPrecommitting) { Value = 0 };
        _ = new SimpleEntity(session, TransactionOnPrecommittingSystem) { Value = 0 };

        _ = new SimpleEntity(session, TransactionOnCommitting) { Value = 0 };
        _ = new SimpleEntity(session, TransactionOnCommittingSystem) { Value = 0 };

        _ = new SimpleEntity(session, TransactionOnCommitted) { Value = 0 };
        _ = new SimpleEntity(session, TransactionOnCommittedSystem) { Value = 0 };

        tx.Complete();
      }
    }

    [Test]
    public void ManualPersistOnPersistingEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        none.Value += 1;

        session.Events.Persisting += ChangeEntityOnPersisting;
        session.SaveChanges();
        session.Events.Persisting -= ChangeEntityOnPersisting;

        Assert.That(session.EntityChangeRegistry.Count, Is.Zero);

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisting);
        Assert.That(testEntity.Value, Is.EqualTo(75));

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, ManualPersistOnPersisting, 75);
      }
    }

    [Test]
    public void ManualPersistOnPersistingSystemEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        none.Value += 1;

        session.SystemEvents.Persisting += ChangeEntityOnPersisting;
        session.SaveChanges();
        session.SystemEvents.Persisting -= ChangeEntityOnPersisting;

        Assert.That(session.EntityChangeRegistry.Count, Is.Zero);

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistingSystem);
        Assert.That(testEntity.Value, Is.EqualTo(74));

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, ManualPersistOnPersistingSystem, 74);
      }
    }

    [Test]
    public void ManualPersistOnPersistedEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        none.Value += 1;

        session.Events.Persisted += ChangeEntityOnPersisted;
        session.SaveChanges();
        session.Events.Persisted -= ChangeEntityOnPersisted;
        // exception handled
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisted);
        Assert.That(testEntity.Value, Is.Not.EqualTo(65));

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntityRegistryChange(eventAccessor.Session, ManualPersistOnPersisted, 65));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ManualPersistOnPersistedSystemEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        none.Value += 1;

        session.SystemEvents.Persisted += ChangeEntityOnPersisted;
        session.SaveChanges();
        session.SystemEvents.Persisted -= ChangeEntityOnPersisted;

        Assert.That(session.EntityChangeRegistry.Count, Is.Not.Zero);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistedSystem);
        Assert.That(testEntity.Value, Is.Not.EqualTo(64));

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, ManualPersistOnPersistedSystem, 64);
      }
    }

    [Test]
    public void PersistOnCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.Persisting += ChangeEntityOnPersisting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.Events.Persisting -= ChangeEntityOnPersisting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommitting);
        Assert.That(testEntity.Value, Is.EqualTo(55));
        
        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, PersistingOnCommitting, 55);
      }
    }

    [Test]
    public void PersistOnCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.Persisting += ChangeEntityOnPersisting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.SystemEvents.Persisting -= ChangeEntityOnPersisting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommittingSystem);
        Assert.That(testEntity.Value, Is.EqualTo(54));

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, PersistingOnCommittingSystem, 54);
      }
    }

    [Test]
    public void ChangesOnPersistedEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.Persisted += ChangeEntityOnPersisted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.Events.Persisted -= ChangeEntityOnPersisted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommitting);
        Assert.That(testEntity.Value, Is.Not.EqualTo(45));

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntityRegistryChange(eventAccessor.Session, PersistedOnCommitting, 45));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ChangesOnPersistedSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.Persisted += ChangeEntityOnPersisted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.SystemEvents.Persisted -= ChangeEntityOnPersisted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommittingSystem);
        Assert.That(testEntity.Value, Is.Not.EqualTo(44));

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, PersistedOnCommittingSystem, 44);
      }
    }

    [Test]
    public void ChangesOnTransactionPreCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionPrecommitting += ChangeEntityOnPrecommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.Events.TransactionPrecommitting -= ChangeEntityOnPrecommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommitting);
        Assert.That(testEntity.Value, Is.EqualTo(35));

        tx.Complete();
      }

      static void ChangeEntityOnPrecommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, TransactionOnPrecommitting, 35);
      }
    }

    [Test]
    public void ChangesOnTransactionPreCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.TransactionPrecommitting += ChangeEntityOnPrecommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.SystemEvents.TransactionPrecommitting -= ChangeEntityOnPrecommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommittingSystem);
        Assert.That(testEntity.Value, Is.EqualTo(34));

        tx.Complete();
      }

      static void ChangeEntityOnPrecommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, TransactionOnPrecommittingSystem, 34);
      }
    }

    [Test]
    public void ChangesOnTransactionCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitting += ChangeEntityOnCommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.Events.TransactionCommitting -= ChangeEntityOnCommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitting);
        Assert.That(testEntity.Value, Is.Not.EqualTo(25));

        tx.Complete();
      }

      static void ChangeEntityOnCommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntityRegistryChange(eventAccessor.Session, TransactionOnCommitting, 25));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ChangesOnTransactionCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.TransactionCommitting += ChangeEntityOnCommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.SystemEvents.TransactionCommitting -= ChangeEntityOnCommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittingSystem);
        Assert.That(testEntity.Value, Is.Not.EqualTo(24));

        tx.Complete();
      }

      static void ChangeEntityOnCommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntityRegistryChange(eventAccessor.Session, TransactionOnCommittingSystem, 24);
      }
    }

    [Test]
    public void ChangesOnTransactionCommittedEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitted += ChangeEntityOnCommitted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.Events.TransactionCommitted -= ChangeEntityOnCommitted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitted);
        Assert.That(testEntity.Value, Is.Not.EqualTo(15));

        tx.Complete();
      }

      static void ChangeEntityOnCommitted(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntityRegistryChange(eventAccessor.Session, TransactionOnCommitted, 15));

        Assert.That(ex.Message.Contains("Active Transaction is required"));
      }
    }

    [Test]
    public void ChangesOnTransactionCommittedSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitted += ChangeEntityOnCommitted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          none.Value += 1;

          tx.Complete();
        }
        session.Events.TransactionCommitted -= ChangeEntityOnCommitted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittedSystem);
        Assert.That(testEntity.Value, Is.Not.EqualTo(14));

        tx.Complete();
      }

      static void ChangeEntityOnCommitted(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntityRegistryChange(eventAccessor.Session, TransactionOnCommittedSystem, 14));

        Assert.That(ex.Message.Contains("Active Transaction is required"));
      }
    }

    private static void MakeEntityRegistryChange(Session session, string identifier, int newValue)
    {
      var testEntity = session.Query.All<SimpleEntity>().FirstOrDefault(t => t.TestIdentifier == identifier);
      testEntity.Value = newValue; // registry change
    }
  }
}
