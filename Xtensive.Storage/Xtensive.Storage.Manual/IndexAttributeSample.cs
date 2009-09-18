// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.06.17

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Manual.Structures;

namespace Xtensive.Storage.Manual
{
  [TestFixture]
  public class IndexAttributeSample
  {
    #region Sample
    [Index("FirstName", "LastName", Unique = true)]
    [Index("Age")]
    [HierarchyRoot]
    public class Person : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string FirstName { get; set; }

      [Field]
      public string LastName { get; set; }
       
      [Field]
      public int Age { get; set; }
    }

    public IEnumerable<Person> GetPersonOverAge(int age)
    {
      // Filtering and ordering uses secondary index
      return
        from person in Query<Person>.All
        where person.Age > age
        orderby person.Age
        select person;
    }

    public Person GetPersonByName(string firstName, string lastName)
    {
      // Filtering uses unique secondery index
      return (
        from person in Query<Person>.All
        where person.FirstName==firstName && person.LastName==lastName
        select person
        ).FirstOrDefault();
    }
    #endregion

    [Test]
    public void Test()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person));
      var domain = Domain.Build(config);

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          
          var alex = new Person { FirstName = "Alex", LastName = "Kofman", Age = 26};
          var ivan = new Person { FirstName = "Ivan", LastName = "Galkin", Age = 28};

          Assert.AreEqual(alex, GetPersonByName("Alex", "Kofman"));
          
          var adults = GetPersonOverAge(27);
          Assert.AreEqual(1, adults.Count());
          Assert.AreEqual(ivan, adults.First());

          transactionScope.Complete();
        }

        AssertEx.Throws<StorageException>(() => {
          using (var transactionScope = Transaction.Open()) {
            new Person {FirstName = "Alex", LastName = "Kofman", Age = 0};
            transactionScope.Complete();
          }
        });
      }
    }
  }
}