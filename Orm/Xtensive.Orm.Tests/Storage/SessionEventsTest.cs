// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.10.08

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.SessionEventsTestModel;
using System.Threading.Tasks;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage.SessionEventsTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class MegaEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; set; }
  }

  public class TestException : Exception { }

  public class EventInfo : SessionBound, IDisposable
  {
    private readonly bool throwExceptionOnCommit;

    public TransactionEventArgs? TransactionOpenArgs;

    public TransactionEventArgs? TransactionPrecommitingArgs;
    public TransactionEventArgs? TransactionCommitingArgs;
    public TransactionEventArgs? TransactionCommitedArgs;

    public TransactionEventArgs? TransactionRollbackingArgs;
    public TransactionEventArgs? TransactionRollbackedArgs;

    public EventArgs PersistingArgs;
    public EventArgs PersistedArgs;

    public EventArgs ChangesCancelingArgs;
    public EventArgs ChangesCanceledArgs;

    public EntityEventArgs EntityCreatedArgs;
    public EntityEventArgs EntityRemovingArgs;
    public EntityEventArgs EntityRemoveArgs;

    public EntityFieldEventArgs EntityFieldGettingArgs;
    public EntityFieldValueEventArgs EntityFieldValueGetArgs;
    public EntityFieldValueEventArgs EntityFieldValueSettingArgs;
    public EntityFieldValueSetEventArgs EntityFieldValueSetArgs;

    public QueryEventArgs QueryExecutingArgs;
    public QueryEventArgs QueryExecutedArgs;

    public DbCommandEventArgs? DbCommandExecutingArgs;
    public DbCommandEventArgs? DbCommandExecutedArgs;

    public void ResetEventArgs()
    {
      TransactionOpenArgs = null;
      TransactionPrecommitingArgs = null;
      TransactionCommitingArgs = null;
      TransactionCommitedArgs = null;

      TransactionRollbackingArgs = null;
      TransactionRollbackedArgs = null;

      PersistingArgs = null;
      PersistedArgs = null;

      ChangesCancelingArgs = null;
      ChangesCanceledArgs = null;

      EntityCreatedArgs = null;
      EntityRemovingArgs = null;
      EntityRemoveArgs = null;

      EntityFieldGettingArgs = null;
      EntityFieldValueGetArgs = null;
      EntityFieldValueSettingArgs = null;
      EntityFieldValueSetArgs = null;

      QueryExecutingArgs = null;
      QueryExecutedArgs = null;

      DbCommandExecutingArgs = null;
      DbCommandExecutedArgs = null;
    }

    private void OnTransactionCommiting(object sender, TransactionEventArgs e)
    {
      TransactionCommitingArgs = e;
      if (throwExceptionOnCommit) { throw new TestException(); }
    }

    private void OnTransactionOpening(object sender, TransactionEventArgs e) => TransactionOpenArgs = e;
    private void OnTransactionPrecommiting(object sender, TransactionEventArgs e) => TransactionPrecommitingArgs = e;
    private void OnTransactionCommited(object sender, TransactionEventArgs e) => TransactionCommitedArgs = e;
    private void OnTransactionRollbacking(object sender, TransactionEventArgs e) => TransactionRollbackingArgs = e;
    private void OnTransactionRollbacked(object sender, TransactionEventArgs e) => TransactionRollbackedArgs = e;
    private void OnPersisting(object sender, EventArgs e) => PersistingArgs = e;
    private void OnPersisted(object sender, EventArgs e) => PersistedArgs = e;
    private void OnChangesCanceling(object sender, EventArgs e) => ChangesCancelingArgs = e;
    private void OnChangesCanceled(object sender, EventArgs e) => ChangesCanceledArgs = e;
    private void OnEntityCreated(object sender, EntityEventArgs e) => EntityCreatedArgs = e;
    private void OnEntityRemoving(object sender, EntityEventArgs e) => EntityRemovingArgs = e;
    private void OnEntityRemove(object sender, EntityEventArgs e) => EntityRemoveArgs = e;
    private void OnEntityFieldValueGetting(object sender, EntityFieldEventArgs e) => EntityFieldGettingArgs = e;
    private void OnEntityFieldValueGet(object sender, EntityFieldValueEventArgs e) => EntityFieldValueGetArgs = e;
    private void OnEntityFieldValueSetting(object sender, EntityFieldValueEventArgs e) => EntityFieldValueSettingArgs = e;
    private void OnEntityFieldValueSet(object sender, EntityFieldValueSetEventArgs e) => EntityFieldValueSetArgs = e;
    private void OnQueryExecuting(object sender, QueryEventArgs e) => QueryExecutingArgs = e;
    private void OnQueryExecuted(object sender, QueryEventArgs e) => QueryExecutedArgs = e;
    private void OnDbCommandExecuting(object sender, DbCommandEventArgs e) => DbCommandExecutingArgs = e;
    private void OnDbCommandExecuted(object sender, DbCommandEventArgs e) => DbCommandExecutedArgs = e;

    public void Dispose()
    {
      Session.Events.TransactionOpening -= OnTransactionOpening;

      Session.Events.TransactionPrecommitting -= OnTransactionPrecommiting;
      Session.Events.TransactionCommitting -= OnTransactionCommiting;
      Session.Events.TransactionCommitted -= OnTransactionCommited;

      Session.Events.TransactionRollbacking -= OnTransactionRollbacking;
      Session.Events.TransactionRollbacked -= OnTransactionRollbacked;

      Session.Events.Persisting -= OnPersisting;
      Session.Events.Persisted -= OnPersisted;

      Session.Events.ChangesCanceling -= OnChangesCanceling;
      Session.Events.ChangesCanceled -= OnChangesCanceled;

      Session.Events.EntityCreated -= OnEntityCreated;
      Session.Events.EntityRemoving -= OnEntityRemoving;
      Session.Events.EntityRemove -= OnEntityRemove;

      Session.Events.EntityFieldValueGetting -= OnEntityFieldValueGetting;
      Session.Events.EntityFieldValueGet -= OnEntityFieldValueGet;
      Session.Events.EntityFieldValueSetting -= OnEntityFieldValueSetting;
      Session.Events.EntityFieldValueSet -= OnEntityFieldValueSet;

      Session.Events.QueryExecuting -= OnQueryExecuting;
      Session.Events.QueryExecuting -= OnQueryExecuted;
      Session.Events.DbCommandExecuting -= OnDbCommandExecuting;
      Session.Events.DbCommandExecuted -= OnDbCommandExecuted;
    }

    public EventInfo(Session session, bool throwExceptionOnCommit = false)
      : base(session)
    {
      this.throwExceptionOnCommit = throwExceptionOnCommit;

      Session.Events.TransactionOpening += OnTransactionOpening;

      Session.Events.TransactionPrecommitting += OnTransactionPrecommiting;
      Session.Events.TransactionCommitting += OnTransactionCommiting;
      Session.Events.TransactionCommitted += OnTransactionCommited;

      Session.Events.TransactionRollbacking += OnTransactionRollbacking;
      Session.Events.TransactionRollbacked += OnTransactionRollbacked;

      Session.Events.Persisting += OnPersisting;
      Session.Events.Persisted += OnPersisted;

      Session.Events.ChangesCanceling += OnChangesCanceling;
      Session.Events.ChangesCanceled += OnChangesCanceled;

      Session.Events.EntityCreated += OnEntityCreated;
      Session.Events.EntityRemoving += OnEntityRemoving;
      Session.Events.EntityRemove += OnEntityRemove;

      Session.Events.EntityFieldValueGetting += OnEntityFieldValueGetting;
      Session.Events.EntityFieldValueGet += OnEntityFieldValueGet;
      Session.Events.EntityFieldValueSetting += OnEntityFieldValueSetting;
      Session.Events.EntityFieldValueSet += OnEntityFieldValueSet;

      Session.Events.QueryExecuting += OnQueryExecuting;
      Session.Events.QueryExecuting += OnQueryExecuted;
      Session.Events.DbCommandExecuting += OnDbCommandExecuting;
      Session.Events.DbCommandExecuted += OnDbCommandExecuted;
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class SessionEventsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof(MegaEntity).Assembly, typeof(MegaEntity).Namespace);
      return config;
    }

    [Test]
    public void CommitTransactionTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session)) {
        using (var transactionScope = session.OpenTransaction()) {
          Assert.That(eventInfo.TransactionOpenArgs, Is.Not.Null);
          Assert.That(eventInfo.TransactionOpenArgs?.Transaction, Is.SameAs(Transaction.Current));

          var megaEntity = new MegaEntity { Value = 1 };
          transactionScope.Complete();
        }

        Assert.That(eventInfo.TransactionRollbackingArgs, Is.Null);
        Assert.That(eventInfo.TransactionRollbackedArgs, Is.Null);
        Assert.That(eventInfo.PersistingArgs, Is.Not.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionPrecommitingArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionCommitingArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionCommitedArgs, Is.Not.Null);
      }
    }

    [Test]
    public void RollbackTransactionTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session)) {
        using (var transactionScope = session.OpenTransaction()) {
          Assert.That(eventInfo.TransactionOpenArgs, Is.Not.Null);
          Assert.That(eventInfo.TransactionOpenArgs?.Transaction, Is.SameAs(Transaction.Current));

          var megaEntity = new MegaEntity { Value = 1 };
        }
        Assert.That(eventInfo.TransactionRollbackingArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionRollbackedArgs, Is.Not.Null);
        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);
        Assert.That(eventInfo.TransactionPrecommitingArgs, Is.Null);
        Assert.That(eventInfo.TransactionCommitingArgs, Is.Null);
        Assert.That(eventInfo.TransactionCommitedArgs, Is.Null);
      }
    }

    [Test]
    public void ErrorOnCommitTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session, true)) {
        var transactionScope = session.OpenTransaction();
        Assert.That(eventInfo.TransactionOpenArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionOpenArgs?.Transaction, Is.SameAs(Transaction.Current));

        var megaEntity = new MegaEntity { Value = 1 };

        transactionScope.Complete();
        AssertEx.Throws<TestException>(transactionScope.Dispose);

        Assert.That(eventInfo.TransactionRollbackingArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionRollbackedArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionPrecommitingArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionCommitingArgs, Is.Not.Null);
        Assert.That(eventInfo.PersistingArgs, Is.Not.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionCommitedArgs, Is.Null);
      }
    }

    [Test]
    public void ManualPersistTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var megaEntity = new MegaEntity { Value = 1 };

        using (var eventInfo = new EventInfo(session)) {
          session.SaveChanges();

          Assert.That(eventInfo.PersistingArgs, Is.Not.Null);
          Assert.That(eventInfo.PersistedArgs, Is.Not.Null);
          Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
          Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

          Assert.That(eventInfo.TransactionRollbackingArgs, Is.Null);
          Assert.That(eventInfo.TransactionRollbackedArgs, Is.Null);
          Assert.That(eventInfo.TransactionPrecommitingArgs, Is.Null);
          Assert.That(eventInfo.TransactionCommitingArgs, Is.Null);
          Assert.That(eventInfo.TransactionCommitedArgs, Is.Null);
        }
      }
    }

    [Test]
    public async Task ManualPersistAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var megaEntity = new MegaEntity { Value = 1 };

        using (var eventInfo = new EventInfo(session)) {
          await session.SaveChangesAsync();

          Assert.That(eventInfo.PersistingArgs, Is.Not.Null);
          Assert.That(eventInfo.PersistedArgs, Is.Not.Null);
          Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
          Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

          Assert.That(eventInfo.TransactionRollbackingArgs, Is.Null);
          Assert.That(eventInfo.TransactionRollbackedArgs, Is.Null);
          Assert.That(eventInfo.TransactionPrecommitingArgs, Is.Null);
          Assert.That(eventInfo.TransactionCommitingArgs, Is.Null);
          Assert.That(eventInfo.TransactionCommitedArgs, Is.Null);
        }
      }
    }

    [Test]
    public void EditEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session)) {
        using (var transactionScope = session.OpenTransaction()) {

          var entity = new MegaEntity();
          Assert.That(eventInfo.EntityCreatedArgs, Is.Not.Null);
          Assert.That(eventInfo.EntityCreatedArgs.Entity, Is.EqualTo(entity));

          eventInfo.ResetEventArgs();

          entity.Value = 2;

          Assert.That(eventInfo.EntityFieldValueSettingArgs, Is.Not.Null);
          Assert.That(eventInfo.EntityFieldValueSettingArgs.Entity, Is.EqualTo(entity));
          Assert.That(eventInfo.EntityFieldValueSettingArgs.Value, Is.EqualTo(2));

          Assert.That(eventInfo.EntityFieldValueSetArgs, Is.Not.Null);
          Assert.That(eventInfo.EntityFieldValueSetArgs.Entity, Is.EqualTo(entity));
          Assert.That(eventInfo.EntityFieldValueSetArgs.OldValue, Is.EqualTo(0));
          Assert.That(eventInfo.EntityFieldValueSetArgs.NewValue, Is.EqualTo(2));

          eventInfo.ResetEventArgs();

          int value = entity.Value;

          Assert.That(eventInfo.EntityFieldValueSettingArgs, Is.Null);
          Assert.That(eventInfo.EntityFieldValueSetArgs, Is.Null);

          Assert.That(eventInfo.EntityFieldGettingArgs, Is.Not.Null);
          Assert.That(eventInfo.EntityFieldGettingArgs.Entity, Is.EqualTo(entity));
          Assert.That(eventInfo.EntityFieldValueGetArgs, Is.Not.Null);
          Assert.That(eventInfo.EntityFieldValueGetArgs.Entity, Is.EqualTo(entity));
          Assert.That(eventInfo.EntityFieldValueGetArgs.Value, Is.EqualTo(2));

          eventInfo.ResetEventArgs();

          entity.Remove();
          Assert.That(eventInfo.EntityRemovingArgs, Is.Not.Null);
          Assert.That(eventInfo.EntityRemovingArgs.Entity, Is.EqualTo(entity));
          Assert.That(eventInfo.EntityRemoveArgs, Is.Not.Null);
          Assert.That(eventInfo.EntityRemoveArgs.Entity, Is.EqualTo(entity));
        }
      }
    }

    [Test]
    public void ExecuteLinqQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<MegaEntity>();
        var expression = query.Expression;
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.QueryExecutedArgs?.Exception, Is.Null);

        Assert.That(expression, Is.EqualTo(eventInfo.QueryExecutingArgs?.Expression));
        Assert.That(eventInfo.QueryExecutingArgs?.Expression, Is.EqualTo(eventInfo.QueryExecutedArgs?.Expression));

        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));
      }
    }

    [Test]
    public async Task ExecuteLinqQueryAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.All<MegaEntity>();
        var expression = query.Expression;
        _ = await query.ExecuteAsync();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.QueryExecutedArgs?.Exception, Is.Null);

        Assert.That(expression, Is.EqualTo(eventInfo.QueryExecutingArgs?.Expression));
        Assert.That(eventInfo.QueryExecutingArgs?.Expression, Is.EqualTo(eventInfo.QueryExecutedArgs?.Expression));

        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.All<MegaEntity>();
        expression = query.Expression;
        _ = (await query.ExecuteAsync()).ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.QueryExecutedArgs?.Exception, Is.Null);

        Assert.That(expression, Is.EqualTo(eventInfo.QueryExecutingArgs?.Expression));
        Assert.That(eventInfo.QueryExecutingArgs?.Expression, Is.EqualTo(eventInfo.QueryExecutedArgs?.Expression));

        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));
      }
    }

    [Test]
    public void ExecuteCompiledQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.Execute(q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));
      }
    }

    [Test]
    public async Task ExecuteCompiledQueryAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));
      }
    }

    [Test]
    public void ExecuteDelayedQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e=>e.Id));
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));
      }
    }

    [Test]
    public async Task ExecuteDelayedQueryAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());
        _ = await query.ExecuteAsync();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = await query.ExecuteAsync();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());
        _ = await query.ExecuteAsync();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Null);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = await query.ExecuteAsync();

        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);

        Assert.That(eventInfo.QueryExecutingArgs, Is.Null);
        Assert.That(eventInfo.QueryExecutedArgs, Is.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutingArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Exception, Is.Null);
        Assert.That(eventInfo.DbCommandExecutingArgs?.Command, Is.Not.Null);
        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.Not.Null);

        Assert.That(eventInfo.DbCommandExecutedArgs?.Command, Is.EqualTo(eventInfo.DbCommandExecutingArgs?.Command));
      }
    }

    [Test]
    public void CancelChangesTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session)) {

        using (var transactionScope = session.OpenTransaction()) {

          var megaEntity = new MegaEntity { Value = 1 };

          session.CancelChanges();
        }

        Assert.That(eventInfo.TransactionRollbackingArgs, Is.Not.Null);
        Assert.That(eventInfo.TransactionRollbackedArgs, Is.Not.Null);
        Assert.That(eventInfo.ChangesCancelingArgs, Is.Not.Null);
        Assert.That(eventInfo.ChangesCanceledArgs, Is.Not.Null);
        Assert.That(eventInfo.PersistingArgs, Is.Null);
        Assert.That(eventInfo.PersistedArgs, Is.Null);
        Assert.That(eventInfo.TransactionCommitingArgs, Is.Null);
        Assert.That(eventInfo.TransactionCommitedArgs, Is.Null);
      }
    }
  }
}