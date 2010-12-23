// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.18

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Linq.Dto;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  namespace Dto
  {
    public enum BudgetType
    {
      Default,
      Regional,
      State
    }

    [HierarchyRoot]
    public class Manager
      : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field,Association(PairTo = "Manager")]
      public EntitySet<Person> Persons { get; private set; }
    }

    [HierarchyRoot]
    public class Person
     : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public Manager Manager { get; set; }

      [Field]
      public int? Tag { get; set; }

      [Field]
      public BudgetType? BudgetType { get; set; }
    }

    public class ManagerDto
    {
      public int Id { get; set; }
      public int? FirstPersonId { get; set; }
    }

    public class PersonDto
    {
      public int Id { get; set; }
      public int? ManagerId { get; set; }
      public string Name { get; set; }
      public int? Tag { get; set; }
      public BudgetType? BudgetType { get; set; }
      public string Description { get; set; }
    }
  }

  [Serializable]
  public class SelectDtoTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var alex = new Person() { Name = "Alex" };
        var query = Query.All<Person>()
          .Select(p => new PersonDto {Name = p.Name})
          .Where(personDto => personDto.Name == "Alex");
        var result = query.ToList();
        var firstPerson = query.SingleOrDefault();
        Assert.IsTrue(firstPerson.Name == "Alex");
      }
    }

    [Test]
    public void WhereTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var person5 = new Person() {Name = "John", Tag = 5, BudgetType = BudgetType.Regional};
        var personEmpty = new Person() {Name = "John"};

        var count = Query.All<Person>()
          .Select(p => new PersonDto() { Id = p.Id, Name = p.Name, Tag = p.Tag, BudgetType = p.BudgetType})
          .Where(x => x.Tag == 5 && x.BudgetType == BudgetType.Regional).OrderBy(dto => dto.Id).Count();
        Assert.AreEqual(1, count);
      }
    }

    [Test]
    public void EnumTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        BudgetType budgetType = BudgetType.Regional;
        BudgetType budgetTypeNotNullable = BudgetType.Regional;
        BudgetType? budgetTypeNullable = BudgetType.Regional;
        Expression<Func<Person, bool>> filterExpression = p => p.BudgetType == budgetType;
        Expression<Func<Person, bool>> filterNullableExpression = p => p.BudgetType == budgetTypeNullable;
        Expression<Func<Person, bool>> filterNotNullableExpression = p => budgetTypeNotNullable == budgetType;
        Expression<Func<Person, BudgetType?>> propertyExpression = p => p.BudgetType;
        var valueExpression = Expression.Convert(Expression.Constant(budgetType), typeof(BudgetType?));
        var body = Expression.Convert(propertyExpression.Body, typeof(BudgetType?));
        Expression<Func<Person, bool>> customFilterExpression = Expression.Lambda<Func<Person, bool>>(
          Expression.Equal(body, valueExpression),
          propertyExpression.Parameters);
        var persons = Query.All<Person>().Where(filterExpression).ToList();
        var customPersons = Query.All<Person>().Where(customFilterExpression).ToList();
        var func = customFilterExpression.Compile();
        func(new Person() {BudgetType = BudgetType.Regional});
      }
    }

    [Test]
    public void SelectNullableTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var manager1 = new Manager() {Name = "M0"};
        var manager2 = new Manager() {Name = "M0"};
        new Person() { Name = "A", Manager = manager1};
        new Person() { Name = "B", Manager = manager1};
        new Person() { Name = "C", Manager = manager2};
        new Person() { Name = "D", Manager = manager2};
        new Person() { Name = "E" };
        new Person() { Name = "F" };
        
        var query = Query.All<Person>()
          .Select(p => new PersonDto {
            Id = p.Id, 
            Name = p.Name, 
            ManagerId = p.Manager != null ? (int?)p.Manager.Id : null});
        var result = query.ToList();
        Assert.AreEqual(6, result.Count);
      }
    }

    [Test]
    public void GroupByTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        new Person() {Name = "A", BudgetType = BudgetType.Default};
        new Person() {Name = "B", BudgetType = BudgetType.Default};
        new Person() {Name = "C", BudgetType = BudgetType.Regional};
        new Person() {Name = "D", BudgetType = BudgetType.State};
        new Person() {Name = "E"};
        new Person() {Name = "F"};

        var types = Query.All<Person>()
          .Select(p => p.BudgetType)
          .Distinct()
          .ToList();
        Assert.AreEqual(4, types.Count);

        var groups = Query.All<Person>()
          .GroupBy(p => p.BudgetType)
          .Select(g => new {g.Key, Count = g.Count()})
          .ToList();
        Assert.AreEqual(4, groups.Count);

        var arrays = Query.All<Person>()
          .Select(p => new PersonDto(){Id = p.Id, Name = p.Name, BudgetType = p.BudgetType})
          .GroupBy(c => c.BudgetType)
          .OrderBy(g => g.Key)
          .Select(g => new[] {(object) g.Key, (object) g.Count()})
          .ToList();
        Assert.AreEqual(4, arrays.Count);
      }
    }

    [Test]
    public void MethodProjectionTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        new Person() { Name = "A", BudgetType = BudgetType.Default };
        new Person() { Name = "B", BudgetType = BudgetType.Default };
        new Person() { Name = "C", BudgetType = BudgetType.Regional };
        new Person() { Name = "D", BudgetType = BudgetType.State };
        new Person() { Name = "E" };
        new Person() { Name = "F" };
        new Person() { Name = null };
        new Person() { Name = null };

        var selectedMethod = Query.All<Person>()
          .Select(p => new PersonDto() {Id = p.Id, Name = p.Name, Description = GetDescription(p)})
          .OrderBy(x => x.Name)
          .ToList();

        var selectedMethod2 = Query.All<Person>()
         .Select(p => new PersonDto() { Id = p.Id, Name = p.Name, Description = p.Name ?? GetDescription(p) })
         .Where(x => x.Name == null)
         .ToList();
      }
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var manager = new Manager() {
          Name = "Manager"
        };
        var looser = new Manager() {
          Name = "Looser"
        };
        new Person() { Name = "A", BudgetType = BudgetType.Default, Manager = manager};
        new Person() { Name = "B", BudgetType = BudgetType.Default, Manager = manager};
        new Person() { Name = "C", BudgetType = BudgetType.Regional, Manager = manager };
        new Person() { Name = "D", BudgetType = BudgetType.State, Manager = manager };
        new Person() { Name = "E" };
        new Person() { Name = "F" };

        var list = Query.All<Manager>()
          .Select(m => new { Entity = m, FirstPerson = m.Persons.FirstOrDefault() })
          .Select(g => new ManagerDto() {
              Id = g.Entity.Id,
              FirstPersonId = g.FirstPerson != null ? (int?)g.FirstPerson.Id : null,
            })
          .ToList();
      }
    }

    public string GetDescription(Person p)
    {
      if (!p.BudgetType.HasValue)
        return "Empty Budget";
      else
        return p.BudgetType + " Budget";
    }
  }
}