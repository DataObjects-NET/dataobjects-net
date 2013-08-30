// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.16

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Model1 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel1;
using Model2 = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel2;

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel1
{
  public abstract class Person : Entity
  {
    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public DateTime Birthday { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }

  }

  [HierarchyRoot]
  public class Author : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; set; }

    [Field]
    public IgnoredTable IgnoredColumn { get; set; }
  }
  
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field(Length = 13)]
    public string ISBN { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  [Index("SomeIgnoredField", "Book", "Customer")]
  public class Order : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string SomeIgnoredField { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Book Book { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Customer Customer { get; set; }
  }

  [HierarchyRoot]
  public class IgnoredTable : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string SomeField { get; set; }

    [Field]
    public Book Book { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Order Order { get; set; }

    [Field]
    [Association(PairTo = "IgnoredColumn")]
    public Author Author { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateModel2
{
  public abstract class Person : Entity
  {
    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public DateTime Birthday { get; set; }
  }

  [HierarchyRoot]
  public class Customer : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }

  }

  [HierarchyRoot]
  public class Author : Person
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; set; }
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field(Length = 13)]
    public string ISBN { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Book Book { get; set; }

    [Field]
    [Association(PairTo = "Orders")]
    public Customer Customer { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class IgnoreRulesValidateTest
  {
    private Key changedOrderKey;

    [Test]
    public void ValidateUpdateTest()
    {
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate);
      initialDomain.Dispose();
      var validatedDomain = BuildDomain(DomainUpgradeMode.Validate);
      validatedDomain.Dispose();
    }

    [Test]
    public void PerformUpdateTest()
    {
      InitialDomainFillData();
      UpgrageDomainPerformMode();
      DomainValidate();
    }
    
    [Test]
    public void PerformSafelyUpdateTest()
    {
      InitialDomainFillData();
      UpgrageDomainPerformSafelyMode();
      DomainValidate();
    }

    private Domain BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      if (mode != DomainUpgradeMode.Recreate) {
        configuration.Types.Register(typeof (Model2.Customer).Assembly, typeof (Model2.Customer).Namespace);
        configuration.IgnoreRules.IgnoreTable("IgnoredTable");
        configuration.IgnoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order");
        configuration.IgnoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author");
      }
      else
        configuration.Types.Register(typeof (Model1.Customer).Assembly, typeof (Model1.Customer).Namespace);
      return Domain.Build(configuration);
    }

    private void InitialDomainFillData()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = new Model1.Author { FirstName = "Иван", LastName = "Гончаров", Birthday = new DateTime(1812, 6, 18) };
        var book = new Model1.Book { ISBN = "9780140440409", Title = "Обломов" };
        book.Authors.Add(author);
        var customer = new Model1.Customer { FirstName = "Алексей", LastName = "Кулаков", Birthday = new DateTime(1988, 8, 31) };
        var order = new Model1.Order { Book = book, Customer = customer, SomeIgnoredField = "Secret information for FBI :)" };
        transaction.Complete();
      }
    }

    private void UpgrageDomainPerformMode()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Perform))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var currentCustomer = session.Query.All<Model2.Customer>().First(c => c.LastName=="Кулаков");
        var order = session.Query.All<Model2.Order>().First(o => o.Customer.LastName==currentCustomer.LastName);
        var newCustomer = new Model2.Customer { FirstName = "Fred", LastName = "Smith", Birthday = new DateTime(1998, 7, 9) };
        order.Customer = newCustomer;
        changedOrderKey = order.Key;
        transaction.Complete();
      }
    }

    private void UpgrageDomainPerformSafelyMode()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.PerformSafely))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var currentCustomer = session.Query.All<Model2.Customer>().First(c => c.LastName=="Кулаков");
        var order = session.Query.All<Model2.Order>().First(o => o.Customer.LastName==currentCustomer.LastName);
        var newCustomer = new Model2.Customer { FirstName = "Fred", LastName = "Smith", Birthday = new DateTime(1998, 7, 9) };
        order.Customer = newCustomer;
        changedOrderKey = order.Key;
        transaction.Complete();
      }
    }
    private void DomainValidate()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Validate;
      configuration.Types.Register(typeof (Model1.Customer).Assembly, typeof (Model1.Customer).Namespace);

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<Model1.Order>().First(o => o.Key==changedOrderKey);
        Assert.That(result.SomeIgnoredField, Is.EqualTo("Secret information for FBI :)"));
      }
    }
  }
}
