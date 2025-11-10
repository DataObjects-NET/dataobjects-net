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
  public sealed class NonPairedEntitySetChangesTest : AutoBuildTest
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

    private const string ExceptionMessageIdentifier = "Events.Persisting";

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof(NonPairedEntitySetContainer));
      domainConfiguration.Types.Register(typeof(RefEntity));

      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var container = new NonPairedEntitySetContainer(session, None);
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 2 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 3 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 4 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 5 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 6 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 7 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 8 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 9 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 10 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 11 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 12 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 13 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 14 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 15 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 16 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 17 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 18 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 19 });
        _ = container.Refs.Add(new RefEntity(session, None) { Value = 20 });

        container = new NonPairedEntitySetContainer(session, ManualPersistOnPersisting);
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersisting) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersisting) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersisting) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, ManualPersistOnPersistingSystem);
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersistingSystem) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersistingSystem) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersistingSystem) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, ManualPersistOnPersisted);
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersisted) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersisted) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersisted) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, ManualPersistOnPersistedSystem);
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersistedSystem) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersistedSystem) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, ManualPersistOnPersistedSystem) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, PersistingOnCommitting);
        _ = container.Refs.Add(new RefEntity(session, PersistingOnCommitting) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, PersistingOnCommitting) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, PersistingOnCommitting) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, PersistingOnCommittingSystem);
        _ = container.Refs.Add(new RefEntity(session, PersistingOnCommittingSystem) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, PersistingOnCommittingSystem) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, PersistingOnCommittingSystem) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, PersistedOnCommitting);
        _ = container.Refs.Add(new RefEntity(session, PersistedOnCommitting) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, PersistedOnCommitting) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, PersistedOnCommitting) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, PersistedOnCommittingSystem);
        _ = container.Refs.Add(new RefEntity(session, PersistedOnCommittingSystem) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, PersistedOnCommittingSystem) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, PersistedOnCommittingSystem) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, TransactionOnPrecommitting);
        _ = container.Refs.Add(new RefEntity(session, TransactionOnPrecommitting) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnPrecommitting) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnPrecommitting) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, TransactionOnPrecommittingSystem);
        _ = container.Refs.Add(new RefEntity(session, TransactionOnPrecommittingSystem) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnPrecommittingSystem) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnPrecommittingSystem) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, TransactionOnCommitting);
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommitting) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommitting) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommitting) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, TransactionOnCommittingSystem);
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommittingSystem) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommittingSystem) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommittingSystem) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, TransactionOnCommitted);
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommitted) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommitted) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommitted) { Value = 2 });

        container = new NonPairedEntitySetContainer(session, TransactionOnCommittedSystem);
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommittedSystem) { Value = 0 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommittedSystem) { Value = 1 });
        _ = container.Refs.Add(new RefEntity(session, TransactionOnCommittedSystem) { Value = 2 });

        tx.Complete();
      }
    }

    [Test]
    public void ManualPersistOnPersistingEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
        var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        _ = none.Refs.Remove(entityToRemove);
        entityToRemove.Value = -1;

        session.Events.Persisting += ChangeEntityOnPersisting;
        session.SaveChanges();
        session.Events.Persisting -= ChangeEntityOnPersisting;

        Assert.That(session.EntitySetChangeRegistry.Count, Is.Zero);

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisting);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisting && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.False);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntitySetRegistryChange(eventAccessor.Session, ManualPersistOnPersisting);
      }
    }

    [Test]
    public void ManualPersistOnPersistingSystemEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
        var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        _ = none.Refs.Remove(entityToRemove);
        entityToRemove.Value = -1;

        session.SystemEvents.Persisting += ChangeEntityOnPersisting;
        session.SaveChanges();
        session.SystemEvents.Persisting -= ChangeEntityOnPersisting;

        Assert.That(session.EntitySetChangeRegistry.Count, Is.Zero);

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistingSystem);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistingSystem && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.False);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntitySetRegistryChange(eventAccessor.Session, ManualPersistOnPersistingSystem);
      }
    }

    [Test]
    public void ManualPersistOnPersistedEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
        var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        _ = none.Refs.Remove(entityToRemove);
        entityToRemove.Value = -1;

        session.Events.Persisted += ChangeEntityOnPersisted;
        session.SaveChanges();
        session.Events.Persisted -= ChangeEntityOnPersisted;
        // exception handled

      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisted);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisted && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, ManualPersistOnPersisted));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ManualPersistOnPersistedSystemEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
        var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        _ = none.Refs.Remove(entityToRemove);
        entityToRemove.Value = -1;

        session.SystemEvents.Persisted += ChangeEntityOnPersisted;
        session.SaveChanges();
        session.SystemEvents.Persisted -= ChangeEntityOnPersisted;
        // exception handled
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistedSystem);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistedSystem && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, ManualPersistOnPersistedSystem));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void PersistOnCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.Persisting += ChangeEntityOnPersisting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.Events.Persisting -= ChangeEntityOnPersisting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommitting);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommitting && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.False);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntitySetRegistryChange(eventAccessor.Session, PersistingOnCommitting);
      }
    }

    [Test]
    public void PersistOnCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.Persisting += ChangeEntityOnPersisting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.SystemEvents.Persisting -= ChangeEntityOnPersisting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommittingSystem);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommittingSystem && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.False);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntitySetRegistryChange(eventAccessor.Session, PersistingOnCommittingSystem);
      }
    }

    [Test(Description = "Data Loss")]
    public void ChangesOnPersistedEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.Persisted += ChangeEntityOnPersisted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.Events.Persisted -= ChangeEntityOnPersisted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommitting);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommitting && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, PersistedOnCommitting));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test(Description = "Data Loss")]
    public void ChangesOnPersistedSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.Persisted += ChangeEntityOnPersisted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.SystemEvents.Persisted -= ChangeEntityOnPersisted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommittingSystem);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommittingSystem && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, PersistedOnCommittingSystem));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ChangesOnTransactionPreCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionPrecommitting += ChangeEntityOnPrecommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.Events.TransactionPrecommitting -= ChangeEntityOnPrecommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommitting);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommitting && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.False);

        tx.Complete();
      }

      static void ChangeEntityOnPrecommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntitySetRegistryChange(eventAccessor.Session, TransactionOnPrecommitting);
      }
    }

    [Test]
    public void ChangesOnTransactionPreCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.TransactionPrecommitting += ChangeEntityOnPrecommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.SystemEvents.TransactionPrecommitting -= ChangeEntityOnPrecommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommittingSystem);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommittingSystem && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.False);

        tx.Complete();
      }

      static void ChangeEntityOnPrecommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeEntitySetRegistryChange(eventAccessor.Session, TransactionOnPrecommittingSystem);
      }
    }

    [Test(Description = "Data Loss")]
    public void ChangesOnTransactionCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitting += ChangeEntityOnCommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.Events.TransactionCommitting -= ChangeEntityOnCommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitting);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitting && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnCommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, TransactionOnCommitting));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test(Description = "Data Loss")]
    public void ChangesOnTransactionCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.TransactionCommitting += ChangeEntityOnCommitting;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.SystemEvents.TransactionCommitting -= ChangeEntityOnCommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittingSystem);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittingSystem && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnCommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, TransactionOnCommittingSystem));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test(Description = "Data Loss")]
    public void ChangesOnTransactionCommittedEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitted += ChangeEntityOnCommitted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.Events.TransactionCommitted -= ChangeEntityOnCommitted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitted);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitted && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnCommitted(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, TransactionOnCommitted));

        Assert.That(ex.Message.Contains("Active Transaction is required"));
      }
    }

    [Test(Description = "Data Loss")]
    public void ChangesOnTransactionCommittedSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitted += ChangeEntityOnCommitted;
        using (var tx = session.OpenTransaction()) {
          var none = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == None);
          var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          _ = none.Refs.Remove(entityToRemove);
          entityToRemove.Value = -1;

          tx.Complete();
        }
        session.Events.TransactionCommitted -= ChangeEntityOnCommitted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittedSystem);
        var removedEntity = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittedSystem && t.Value == 2);
        Assert.That(testEntity.Refs.Contains(removedEntity), Is.True);

        tx.Complete();
      }

      static void ChangeEntityOnCommitted(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeEntitySetRegistryChange(eventAccessor.Session, TransactionOnCommittedSystem));

        Assert.That(ex.Message.Contains("Active Transaction is required"));
      }
    }

    private static void MakeEntitySetRegistryChange(Session session, string identifier)
    {
      var testEntity = session.Query.All<NonPairedEntitySetContainer>().FirstOrDefault(t => t.TestIdentifier == identifier);
      var entityToRemove = session.Query.All<RefEntity>().FirstOrDefault(t => t.TestIdentifier == identifier && t.Value == 2);
      // registry change
      if (!testEntity.Refs.Remove(entityToRemove)) {
        throw new Exception("Something went wrong");
      }
    }
  }
}
