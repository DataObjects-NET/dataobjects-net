// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.17

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.IoC;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Manual.ModellingDomain.AuditAndOpenGenericsTest
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

    internal TransactionInfo()
    {} // Only this assmebly may create or modify it
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
      return "{0,7} {1}\r\n{2,7} Now: {3}".FormatWith(
        ChangeType,
        EntityAsString,
        string.Empty,
        Entity.IsRemoved() ? "removed " +EntityKey : Entity.ToString());
    }

    internal AuditRecord()
    {} // Only this assmebly may create or modify it
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
      return "{0} (Id = {1}), Pets: {2}".FormatWith(Name, Id, 
        Pets.Select(p => p.Name).ToCommaDelimitedString());
    }
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
      return "{0} ({1}, Id = {2}), Owner: {3}".FormatWith(Name, GetType().GetShortName(), 
        Id, Owner==null ? "none" : Owner.Name);
    }
  }

  [Serializable]
  public class Cat : Animal
  {
  }

  [Serializable]
  public class Dog : Animal
  {
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
        args.Session.TransactionOpened += TransactionOpened;
        args.Session.TransactionCommitting += TransactionCommitting;
        args.Session.EntityCreated  += (sender, e) => EntityEvent(sender, e, true);
        args.Session.EntityRemoveCompleted  += (sender, e) => EntityEvent(sender, e, false);
        args.Session.EntityFieldValueSetCompleted += (sender, e) => EntityEvent(sender, e, false); 
        // To catches events from EntitySet<T> fields
        args.Session.EntityVersionInfoChanged += (sender, e) => EntityEvent(sender, e, false);
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

      var transactionInfo = new TransactionInfo {
        Guid = transaction.Guid,
        TimeStamp = transaction.TimeStamp
      };

      var batches = info.ChangedEntities.Batch(); // To avoid further GCs, if set is large
      foreach (var changedEntities in batches) {
        changedEntities.Prefetch<AuditRecord, Key>(key => key);
        foreach (var key in changedEntities) {
          var entity = Query.SingleOrDefault(key);
          bool isRemoved = entity.IsRemoved();
          bool isCreated = info.CreatedEntities.Contains(key);
          if (isRemoved && isCreated)
            continue; // Nothing really happened ;)
          
          var entityHierarchyRootType = (isRemoved ? key.Type : entity.Type).Hierarchy.Root.UnderlyingType;
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

      Assert.AreEqual(8, autoGenericInstances.Count);

      Cat tom;
      Cat musya;
      Person alex;

      using (Session.Open(domain)) {
        using (var tx = Transaction.Open()) {
          tom = new Cat {Name = "Tom"};
          new Dog {Name = "Sharik"};
          tx.Complete();
        }
        Assert.AreEqual(1, Query.All<TransactionInfo>().Count());
        Assert.AreEqual(2, Query.All<AuditRecord<Animal>>().Count());

        using (var tx = Transaction.Open()) {
          musya = new Cat();
          tx.Complete();
        }

        // Auto transactions
        musya.Name = "Musya";
        musya.Remove();

        // Rollback test
        using (var tx = Transaction.Open()) {
          tom.Name = "Shushera";
          // tx.Complete();
        }

        // Another session & transaction
        using (Session.Deactivate()) // Blocks session switching check
        using (Session.Open(domain)) {
          using (var tx = Transaction.Open()) {
            alex = new Person {Name = "Alex"};
            tx.Complete();
          }
        }

        alex = Query.Single<Person>(alex.Key); // Materializing entity from enother Session here

        // Auto transaction
        tom.Owner = alex;

        // And now - the magic!
        Console.WriteLine("Audit log:");
        using (var tx = Transaction.Open()) {
          var auditRecords =
            from record in Query.All<AuditRecord>()
            let transaction = record.Transaction
            orderby transaction.Id, record.EntityKey
            select record;

          TransactionInfo lastTransaction = null;
          foreach (var record in auditRecords) {
            var transaction = record.Transaction;
            if (lastTransaction!=transaction) {
              // Client-side grouping ;) Actually, for nicer output.
              Console.WriteLine();
              Console.WriteLine(transaction.ToString());
              lastTransaction = transaction;
            }
            Console.WriteLine(record.ToString().Indent(2));
          }
          tx.Complete();
        }
      }
    }
  }
}