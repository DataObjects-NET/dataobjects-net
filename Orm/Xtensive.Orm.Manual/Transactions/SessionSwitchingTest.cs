// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using System.Linq;

namespace Xtensive.Orm.Manual.Transactions.SessionSwitching
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

      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;
      sessionCfg.Options |= SessionOptions.AutoTransactionOpenMode;
      using (var sessionA = domain.OpenSession(sessionCfg)) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();
        using (var sessionB = domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          string name = personA.Name;

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              name = personA.Name;
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              name = personA.Name;
            }
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingTest()
    {
      var domain = GetDomain();

      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;
      sessionCfg.Options |= SessionOptions.AutoTransactionOpenMode;
      using (var sessionA = domain.OpenSession(sessionCfg)) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();
        using (var sessionB = domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            var name = personA.Name;
          }
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
        var sessionCfg = new SessionConfiguration();
        sessionCfg.Options |= SessionOptions.AutoActivation;
        using (var session = domain.OpenSession(sessionCfg)) {
          using (var transactionScope = session.OpenTransaction()) {
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