// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.16

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Customer = Xtensive.Orm.Tests.Storage.IgnoreRulesValidateValidateModel.Customer;

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateRecreateModel
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

namespace Xtensive.Orm.Tests.Storage.IgnoreRulesValidateValidateModel
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
    [Test]
    public void MainTest()
    {
      var initialDomain = BuildDomain(DomainUpgradeMode.Recreate);
      initialDomain.Dispose();
      var validatedDomain = BuildDomain(DomainUpgradeMode.Validate);
      validatedDomain.Dispose();
    }

    [Test]
    public void IgnoreRuleConfigTest()
    {
      var configuration = DomainConfiguration.Load("AppConfigTest", "IgnoreRuleConfigTest");
      ValidateIgnoringConfiguration(configuration);
      var clone = configuration.Clone();
      ValidateIgnoringConfiguration(clone);

      var good = configuration.Clone();
      good.IgnoreRules.Clear();
      good.IgnoreRules.IgnoreTable("ignored-table").WhenDatabase("Other-DO40-Test").WhenSchema("dbo");
      good.IgnoreRules.IgnoreColumn("ignored-column");
      good.Lock();
    }
    
    private void ValidateIgnoringConfiguration(DomainConfiguration configuration)
    {
      Assert.That(configuration.DefaultDatabase, Is.EqualTo("main"));
      Assert.That(configuration.DefaultSchema, Is.EqualTo("dbo"));
      Assert.That(configuration.IgnoreRules.Count, Is.EqualTo(11));
      var rule = configuration.IgnoreRules[0];
      Assert.That(rule.Database, Is.EqualTo("Other-DO40-Tests"));
      var rule2 = configuration.IgnoreRules[2];
      Assert.That(rule2.Schema, Is.EqualTo("some-schema3"));
      Assert.That(rule2.Table, Is.EqualTo("table2"));
      Assert.That(rule2.Column, Is.EqualTo("col3"));
      var databases = configuration.Databases;
      Assert.That(databases.Count, Is.EqualTo(2));
      Assert.That(databases[0].Name, Is.EqualTo("main"));
      Assert.That(databases[0].RealName, Is.EqualTo("DO40-Tests"));
      Assert.That(databases[1].Name, Is.EqualTo("other"));
      Assert.That(databases[1].RealName, Is.EqualTo("Other-DO40-Tests"));
      configuration.Lock();
    }

    private Domain BuildDomain(DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      if (mode == DomainUpgradeMode.Validate)
      {
        configuration.Types.Register(typeof(Customer).Assembly, typeof(Customer).Namespace);
        configuration.IgnoreRules.IgnoreTable("IgnoredTable");
        configuration.IgnoreRules.IgnoreColumn("SomeIgnoredField").WhenTable("Order");
        configuration.IgnoreRules.IgnoreColumn("IgnoredColumn.Id").WhenTable("Author");
      }
      else
        configuration.Types.Register(typeof(IgnoreRulesValidateRecreateModel.Customer).Assembly, typeof(IgnoreRulesValidateRecreateModel.Customer).Namespace);
      return Domain.Build(configuration);
    }
  }
}
