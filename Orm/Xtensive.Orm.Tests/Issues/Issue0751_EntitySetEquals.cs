// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.08.31

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0751_EntitySetEqualsModel;

namespace Xtensive.Orm.Tests.Issues.Issue0751_EntitySetEqualsModel
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
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Company")]
    public EntitySet<Person> Persons { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company();
          var company2 = new Company();
          var person11 = new Person {Company = company1};
          var person12 = new Person {Company = company1};
          var person13 = new Person {Company = company1};
          var person21 = new Person {Company = company2};
          var person22 = new Person {Company = company2};
          var person23 = new Person {Company = company2};
          var query = from company in session.Query.All<Company>()
                      where null==company.Persons
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company();
          var company2 = new Company();
          var person11 = new Person {Company = company1};
          var person12 = new Person {Company = company1};
          var person13 = new Person {Company = company1};
          var person21 = new Person {Company = company2};
          var person22 = new Person {Company = company2};
          var person23 = new Person {Company = company2};
          var query = from company in session.Query.All<Company>()
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company{Name = "Company"};
          var company2 = new Company{Name = "Company"};

          var query =
            from x in
              from c1 in session.Query.All<Company>()
              join c2 in session.Query.All<Company>()
                on c1.Name equals c2.Name
              select new {Company1 = c1, Company2 = c2}
            where x.Company1.Persons==x.Company2.Persons
            select x;
          var result = query.ToList();
          Assert.AreEqual(2, result.Count);
          foreach (var pair in result)
            Assert.AreEqual(pair.Company1, pair.Company2);
          // Rollback
        }
      }
    }
  
    [Test]
    public void NotEqualTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company{Name = "Company"};
          var company2 = new Company{Name = "Company"};

          var query =
            from x in
              from c1 in session.Query.All<Company>()
              join c2 in session.Query.All<Company>()
                on c1.Name equals c2.Name
              select new {Company1 = c1, Company2 = c2}
            where x.Company1.Persons!=x.Company2.Persons
            select x;
          var result = query.ToList();
          Assert.AreEqual(2, result.Count);
          foreach (var pair in result)
            Assert.AreNotEqual(pair.Company1, pair.Company2);
          // Rollback
        }
      }
    }
  
    [Test]
    public void ParameterNullTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company {Name = "Company"};
          var company2 = new Company {Name = "Company"};
          EntitySet<Person> nullEntitySet = null;
          var query =
            from c in session.Query.All<Company>()
            where c.Persons==nullEntitySet
            select c;
          var result = query.ToList();
          Assert.AreEqual(0, result.Count);
          // Rollback
        }
      }
    }
  
    [Test]
    public void ClosureEqual1Test()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company {Name = "Company1"};
          var company2 = new Company {Name = "Company2"};
          EntitySet<Person> persons = company1.Persons;
          var query =
            from c in session.Query.All<Company>()
            where c.Persons==persons
            select c;
          var result = query.ToList();
          Assert.AreEqual(1, result.Count);
          Assert.AreEqual(company1, result.Single());
          // Rollback
        }
      }
    }
  
    [Test]
    public void ClosureEqual2Test()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company {Name = "Company1"};
          var company2 = new Company {Name = "Company2"};
          var query =
            from c in session.Query.All<Company>()
            where c.Persons==company1.Persons
            select c;
          var result = query.ToList();
          Assert.AreEqual(1, result.Count);
          Assert.AreEqual(company1, result.Single());
          // Rollback
        }
      }
    }

    [Test]
    public void ClosureNotEqualTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company {Name = "Company1"};
          var company2 = new Company {Name = "Company2"};
          EntitySet<Person> persons = company1.Persons;
          var query =
            from c in session.Query.All<Company>()
            where c.Persons!=persons
            select c;
          var result = query.ToList();
          Assert.AreEqual(1, result.Count);
          Assert.AreEqual(company2, result.Single());
          // Rollback
        }
      }
    }
  
    [Test]
    public void ParameterEqualTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var company1 = new Company {Name = "Company1"};
          var company2 = new Company {Name = "Company2"};
          EntitySet<Person> persons = company1.Persons;
          ParameterEqual(company1, persons);
          // Rollback
        }
      }
    }

    private void ParameterEqual(Company company1, EntitySet<Person> persons)
    {
      var query =
        from c in Session.Demand().Query.All<Company>()
        where c.Persons==persons
        select c;
      var result = query.ToList();
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(company1, result.Single());
    }
  }
}