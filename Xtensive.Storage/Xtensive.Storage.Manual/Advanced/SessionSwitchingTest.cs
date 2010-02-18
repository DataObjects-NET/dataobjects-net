// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using System.Linq;

namespace Xtensive.Storage.Manual.Advanced.SessionSwitching
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
    public void MainTest()
    {
      var domain = GetDomain();

      using (var sessionA = Session.Open(domain)) {
        using (var txA = Transaction.Open(sessionA)) {
          // Getting Person instance bound to sessionA
          var personA = Query.All<Person>().First();
          using (var sessionB = Session.Open(domain)) {
            using (var txB = Transaction.Open(sessionB)) {
              // Getting Person instance bound to sessionB
              // Note: Entity.Key getter requires no Session activation
              var personB = Query.Single<Person>(personA.Key); 
              try {
                // Session switching will be detected here
                bool isNamesakeWrong = personA.Name==personB.Name;
                Assert.Fail("InvalidOperationException is expected.");
              }
              catch (InvalidOperationException) {
                return;
              }
              catch (Exception) {
                Assert.Fail("InvalidOperationException is expected.");
              }

              // Manual activation, way 1:
              using (Session.Deactivate()) // Manual deactivation
                Assert.IsTrue(personA.Name==personB.Name); // Subsequent activation of sessionA & sessionB

              // Manual activation, way 2:
              string personBName;
              using (personB.Session.Activate()) // Manual activation
                personBName = personB.Name;
              bool isNamesakeRight = personA.Name==personBName;
              Assert.IsTrue(isNamesakeRight);
              txB.Complete();
            }
          }
          txA.Complete();
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