// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using System.Linq;

namespace Xtensive.Storage.Manual.Transactions.SessionSwitching
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
  }

  #endregion
  
  [TestFixture]
  public class SessionSwitchingTest
  {
    private Domain existingDomain;

    [Test]
    public void DenySwitchingTest()
    {
      var domain = GetDomain();

      using (var sessionA = Session.Open(domain)) { // Open & activate
        // sessionA is active here

        // Getting Person instance bound to sessionA
        var personA = Query.All<Person>().First();
        using (var sessionB = Session.Open(domain)) { // Open & activate
          // sessionB is active here

          // Getting Person instance bound to sessionB
          // Note: Entity.Key getter requires no Session activation
          var personB = Query.Single<Person>(personA.Key); 
          try {
            // Session switching will be detected here
            bool ignored = personA.Name==personB.Name;
            Assert.Fail("InvalidOperationException is expected.");
          }
          catch (InvalidOperationException) {
            // Must fall here
          }
          catch (Exception) {
            Assert.Fail("InvalidOperationException is expected.");
          }

          // Manual activation, way 1:
          using (Session.Deactivate()) // Manual deactivation
            Assert.IsTrue(personA.Name==personB.Name); // Subsequent activation of sessionA & sessionB

          // Manual activation, way 2:
          string personAName;
          using (personA.Session.Activate()) // Manual activation
            personAName = personA.Name;
          bool isNamesakeRight = personB.Name==personAName;
          Assert.IsTrue(isNamesakeRight);
        }
      }
    }

    [Test]
    public void AllowSwitchingTest()
    {
      var domain = GetDomain();

      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      using (var sessionA = Session.Open(domain, sessionCfg)) { // Open & activate
        // sessionA is active here
        var personA = Query.All<Person>().First();
        using (var sessionB = Session.Open(domain, sessionCfg)) { // Open & activate
          // sessionB is active here
          var personB = Query.Single<Person>(personA.Key); 
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          Assert.IsTrue(personA.Name==personB.Name);
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
            new Person {Name = "Tereza"};
            new Person {Name = "Ivan"};

            transactionScope.Complete();
          }
        }
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}