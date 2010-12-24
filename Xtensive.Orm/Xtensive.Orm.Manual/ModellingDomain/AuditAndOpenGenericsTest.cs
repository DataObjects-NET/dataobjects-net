// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.17

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Reflection;

namespace Xtensive.Orm.Manual.ModellingDomain.AuditAndOpenGenericsTest
{
  #region Model

  [Service(typeof(KeyGenerator), Name="Transaction")]
  public class TransactionKeyGenerator : CachingKeyGenerator<long>
  {
    [ServiceConstructor]
    public TransactionKeyGenerator(DomainConfiguration configuration)
      : base(configuration)
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(Name = "Transaction")] // To ensure there is its own sequence
  [Index("Guid", Unique = true)]
  public class TransactionInfo : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public Guid Guid { get; internal set; }

    [Field]
    public DateTime TimeStamp { get; internal set; }

    [Field]
    public TimeSpan Duration { get; internal set; }

    public override string ToString()
    {
      return "Transaction #{0} ({1}, {2}ms)".FormatWith(
        Id, TimeStamp, Duration.TotalMilliseconds);
    }

    // Only this assmebly may create or modify this instance
    internal TransactionInfo(Session session)
      : base (session)
    {
      var thisType = typeof (TransactionInfo);
      try {
        Initialize(thisType);
      }
      catch (Exception e) {
        InitializationError(thisType, e);
        throw;
      }
    }
  }

  public enum EntityChangeType
  {
    Created,
    Changed,
    Removed
  }

  [Serializable]
  [HierarchyRoot]
  [Index("Transaction", "EntityKey", Unique = true)]
  public abstract class AuditRecord : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public TransactionInfo Transaction { get; internal set; }

    [Field]
    public Key EntityKey { get; internal set; }

    [Field]
    public EntityChangeType ChangeType { get; internal set; }

    [Field]
    public string EntityAsString { get; internal set; }

    public abstract Entity UntypedEntity { get; internal set; }

    protected AuditRecord(Session session)
      : base(session)
    {}
  }

  // Unused, here it is just to show one more open generic
  [Serializable]
  [HierarchyRoot]
  public sealed class SyncInfo<T> : Entity
    where T: Entity
  {
    [Field, Key]
    [Association(OnTargetRemove = OnRemoveAction.None)] 
    public T Entity { get; private set; }

    public SyncInfo(Session session)
      : base(session)
    {}
  }

  [Serializable]
  public sealed class AuditRecord<T> : AuditRecord
    where T: Entity
  {
    public override Entity UntypedEntity {
      get { return Entity; }
      internal set { Entity = (T) value; }
    }

    [Field]
    // Required to eliminate FK contraint:
    [Association(OnTargetRemove = OnRemoveAction.None)] 
    public T Entity { get; private set; }

    public override string ToString()
    {
      return "{0,7} {1}\r\n{2,7} Current state: {3}".FormatWith(
        ChangeType,
        EntityAsString,
        string.Empty,
        Entity.IsRemoved() ? "removed " +EntityKey : Entity.ToString());
    }

    // Only this assmebly may create or modify this instance
    internal AuditRecord()
      : base (Session.Demand())
    {
      var thisType = typeof (AuditRecord<>);
      try {
        Initialize(thisType);
      }
      catch (Exception e) {
        InitializationError(thisType, e);
        throw;
      }
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<Animal> Pets { get; private set; }

    public override string ToString()
    {
      return "{0} {1}, Id: #{2}, Pets: {3}".FormatWith(GetType().GetShortName(), Name, 
        Id, Pets.Select(p => p.Name).ToCommaDelimitedString());
    }

    public Person(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Animal : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Person Owner { get; set; }

    public override string ToString()
    {
      return "{0} {1}, Id: #{2}, Owner: {3}".FormatWith(GetType().GetShortName(), Name, 
        Id, Owner==null ? "none" : Owner.Name);
    }

    public Animal(Session session)
      : base(session)
    {}
  }

  [Serializable]
  public class Cat : Animal
  {
    public Cat(Session session)
      : base(session)
    {}
  }

  [Serializable]
  public class Dog : Animal
  {
    public Dog(Session session)
      : base(session)
    {}
  }

  #endregion

  #region SimpleAuditModule (IModule)

  public sealed class SimpleAuditModule : IModule
  {
    private class AuditModuleInfo
    {
      public HashSet<Key> CreatedEntities = new HashSet<Key>();
      public HashSet<Key> ChangedEntities = new HashSet<Key>();

      public AuditModuleInfo()
      {
      }
    }

    private Domain Domain { get; set; }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {}

    public void OnBuilt(Domain domain)
    {
      Domain = domain;
      domain.SessionOpen += (source, args) => {
        args.Session.Events.TransactionOpened += TransactionOpened;
        args.Session.Events.TransactionCommitting += TransactionCommitting;
        args.Session.Events.EntityCreated  += (sender, e) => EntityEvent(sender, e, true);
        args.Session.Events.EntityRemoveCompleted  += (sender, e) => EntityEvent(sender, e, false);
        args.Session.Events.EntityFieldValueSetCompleted += (sender, e) => EntityEvent(sender, e, false); 
        // To catches events from EntitySet<T> fields
        args.Session.Events.EntityVersionInfoChanged += (sender, e) => EntityEvent(sender, e, false);
      };
    }

    private void TransactionOpened(object sender, TransactionEventArgs e)
    {
      var transaction = e.Transaction;
      if (transaction.IsNested)
        return;
      var session = transaction.Session;
      if (session.IsDisconnected)
        return;
      var info = new AuditModuleInfo();
      session.Extensions.Set(info);
    }

    private void TransactionCommitting(object sender, TransactionEventArgs e)
    {
      var transaction = e.Transaction;
      if (transaction.IsNested)
        return;
      var session = transaction.Session;
      if (session.IsDisconnected)
        return;
      var info = session.Extensions.Get<AuditModuleInfo>();
      if (info.ChangedEntities.Count==0)
        return; // Logging only writing transactions

      var transactionInfo = new TransactionInfo (session) {
        Guid = transaction.Guid,
        TimeStamp = transaction.TimeStamp
      };
      using (var ss = session.Activate()) {
        var batches = info.ChangedEntities.Batch(); // To avoid further GCs, if set is large
        foreach (var changedEntities in batches) {
          session.Query.Many<AuditRecord>(changedEntities).Run();
          foreach (var key in changedEntities) {
            var entity = session.Query.SingleOrDefault(key);
            bool isRemoved = entity.IsRemoved();
            bool isCreated = info.CreatedEntities.Contains(key);
            if (isRemoved && isCreated)
              continue; // Nothing really happened ;)
          
            var entityHierarchyRootType = (isRemoved ? key.TypeInfo : entity.TypeInfo).Hierarchy.Root.UnderlyingType;
            var recordType = typeof (AuditRecord<>).MakeGenericType(entityHierarchyRootType);
            var auditRecord = (AuditRecord) Activator.CreateInstance(recordType, true);
            auditRecord.Transaction = transactionInfo;
            auditRecord.ChangeType = 
              isRemoved ? EntityChangeType.Removed : // Order is important here!
              isCreated ? EntityChangeType.Created :
                          EntityChangeType.Changed;
            auditRecord.EntityAsString = isRemoved ? key.ToString() : entity.ToString();
            auditRecord.EntityKey = key;
            if (!isRemoved)
              auditRecord.UntypedEntity = entity;
          }
          // In the end of all, to log it more precisely:
          transactionInfo.Duration = DateTime.UtcNow - transactionInfo.TimeStamp;
        }
      }
    }

    private void EntityEvent(object sender, EntityEventArgs e, bool created)
    {
      try {
        var entity = e.Entity;
        if (entity is AuditRecord)
          return; // Avoding recursion ;)
        if (entity is TransactionInfo)
          return; // That's silly ;)
        var session = entity.Session;
        if (session.IsDisconnected)
          return;
        var info = session.Extensions.Get<AuditModuleInfo>();
        if (info.ChangedEntities.Contains(entity.Key))
          return;
        info.ChangedEntities.Add(entity.Key);
        if (created)
          info.CreatedEntities.Add(entity.Key);
      }
      catch {
        // Must not affect on operation!
        return;
      }
    }
  }

  #endregion

  [TestFixture]
  public class AuditAndOpenGenericsTest
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      var domain = Domain.Build(config);

      DumpAutoRegisteredGenericInstances(domain);

      Cat tom;
      Cat musya;
      Person alex;
      using (var session = domain.OpenSession())
      using (var ss = session.Activate()) {
        using (var tx = session.OpenTransaction()) {
          tom = new Cat (session) {Name = "Tom"};
          new Dog (session) {Name = "Sharik"};
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          Assert.AreEqual(1, session.Query.All<TransactionInfo>().Count());
          Assert.AreEqual(2, session.Query.All<AuditRecord<Animal>>().Count());
          musya = new Cat(session);
          tx.Complete();
        }

        // Auto transactions
        using (var tx = session.OpenTransaction()) {
          musya.Name = "Musya";
          musya.Remove();
          tx.Complete();
        }

        // Rollback test
        using (var tx = session.OpenTransaction()) {
          tom.Name = "Shushera";
          // tx.Complete();
        }

        // Another session & transaction
        using (Session.Deactivate()) // Blocks session switching check
        using (var session2 = domain.OpenSession()) {
          using (var tx = session2.OpenTransaction()) {
            alex = new Person (session2) {Name = "Alex"};
            tx.Complete();
          }
        }

        using (var tx = session.OpenTransaction()) {
          alex = session.Query.Single<Person>(alex.Key); // Materializing entity from enother Session here
        }

        using (var tx = session.OpenTransaction())
        {
          tom.Owner = alex;
          tx.Complete();
        }

        // And now - the magic!
        DumpAuditLog();
      }
    }

    private void DumpAutoRegisteredGenericInstances(Domain domain)
    {
      var registeredTypes = (
        from typeInfo in domain.Model.Types
        orderby typeInfo.UnderlyingType.GetShortName()
        select typeInfo).ToList();

      var autoGenericInstances = (
        from typeInfo in registeredTypes
        where typeInfo.IsAutoGenericInstance
        select typeInfo).ToList();

      Console.WriteLine("Automatically registered generic instances:");
      foreach (var typeInfo in autoGenericInstances)
        Console.WriteLine("  {0}", typeInfo.UnderlyingType.GetShortName());
      Console.WriteLine();
      Assert.AreEqual(6, autoGenericInstances.Count);
    }

    [Transactional(TransactionalBehavior.Open)]
    private void DumpAuditLog()
    {
      Console.WriteLine("Audit log:");
      var session = Session.Demand();
      var auditTable =
        from record in session.Query.All<AuditRecord>()
        let transaction = record.Transaction
        orderby transaction.Id , record.EntityKey
        select new {Record = record, Transaction = transaction};

      // Prefetching AuditRecord<T> - so far we're sure there is only
      // AuditRecord fields and Transaction, but we need descendant fields
      // as well.
      Session.Demand().Query
        .Many<Entity>(auditTable.Select(e => e.Record.Key))
        .Run();
      // Prefetching AuditRecord.EntityKey
      Session.Demand().Query
        .Many<Entity>(auditTable.Select(e => e.Record.EntityKey))
        .Run();

      TransactionInfo lastTransaction = null;
      foreach (var entry in auditTable) {
        var transaction = entry.Transaction;
        if (lastTransaction!=transaction) {
          // Client-side grouping ;) Actually, for nicer output.
          Console.WriteLine();
          Console.WriteLine(transaction.ToString());
          lastTransaction = transaction;
        }
        Console.WriteLine(entry.Record.ToString().Indent(2));
      }
    }
  }
}