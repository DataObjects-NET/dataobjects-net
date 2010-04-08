// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.24

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Aspects;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.Transactions.AutoTransactions
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field(Length = 200)]
    public string Surname { get; set; }

    [Field]
    [Association(PairTo = "Friends")]
    public EntitySet<Person> Friends { get; private set; }

    // Transactional - this is default mode for any public ISessionBound method
    public string FullName {
      get {
        return "{Name} {Surname}".FormatWith(this);
      }
    }

    [Infrastructure] // Ok, because it forwards all its job to a single transactional method
    public override string ToString()
    {
      return ToString(false);
    }

    // Transactional - this is default mode for any public ISessionBound method
    public string ToString(bool withFriends)
    {
      if (withFriends)
        return "Person('{0}', Friends={{1}})".FormatWith(FullName, Friends.ToCommaDelimitedString());
      else
        return "Person('{0}')".FormatWith(FullName);
    }

    [Transactional(TransactionOpenMode.New)]
    public void TransactionalMethodRequiringNewTransaction(Transaction outerTransaction)
    {
      Assert.AreSame(Session, Session.Current);
      Assert.AreNotSame(outerTransaction, Transaction.Current);
    }

    [NonTransactional]
    [ActivateSession(true)] // Actually unnecessary - this is default option
    public void NonTransactionalMethod(Transaction expectedTransaction)
    {
      Assert.AreSame(Session, Session.Current);
      Assert.AreSame(expectedTransaction, Transaction.Current);
    }

    [Infrastructure]
    public void InfrastructureMethod(Session expectedSession, Transaction expectedTransaction)
    {
      Assert.AreSame(expectedSession, Session.Current);
      Assert.AreSame(expectedTransaction, Transaction.Current);
    }
  }

  #endregion
  
  [TestFixture]
  public class AutoTransactionsTest
  {
    private Domain existingDomain;
    private Dictionary<string, Key> personKeys = new Dictionary<string, Key>();

    [Test]
    public void MainTest()
    {
      var domain = GetDomain();
      using (var session = Session.Open(domain)) {
        Assert.IsNull(Transaction.Current);
        
        var alex = Query.Single<Person>(personKeys["Alex"]);
        Assert.IsNull(Transaction.Current);

        alex.TransactionalMethodRequiringNewTransaction(null);
        alex.NonTransactionalMethod(null);
        alex.InfrastructureMethod(session, null);
        using (Session.Deactivate()) {
          Assert.IsNull(Transaction.Current);
          Assert.IsNull(Session.Current);
          alex.NonTransactionalMethod(null);
          alex.InfrastructureMethod(null, null);
        }
        
        // Manual transaction
        using (var tx = Transaction.Open()) {
          alex.TransactionalMethodRequiringNewTransaction(tx.Transaction);
          alex.NonTransactionalMethod(tx.Transaction);
          alex.InfrastructureMethod(session, tx.Transaction);
          alex.Name = "Not Alex";
          // no tx.Complete() => rollback
        }
        Assert.AreEqual("Alex", alex.Name); // Auto state update

        // Auto transactions on query enumeration
        Console.WriteLine("All persons:");
        var two = 2;
        foreach (var item in 
          from person in Query.All<Person>()
          select new {person, twoFriends = person.Friends.Take(() => two), friendCount = person.Friends.Count()}) {
          Assert.IsNull(Transaction.Current);
          Console.WriteLine("  {0}, {1}, {2}", 
            item.person,
            item.twoFriends.ToCommaDelimitedString(), // Auto transaction for subquery 
            item.friendCount);
        }

        // Auto transactions on EntitySet enumeration
        Console.WriteLine("Fields of Alex (EntitySet enumeration):");
        foreach (var person in alex.Friends) {
          Assert.IsNull(Transaction.Current);
          Console.WriteLine("  " + person);
        }

        // Auto transactions on scalar queries
        var count         = Query.All<Person>().Count();
        var compiledCount = Query.Execute(()  => Query.All<Person>().Count());
        Assert.AreEqual(count, compiledCount);
        Assert.IsNull(Transaction.Current);

        // Auto transactions on compiled query enumeration
        Console.WriteLine("All persons (compiled query):");
        foreach (var item in Query.Execute(() => 
          from person in Query.All<Person>()
          select new {person, twoFriends = person.Friends.Take(() => two), friendCount = person.Friends.Count()})) {
          Assert.IsNull(Transaction.Current);
          Console.WriteLine("  {0}, {1}, {2}", 
            item.person,
            item.twoFriends.ToCommaDelimitedString(), // Auto transaction for subquery 
            item.friendCount);
        }
      }
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
          UpgradeMode = DomainUpgradeMode.Recreate
        };
        config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
        var domain = Domain.Build(config);
        using (var session = Session.Open(domain)) {
          using (var transactionScope = Transaction.Open(session)) {
            // Creating initial content
            var alex   = new Person {Name = "Alex", Surname = "Yakunin"};
            var dmitri = new Person {Name = "Dmitri", Surname = "Maximov"};
            var ivan   = new Person {Name = "Ivan", Surname = "Galkin"};
            alex.Friends.Add(dmitri);
            alex.Friends.Add(ivan);
            dmitri.Friends.Add(ivan);
            // All are frieds of all now
            foreach (var person in Query.All<Person>())
              personKeys[person.Name] = person.Key;

            transactionScope.Complete();
          }
        }
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}