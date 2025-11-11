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
  public sealed class NonPairedReferenceChangesTest : AutoBuildTest
  {
    private const string None = "None";
    private const string ManualPersistOnPersisting = nameof(ManualPersistOnPersistingEventTest);
    private const string ManualPersistOnPersistingSystem = nameof(ManualPersistOnPersistingSystemEventTest);
    private const string ManualPersistOnPersisted = nameof(ManualPersistOnPersistedEventTest);
    private const string ManualPersistOnPersistedSystem = nameof(ManualPersistOnPersistedSystemEventTest);
    private const string PersistingOnCommitting = nameof(PersistOnCommittingEventTest);
    private const string PersistingOnCommittingSystem = nameof(PersistOnCommittingSystemEventTest);
    private const string PersistedOnCommitting = nameof(ChangesOnPersistedEventTest);
    private const string PersistedOnCommittingSystem = nameof(ChangesOnPersistedSystemEventTest);
    private const string TransactionOnPrecommitting = nameof(ChangesOnTransactionPreCommittingEventTest);
    private const string TransactionOnPrecommittingSystem = nameof(ChangesOnTransactionPreCommittingSystemEventTest);
    private const string TransactionOnCommitting = nameof(ChangesOnTransactionCommittingEventTest);
    private const string TransactionOnCommittingSystem = nameof(ChangesOnTransactionCommittingSystemEventTest);
    private const string TransactionOnCommitted = nameof(ChangesOnTransactionCommittedEventTest);
    private const string TransactionOnCommittedSystem = nameof(ChangesOnTransactionCommittedSystemEventTest);

    private const string ExceptionMessageIdentifier = "possibility of changes not saved";

    protected override DomainConfiguration BuildConfiguration()
    {
      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      domainConfiguration.Types.Register(typeof(NonPairedReferencingEntity));
      domainConfiguration.Types.Register(typeof(NonPairedReferencedEntity));

      return domainConfiguration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new NonPairedReferencingEntity(session, None) {
          Ref = new NonPairedReferencedEntity(session, None) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };
        _ = new NonPairedReferencedEntity(session, None) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, ManualPersistOnPersisting) {
          Ref = new NonPairedReferencedEntity(session, ManualPersistOnPersisting) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, ManualPersistOnPersisting) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, ManualPersistOnPersistingSystem) {
          Ref = new NonPairedReferencedEntity(session, ManualPersistOnPersistingSystem) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, ManualPersistOnPersistingSystem) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, ManualPersistOnPersisted) {
          Ref = new NonPairedReferencedEntity(session, ManualPersistOnPersisted) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, ManualPersistOnPersisted) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, ManualPersistOnPersistedSystem) {
          Ref = new NonPairedReferencedEntity(session, ManualPersistOnPersistedSystem) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, ManualPersistOnPersistedSystem) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, PersistingOnCommitting) {
          Ref = new NonPairedReferencedEntity(session, PersistingOnCommitting) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, PersistingOnCommitting) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, PersistingOnCommittingSystem) {
          Ref = new NonPairedReferencedEntity(session, PersistingOnCommittingSystem) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, PersistingOnCommittingSystem) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, PersistedOnCommitting) {
          Ref = new NonPairedReferencedEntity(session, PersistedOnCommitting) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, PersistedOnCommitting) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, PersistedOnCommittingSystem) {
          Ref = new NonPairedReferencedEntity(session, PersistedOnCommittingSystem) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, PersistedOnCommittingSystem) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, TransactionOnPrecommitting) {
          Ref = new NonPairedReferencedEntity(session, TransactionOnPrecommitting) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, TransactionOnPrecommitting) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, TransactionOnPrecommittingSystem) {
          Ref = new NonPairedReferencedEntity(session, TransactionOnPrecommittingSystem) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, TransactionOnPrecommittingSystem) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, TransactionOnCommitting) {
          Ref = new NonPairedReferencedEntity(session, TransactionOnCommitting) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, TransactionOnCommitting) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, TransactionOnCommittingSystem) {
          Ref = new NonPairedReferencedEntity(session, TransactionOnCommittingSystem) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, TransactionOnCommittingSystem) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, TransactionOnCommitted) {
          Ref = new NonPairedReferencedEntity(session, TransactionOnCommitted) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, TransactionOnCommitted) { Value = 1 };

        _ = new NonPairedReferencingEntity(session, TransactionOnCommittedSystem) {
          Ref = new NonPairedReferencedEntity(session, TransactionOnCommittedSystem) { Value = 0 }
        };
        _ = new NonPairedReferencedEntity(session, TransactionOnCommittedSystem) { Value = 1 };

        tx.Complete();
      }
    }

    [Test]
    public void ManualPersistOnPersistingEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        var oldRef = noneReferencing.Ref;
        var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        noneReferencing.Ref = newRef;
        newRef.Value = 0;

        session.Events.Persisting += ChangeEntityOnPersisting;
        session.SaveChanges();
        session.Events.Persisting -= ChangeEntityOnPersisting;

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisting);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Not.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, ManualPersistOnPersisting);
      }
    }

    [Test]
    public void ManualPersistOnPersistingSystemEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        var oldRef = noneReferencing.Ref;
        var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        noneReferencing.Ref = newRef;
        newRef.Value = 0;

        session.SystemEvents.Persisting += ChangeEntityOnPersisting;
        session.SaveChanges();
        session.SystemEvents.Persisting -= ChangeEntityOnPersisting;

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistingSystem);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Not.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, ManualPersistOnPersistingSystem);
      }
    }

    [Test]
    public void ManualPersistOnPersistedEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        var oldRef = noneReferencing.Ref;
        var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        noneReferencing.Ref = newRef;
        newRef.Value = 0;

        session.Events.Persisted += ChangeEntityOnPersisted;
        session.SaveChanges();
        session.Events.Persisted -= ChangeEntityOnPersisted;
        // exception handled
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersisted);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeNonPairedReferenceRegistryChange(eventAccessor.Session, ManualPersistOnPersisted));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ManualPersistOnPersistedSystemEventTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
        var oldRef = noneReferencing.Ref;
        var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
        noneReferencing.Ref = newRef;
        newRef.Value = 0;

        session.SystemEvents.Persisted += ChangeEntityOnPersisted;
        session.SaveChanges();
        session.SystemEvents.Persisted -= ChangeEntityOnPersisted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == ManualPersistOnPersistedSystem);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, ManualPersistOnPersistedSystem);
      }
    }

    [Test]
    public void PersistOnCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.Persisting += ChangeEntityOnPersisting;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.Events.Persisting -= ChangeEntityOnPersisting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommitting);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Not.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, PersistingOnCommitting);
      }
    }

    [Test]
    public void PersistOnCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.Persisting += ChangeEntityOnPersisting;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.SystemEvents.Persisting -= ChangeEntityOnPersisting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == PersistingOnCommittingSystem);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Not.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisting(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, PersistingOnCommittingSystem);
      }
    }

    [Test]
    public void ChangesOnPersistedEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.Persisted += ChangeEntityOnPersisted;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.Events.Persisted -= ChangeEntityOnPersisted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommitting);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeNonPairedReferenceRegistryChange(eventAccessor.Session, PersistedOnCommitting));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ChangesOnPersistedSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.Persisted += ChangeEntityOnPersisted;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.SystemEvents.Persisted -= ChangeEntityOnPersisted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == PersistedOnCommittingSystem);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPersisted(object sender, EventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, PersistedOnCommittingSystem);
      }
    }

    [Test]
    public void ChangesOnTransactionPreCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionPrecommitting += ChangeEntityOnPrecommitting;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.Events.TransactionPrecommitting -= ChangeEntityOnPrecommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommitting);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Not.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPrecommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, TransactionOnPrecommitting);
      }
    }

    [Test]
    public void ChangesOnTransactionPreCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.TransactionPrecommitting += ChangeEntityOnPrecommitting;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.SystemEvents.TransactionPrecommitting -= ChangeEntityOnPrecommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnPrecommittingSystem);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Not.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnPrecommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, TransactionOnPrecommittingSystem);
      }
    }

    [Test]
    public void ChangesOnTransactionCommittingEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitting += ChangeEntityOnCommitting;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.Events.TransactionCommitting -= ChangeEntityOnCommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitting);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnCommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeNonPairedReferenceRegistryChange(eventAccessor.Session, TransactionOnCommitting));
        Assert.That(ex.Message.Contains(ExceptionMessageIdentifier));
      }
    }

    [Test]
    public void ChangesOnTransactionCommittingSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.SystemEvents.TransactionCommitting += ChangeEntityOnCommitting;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.SystemEvents.TransactionCommitting -= ChangeEntityOnCommitting;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittingSystem);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnCommitting(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        MakeNonPairedReferenceRegistryChange(eventAccessor.Session, TransactionOnCommittingSystem);
      }
    }

    [Test]
    public void ChangesOnTransactionCommittedEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitted += ChangeEntityOnCommitted;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.Events.TransactionCommitted -= ChangeEntityOnCommitted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommitted);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnCommitted(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeNonPairedReferenceRegistryChange(eventAccessor.Session, TransactionOnCommitted));

        Assert.That(ex.Message.Contains("Active Transaction is required"));
      }
    }

    [Test]
    public void ChangesOnTransactionCommittedSystemEventTest()
    {
      using (var session = Domain.OpenSession()) {
        session.Events.TransactionCommitted += ChangeEntityOnCommitted;
        using (var tx = session.OpenTransaction()) {
          var noneReferencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == None);
          var oldRef = noneReferencing.Ref;
          var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == None && t.Value > 0);
          noneReferencing.Ref = newRef;
          newRef.Value = 0;

          tx.Complete();
        }
        session.Events.TransactionCommitted -= ChangeEntityOnCommitted;
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == TransactionOnCommittedSystem);
        var referenced = referencing.Ref;
        Assert.That(referenced.Value, Is.Zero);

        tx.Complete();
      }

      static void ChangeEntityOnCommitted(object sender, TransactionEventArgs e)
      {
        var eventAccessor = (SessionEventAccessor) sender;
        var ex = Assert.Throws<InvalidOperationException>(() =>
          MakeNonPairedReferenceRegistryChange(eventAccessor.Session, TransactionOnCommittedSystem));

        Assert.That(ex.Message.Contains("Active Transaction is required"));
      }
    }

    private static void MakeNonPairedReferenceRegistryChange(Session session, string identifier)
    {
      var referencing = session.Query.All<NonPairedReferencingEntity>().FirstOrDefault(t => t.TestIdentifier == identifier);
      var oldRef = referencing.Ref;
      var newRef = session.Query.All<NonPairedReferencedEntity>().FirstOrDefault(t => t.TestIdentifier == identifier && t.Value > 0);
      referencing.Ref = newRef;
    }
  }
}
