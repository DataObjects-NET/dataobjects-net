// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.17

using System;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.DomainAndSession
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    // Providing session explicitly
    public Person(Session session)
      : base(session)
    {}

    // Active session is required
    public Person()
    {}
  }

  #endregion

  [TestFixture]
  public class DomainAndSessionSample : HasConfigurationAccessTest
  {
    #region Connection URL examples
    
    public const string SqlServerUrl1  = @"sqlserver://localhost/MyDatabase";
    public const string SqlServerUrl2  = @"sqlserver://dbServer\MSSQL2008/Production";
    public const string OracleUrl1     = @"oracle://user:password@localhost/MyDatabase";
    public const string OracleUrl2     = @"oracle://user:password@dbServer:5511/MyDatabase";
    public const string PostrgeSqlUrl1 = @"postgresql://user:password@127.0.0.1:8032/MyDatabase?Encoding=Unicode";
    public const string PostrgeSqlUrl2 = @"postgresql://user:password@dbServer/MyDatabase?Pooling=on&MinPoolSize=1&MaxPoolSize=5";
    public const string InMemoryUrl    = @"memory://localhost/MyDatabase";
    
    #endregion

    [Test]
    public void OpenSessionTest()
    {
      #region Domain sample

      // Creating new Domain configuration
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      // Registering all types in the specified assembly and namespace
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          var person = new Person(session);
          person.Name = "Barack Obama";

          transactionScope.Complete();
        }
      }

      #endregion
    }

    [Test]
    public void ConnectionStringTest()
    {
      #region Connection string sample

      // Creating new Domain configuration
      var config = DomainConfigurationFactory.CreateForConnectionStringTest();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      // Registering all types in the specified assembly and namespace
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      #endregion
    }

    [Test]
    public void ConnectionStringInAppConfigTest()
    {
      #region Connection string in App.config sample

      // Creating new Domain configuration
      var config = LoadDomainConfiguration("mssql2005cs");
      // Registering all types in the specified assembly and namespace
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      #endregion
    }

    [Test]
    public void CurrentSessionTest()
    {
      #region Session sample

      // Creating new Domain configuration
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      // Registering all types in the specified assembly and namespace
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      int personId;
      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          personId = new Person(session).Id;

          transactionScope.Complete();
        }
      }

      // Opens the session with default configuration - ServerProfile. Auto actiovation option is disabled.
      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          // you need to provide session explicitly 
          var newPerson = new Person(session);
          var fetchedPerson = session.Query.Single<Person>(personId);

          Console.WriteLine("Current session is null: {0}", Session.Current==null);
          Console.WriteLine("New entity is bound to our session: {0}", newPerson.Session==session);
          Console.WriteLine("Fetched entity is bound to our session: {0}", fetchedPerson.Session==session);

          transactionScope.Complete();
        }
      }

      // New session configuration with actiovation option enabled.
      var sessionConfiguration = new SessionConfiguration(SessionOptions.AutoActivation);
      using (var session = domain.OpenSession(sessionConfiguration)) {
        using (var transactionScope = session.OpenTransaction()) {

          // you don't need to provide session explicitly. Active session will be used instead.
          var newPerson = new Person();
          var fetchedPerson = session.Query.Single<Person>(personId);

          Console.WriteLine("Our session is current: {0}", Session.Current==session);
          Console.WriteLine("New entity is bound to our session: {0}", newPerson.Session==session);
          Console.WriteLine("Fetched entity is bound to our session: {0}", fetchedPerson.Session==session);

          transactionScope.Complete();
        }
      }

      #endregion

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          var person = new Person(session);
          person.Name = "Barack Obama";

          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void SessionConfigurationTest()
    {
      // Creating new Domain configuration
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      // Registering all types in the specified assembly and namespace
      config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      // First named Session configuration
      var sessionCongfigOne = new SessionConfiguration {
        BatchSize = 25,
        DefaultIsolationLevel = IsolationLevel.RepeatableRead,
        CacheSize = 16384,
        Options = SessionOptions.ServerProfile
      };

      // Second named Session configuration
      var sessionConfigTwo = config.Sessions["TestSession"];

      Assert.AreEqual(sessionConfigTwo.BatchSize, sessionCongfigOne.BatchSize);
      Assert.AreEqual(sessionConfigTwo.DefaultIsolationLevel, sessionCongfigOne.DefaultIsolationLevel);
      Assert.AreEqual(sessionConfigTwo.CacheSize, sessionCongfigOne.CacheSize);
      Assert.AreEqual(sessionConfigTwo.Options, sessionCongfigOne.Options);

      using (domain.OpenSession(sessionConfigTwo)) {
        // ...
      }
    }
  }
}
