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
    public EntityEventArgs EntityRemoving;
    public EntityEventArgs EntityRemoved;

    public EntityFieldEventArgs EntityFieldGettingArgs;
    public EntityFieldValueEventArgs EntityFieldValueGetArgs;
    public EntityFieldValueEventArgs EntityFieldValueSettingArgs;
    public EntityFieldValueSetEventArgs EntityFieldValueSetArgs;

    public QueryEventArgs QueryExecuting;
    public QueryEventArgs QueryExecuted;

    public DbCommandEventArgs? DbCommandExecuting;
    public DbCommandEventArgs? DbCommandExecuted;

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
      EntityRemoving = null;
      EntityRemoved = null;

      EntityFieldGettingArgs = null;
      EntityFieldValueGetArgs = null;
      EntityFieldValueSettingArgs = null;
      EntityFieldValueSetArgs = null;

      QueryExecuting = null;
      QueryExecuted = null;

      DbCommandExecuting = null;
      DbCommandExecuted = null;
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
    private void OnEntityRemoving(object sender, EntityEventArgs e) => EntityRemoving = e;
    private void OnEntityRemove(object sender, EntityEventArgs e) => EntityRemoved = e;
    private void OnEntityFieldValueGetting(object sender, EntityFieldEventArgs e) => EntityFieldGettingArgs = e;
    private void OnEntityFieldValueGet(object sender, EntityFieldValueEventArgs e) => EntityFieldValueGetArgs = e;
    private void OnEntityFieldValueSetting(object sender, EntityFieldValueEventArgs e) => EntityFieldValueSettingArgs = e;
    private void OnEntityFieldValueSet(object sender, EntityFieldValueSetEventArgs e) => EntityFieldValueSetArgs = e;
    private void OnQueryExecuting(object sender, QueryEventArgs e) => QueryExecuting = e;
    private void OnQueryExecuted(object sender, QueryEventArgs e) => QueryExecuted = e;
    private void OnDbCommandExecuting(object sender, DbCommandEventArgs e) => DbCommandExecuting = e;
    private void OnDbCommandExecuted(object sender, DbCommandEventArgs e) => DbCommandExecuted = e;

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
      config.Types.Register(typeof(MegaEntity).Assembly, typeof(MegaEntity).Namespace);
      return config;
    }

    [Test]
    public void CommitTransactionTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session)) {
        using (var transactionScope = session.OpenTransaction()) {
          Assert.IsNotNull(eventInfo.TransactionOpenArgs);
          Assert.AreSame(Transaction.Current, eventInfo.TransactionOpenArgs?.Transaction);

          var megaEntity = new MegaEntity { Value = 1 };
          transactionScope.Complete();
        }

        Assert.IsNull(eventInfo.TransactionRollbackingArgs);
        Assert.IsNull(eventInfo.TransactionRollbackedArgs);
        Assert.IsNotNull(eventInfo.PersistingArgs);
        Assert.IsNotNull(eventInfo.PersistedArgs);
        Assert.IsNotNull(eventInfo.TransactionPrecommitingArgs);
        Assert.IsNotNull(eventInfo.TransactionCommitingArgs);
        Assert.IsNotNull(eventInfo.TransactionCommitedArgs);
      }
    }

    [Test]
    public void RollbackTransactionTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session)) {
        using (var transactionScope = session.OpenTransaction()) {
          Assert.IsNotNull(eventInfo.TransactionOpenArgs);
          Assert.AreSame(Transaction.Current, eventInfo.TransactionOpenArgs?.Transaction);

          var megaEntity = new MegaEntity { Value = 1 };
        }
        Assert.IsNotNull(eventInfo.TransactionRollbackingArgs);
        Assert.IsNotNull(eventInfo.TransactionRollbackedArgs);
        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);
        Assert.IsNull(eventInfo.TransactionPrecommitingArgs);
        Assert.IsNull(eventInfo.TransactionCommitingArgs);
        Assert.IsNull(eventInfo.TransactionCommitedArgs);
      }
    }

    [Test]
    public void ErrorOnCommitTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session, true)) {
        var transactionScope = session.OpenTransaction();
        Assert.IsNotNull(eventInfo.TransactionOpenArgs);
        Assert.AreSame(Transaction.Current, eventInfo.TransactionOpenArgs?.Transaction);

        var megaEntity = new MegaEntity { Value = 1 };

        transactionScope.Complete();
        AssertEx.Throws<TestException>(transactionScope.Dispose);

        Assert.IsNotNull(eventInfo.TransactionRollbackingArgs);
        Assert.IsNotNull(eventInfo.TransactionRollbackedArgs);
        Assert.IsNotNull(eventInfo.TransactionPrecommitingArgs);
        Assert.IsNotNull(eventInfo.TransactionCommitingArgs);
        Assert.IsNotNull(eventInfo.PersistingArgs);
        Assert.IsNotNull(eventInfo.PersistedArgs);
        Assert.IsNull(eventInfo.TransactionCommitedArgs);
      }
    }

    [Test]
    public void EditEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session)) {
        using (var transactionScope = session.OpenTransaction()) {

          var entity = new MegaEntity();
          Assert.IsNotNull(eventInfo.EntityCreatedArgs);
          Assert.AreEqual(entity, eventInfo.EntityCreatedArgs.Entity);

          eventInfo.ResetEventArgs();

          entity.Value = 2;

          Assert.IsNotNull(eventInfo.EntityFieldValueSettingArgs);
          Assert.AreEqual(entity, eventInfo.EntityFieldValueSettingArgs.Entity);
          Assert.AreEqual(2, eventInfo.EntityFieldValueSettingArgs.Value);

          Assert.IsNotNull(eventInfo.EntityFieldValueSetArgs);
          Assert.AreEqual(entity, eventInfo.EntityFieldValueSetArgs.Entity);
          Assert.AreEqual(0, eventInfo.EntityFieldValueSetArgs.OldValue);
          Assert.AreEqual(2, eventInfo.EntityFieldValueSetArgs.NewValue);

          eventInfo.ResetEventArgs();

          int value = entity.Value;

          Assert.IsNull(eventInfo.EntityFieldValueSettingArgs);
          Assert.IsNull(eventInfo.EntityFieldValueSetArgs);

          Assert.IsNotNull(eventInfo.EntityFieldGettingArgs);
          Assert.AreEqual(entity, eventInfo.EntityFieldGettingArgs.Entity);
          Assert.IsNotNull(eventInfo.EntityFieldValueGetArgs);
          Assert.AreEqual(entity, eventInfo.EntityFieldValueGetArgs.Entity);
          Assert.AreEqual(2, eventInfo.EntityFieldValueGetArgs.Value);

          eventInfo.ResetEventArgs();

          entity.Remove();
          Assert.IsNotNull(eventInfo.EntityRemoving);
          Assert.AreEqual(entity, eventInfo.EntityRemoving.Entity);
          Assert.IsNotNull(eventInfo.EntityRemoved);
          Assert.AreEqual(entity, eventInfo.EntityRemoved.Entity);
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

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNotNull(eventInfo.QueryExecuting);
        Assert.IsNotNull(eventInfo.QueryExecuted);
        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.QueryExecuted?.Exception);

        Assert.AreEqual(eventInfo.QueryExecuting?.Expression, expression);
        Assert.AreEqual(eventInfo.QueryExecuted?.Expression, eventInfo.QueryExecuting?.Expression);

        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);
        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);
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

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNotNull(eventInfo.QueryExecuting);
        Assert.IsNotNull(eventInfo.QueryExecuted);
        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.QueryExecuted?.Exception);

        Assert.AreEqual(eventInfo.QueryExecuting?.Expression, expression);
        Assert.AreEqual(eventInfo.QueryExecuted?.Expression, eventInfo.QueryExecuting?.Expression);

        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);
        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.All<MegaEntity>();
        expression = query.Expression;
        _ = (await query.ExecuteAsync()).ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNotNull(eventInfo.QueryExecuting);
        Assert.IsNotNull(eventInfo.QueryExecuted);
        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.QueryExecuted?.Exception);

        Assert.AreEqual(eventInfo.QueryExecuting?.Expression, expression);
        Assert.AreEqual(eventInfo.QueryExecuted?.Expression, eventInfo.QueryExecuting?.Expression);

        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);
        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);
      }
    }

    [Test]
    public void ExecuteCompiledQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.Execute(q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);
        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);
        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.Execute(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);
      }
    }

    [Test]
    public async Task ExecuteCompiledQueryAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = await session.Query.ExecuteAsync(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);
      }
    }

    [Test]
    public void ExecuteDelayedQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e=>e.Id));
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = query.ToList();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);
      }
    }

    [Test]
    public async Task ExecuteDelayedQueryAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var eventInfo = new EventInfo(session))
      using (var transaction = session.OpenTransaction()) {
        var query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>());
        _ = await query.ExecuteAsync();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = await query.ExecuteAsync();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>());
        _ = await query.ExecuteAsync();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting);
        Assert.IsNull(eventInfo.DbCommandExecuted);

        eventInfo.ResetEventArgs();

        query = session.Query.CreateDelayedQuery(new object(), q => q.All<MegaEntity>().OrderBy(e => e.Id));
        _ = await query.ExecuteAsync();

        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);

        Assert.IsNull(eventInfo.QueryExecuting);
        Assert.IsNull(eventInfo.QueryExecuted);

        Assert.IsNotNull(eventInfo.DbCommandExecuting);
        Assert.IsNotNull(eventInfo.DbCommandExecuted);

        Assert.IsNull(eventInfo.DbCommandExecuting?.Exception);
        Assert.IsNull(eventInfo.DbCommandExecuted?.Exception);
        Assert.IsNotNull(eventInfo.DbCommandExecuting?.Command);
        Assert.IsNotNull(eventInfo.DbCommandExecuted?.Command);

        Assert.AreEqual(eventInfo.DbCommandExecuting?.Command, eventInfo.DbCommandExecuted?.Command);
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

        Assert.IsNotNull(eventInfo.TransactionRollbackingArgs);
        Assert.IsNotNull(eventInfo.TransactionRollbackedArgs);
        Assert.IsNotNull(eventInfo.ChangesCancelingArgs);
        Assert.IsNotNull(eventInfo.ChangesCanceledArgs);
        Assert.IsNull(eventInfo.PersistingArgs);
        Assert.IsNull(eventInfo.PersistedArgs);
        Assert.IsNull(eventInfo.TransactionCommitingArgs);
        Assert.IsNull(eventInfo.TransactionCommitedArgs);
      }
    }
  }
}