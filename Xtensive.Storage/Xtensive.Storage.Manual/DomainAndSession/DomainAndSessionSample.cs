// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.17

using System;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.DomainAndSession
{
  #region Model

  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  #endregion

  [TestFixture]
  public class DomainAndSessionSample
  {
    private Domain existingDomain;

    [Test]
    public void OpenSessionTest()
    {
      var domain = GetDomain();
      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          var person = new Person();
          person.Name = "Barack Obama";

          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void CurrentSessionTest()
    {
      var domain = GetDomain();
      int personId;
      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          personId = new Person().Id;

          transactionScope.Complete();
        }
      }

      using (var session = Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          var newPerson = new Person();
          var fetchedPerson = Query.Single<Person>(personId);

          Console.WriteLine("Our session is current: {0}", Session.Current==session);
          Console.WriteLine("New entity is bound to our session: {0}", newPerson.Session==session);
          Console.WriteLine("Fetched entity is bound to our session: {0}", fetchedPerson.Session==session);

          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void SessionConfigurationTest()
    {
      var config = DomainConfiguration.Load("SessionConfigurationTestDomain");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person));
      config.ValidationMode = ValidationMode.OnDemand;
      var domain = Domain.Build(config);

      var sessionCongfigOne = new SessionConfiguration {
        BatchSize = 25,
        DefaultIsolationLevel = IsolationLevel.ReadCommitted,
        CacheSize = 1000,
        Options = SessionOptions.AutoShortenTransactions
      };

      var sessionConfigTwo = config.Sessions["TestSession"];

      Assert.AreEqual(sessionConfigTwo.BatchSize, sessionCongfigOne.BatchSize);
      Assert.AreEqual(sessionConfigTwo.DefaultIsolationLevel, sessionCongfigOne.DefaultIsolationLevel);
      Assert.AreEqual(sessionConfigTwo.CacheSize, sessionCongfigOne.CacheSize);
      Assert.AreEqual(sessionConfigTwo.Options, sessionCongfigOne.Options);

      using (Session.Open(domain, sessionConfigTwo)) {
        // ...
      }
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        // Creating new Domain configuration
        var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
          UpgradeMode = DomainUpgradeMode.Recreate
        };
        // Registering all types in the specified assembly and namespace
        config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
        // And finally building the domain
        var domain = Domain.Build(config);
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}