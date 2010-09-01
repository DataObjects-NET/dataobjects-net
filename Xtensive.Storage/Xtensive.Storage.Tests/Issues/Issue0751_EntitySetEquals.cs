// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.08.31

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0751_EntitySetEqualsModel;

namespace Xtensive.Storage.Tests.Issues.Issue0751_EntitySetEqualsModel
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Company Company { get; set; }
  }

  [HierarchyRoot]
  public class Company : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(PairTo = "Company")]
    public EntitySet<Person> Persons { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0751_EntitySetEquals : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void EqualToNullTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var company1 = new Company();
          var company2 = new Company();
          var person11 = new Person {Company = company1};
          var person12 = new Person {Company = company1};
          var person13 = new Person {Company = company1};
          var person21 = new Person {Company = company2};
          var person22 = new Person {Company = company2};
          var person23 = new Person {Company = company2};
          var query = from company in Query.All<Company>()
                      where company.Persons==null
                      select company;
          var result = query.ToList();
          Assert.AreEqual(0, result.Count);
          // Rollback
        }
      }
    }
  
    [Test]
    public void NotEqualToNullTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var company1 = new Company();
          var company2 = new Company();
          var person11 = new Person {Company = company1};
          var person12 = new Person {Company = company1};
          var person13 = new Person {Company = company1};
          var person21 = new Person {Company = company2};
          var person22 = new Person {Company = company2};
          var person23 = new Person {Company = company2};
          var query = from company in Query.All<Company>()
                      where company.Persons!=null
                      select company;
          var result = query.ToList();
          Assert.AreEqual(2, result.Count);
          // Rollback
        }
      }
    }

    [Test]
    public void EqualTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var company1 = new Company();
          var company2 = new Company();
          var person11 = new Person {Company = company1};
          var person12 = new Person {Company = company1};
          var person13 = new Person {Company = company1};
          var person21 = new Person {Company = company2};
          var person22 = new Person {Company = company2};
          var person23 = new Person {Company = company2};

          var query = from c in
            from company in Query.All<Company>()
            where company.Persons==null
            select new {Company1 = company, Company2 = company}
                      where c.Company1.Persons==c.Company2.Persons
                      select c;
          var result = query.ToList();
          Assert.AreEqual(2, result.Count);
          // Rollback
        }
      }
    }

    [Test]
    public void NotEqualTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var company1 = new Company();
          var company2 = new Company();
          var person11 = new Person {Company = company1};
          var person12 = new Person {Company = company1};
          var person13 = new Person {Company = company1};
          var person21 = new Person {Company = company2};
          var person22 = new Person {Company = company2};
          var person23 = new Person {Company = company2};

          var query = from c in
            from company in Query.All<Company>()
            where company.Persons==null
            select new {Company1 = company, Company2 = company}
                      where c.Company1.Persons==c.Company2.Persons
                      select c;
          var result = query.ToList();
          Assert.AreEqual(2, result.Count);
          // Rollback
        }
      }
    }
  
  }
}