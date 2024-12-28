// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.18

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.Dto;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq
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
    public class Manager : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field,Association(PairTo = "Manager")]
      public EntitySet<Person> Persons { get; private set; }

      public Manager(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class Person : Entity
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

      public Person(Session session)
        : base(session)
      {
      }
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
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var alex = new Person(session) { Name = "Alex" };
        var query = session.Query.All<Person>()
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
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var person5 = new Person(session) {Name = "John", Tag = 5, BudgetType = BudgetType.Regional};
        var personEmpty = new Person(session) {Name = "John"};

        var count = session.Query.All<Person>()
          .Select(p => new PersonDto() { Id = p.Id, Name = p.Name, Tag = p.Tag, BudgetType = p.BudgetType})
          .Where(x => x.Tag == 5 && x.BudgetType == BudgetType.Regional).OrderBy(dto => dto.Id).Count();
        Assert.AreEqual(1, count);
      }
    }

    [Test]
    public void EnumTest()
    {
      BudgetType budgetType = BudgetType.Regional;
      BudgetType budgetTypeNotNullable = BudgetType.State;
      BudgetType? budgetTypeNullable = BudgetType.Default;
      BudgetType? nullBudgetType = null;

      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new Person(session) { Name = "A", BudgetType = BudgetType.Default };
        _ = new Person(session) { Name = "B", BudgetType = BudgetType.Default };
        _ = new Person(session) { Name = "C", BudgetType = BudgetType.Regional };
        _ = new Person(session) { Name = "D", BudgetType = BudgetType.State };
        _ = new Person(session) { Name = "E" };
        _ = new Person(session) { Name = "F" };
        _ = new Person(session) { Name = null };
        _ = new Person(session) { Name = null };

        session.SaveChanges();

        Expression<Func<Person, bool>> filterExpression = p => p.BudgetType == budgetType;
        var regionalBudgetPeople = session.Query.All<Person>().Where(filterExpression).ToList();
        Assert.That(regionalBudgetPeople.Count, Is.EqualTo(1));
        Assert.That(regionalBudgetPeople[0].Name, Is.EqualTo("C"));

        Expression<Func<Person, bool>> filterNotNullableExpression = p => p.BudgetType == budgetTypeNotNullable;
        var stateBudgetPeople = session.Query.All<Person>().Where(filterNotNullableExpression).ToList();
        Assert.That(stateBudgetPeople.Count, Is.EqualTo(1));
        Assert.That(stateBudgetPeople[0].Name, Is.EqualTo("D"));
       
        Expression<Func<Person, bool>> filterNullableExpression = p => p.BudgetType == budgetTypeNullable;
        var defaultBudgetPeople = session.Query.All<Person>().Where(filterNullableExpression).ToList();
        Assert.That(defaultBudgetPeople.Count, Is.EqualTo(2));
        Assert.That(defaultBudgetPeople[0].Name, Is.EqualTo("A").Or.EqualTo("B"));
        Assert.That(defaultBudgetPeople[1].Name, Is.EqualTo("A").Or.EqualTo("B"));

        Expression<Func<Person, bool>> filterNullExpression = p => p.BudgetType == nullBudgetType;
        var undefinedBudgetPeople = session.Query.All<Person>().Where(filterNullExpression).ToList();
        Assert.That(undefinedBudgetPeople.Count, Is.EqualTo(4));
        foreach (var person in undefinedBudgetPeople) {
          Assert.That(undefinedBudgetPeople[0].Name, Is.EqualTo("E").Or.EqualTo("F").Or.EqualTo(null));
        }

        Expression<Func<Person, BudgetType?>> propertyExpression = p => p.BudgetType;
        var valueExpression = Expression.Convert(Expression.Constant(budgetType), typeof(BudgetType?));
        var body = Expression.Convert(propertyExpression.Body, typeof(BudgetType?));
        var customFilterExpression = Expression.Lambda<Func<Person, bool>>(
          Expression.Equal(body, valueExpression),
          propertyExpression.Parameters);
        var customFilterExpressionResults = session.Query.All<Person>().Where(customFilterExpression).ToList();
        Assert.That(customFilterExpressionResults.OrderBy(p => p.Id).SequenceEqual(regionalBudgetPeople.OrderBy(p => p.Id)), Is.True);

        var func = customFilterExpression.Compile();
        Assert.That(func(new Person(session) {BudgetType = BudgetType.Regional}),Is.True);
      }
    }

    [Test]
    public void SelectNullableTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var manager1 = new Manager(session) {Name = "M0"};
        var manager2 = new Manager(session) {Name = "M0"};
        _ = new Person(session) { Name = "A", Manager = manager1};
        _ = new Person(session) { Name = "B", Manager = manager1};
        _ = new Person(session) { Name = "C", Manager = manager2};
        _ = new Person(session) { Name = "D", Manager = manager2};
        _ = new Person(session) { Name = "E" };
        _ = new Person(session) { Name = "F" };
        
        var query = session.Query.All<Person>()
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
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new Person(session) {Name = "A", BudgetType = BudgetType.Default};
        _ = new Person(session) {Name = "B", BudgetType = BudgetType.Default};
        _ = new Person(session) {Name = "C", BudgetType = BudgetType.Regional};
        _ = new Person(session) {Name = "D", BudgetType = BudgetType.State};
        _ = new Person(session) {Name = "E"};
        _ = new Person(session) {Name = "F"};

        var types = session.Query.All<Person>()
          .Select(p => p.BudgetType)
          .Distinct()
          .ToList();
        Assert.AreEqual(4, types.Count);

        var groups = session.Query.All<Person>()
          .GroupBy(p => p.BudgetType)
          .Select(g => new {g.Key, Count = g.Count()})
          .ToList();
        Assert.AreEqual(4, groups.Count);

        var arrays = session.Query.All<Person>()
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
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new Person(session) { Name = "A", BudgetType = BudgetType.Default };
        _ = new Person(session) { Name = "B", BudgetType = BudgetType.Default };
        _ = new Person(session) { Name = "C", BudgetType = BudgetType.Regional };
        _ = new Person(session) { Name = "D", BudgetType = BudgetType.State };
        _ = new Person(session) { Name = "E" };
        _ = new Person(session) { Name = "F" };
        _ = new Person(session) { Name = null };
        _ = new Person(session) { Name = null };

        var selectedMethod = session.Query.All<Person>()
          .Select(p => new PersonDto() {Id = p.Id, Name = p.Name, Description = GetDescription(p)})
          .OrderBy(x => x.Name)
          .ToList();

        Assert.That(selectedMethod.Count, Is.EqualTo(8));
        Assert.That(selectedMethod[0].Name, Is.Null);
        Assert.That(selectedMethod[1].Name, Is.Null);
        Assert.That(selectedMethod.Skip(2).Select(dto => dto.Name).SequenceEqual(new[] { "A", "B", "C", "D", "E", "F" }), Is.True);

        var selectedMethod2 = session.Query.All<Person>()
         .Select(p => new PersonDto() { Id = p.Id, Name = p.Name, Description = p.Name ?? GetDescription(p) })
         .Where(x => x.Name == null)
         .ToList();

        Assert.That(selectedMethod2.Count, Is.EqualTo(2));
        Assert.That(selectedMethod2[0].Name, Is.Null);
        Assert.That(selectedMethod2[1].Name, Is.Null);
      }
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var manager = new Manager(session) {
          Name = "Manager"
        };
        var looser = new Manager(session) {
          Name = "Looser"
        };
        _ = new Person(session) { Name = "A", BudgetType = BudgetType.Default, Manager = manager};
        _ = new Person(session) { Name = "B", BudgetType = BudgetType.Default, Manager = manager};
        _ = new Person(session) { Name = "C", BudgetType = BudgetType.Regional, Manager = manager };
        _ = new Person(session) { Name = "D", BudgetType = BudgetType.State, Manager = manager };
        _ = new Person(session) { Name = "E" };
        _ = new Person(session) { Name = "F" };

        var list = session.Query.All<Manager>()
          .Select(m => new { Entity = m, FirstPerson = m.Persons.FirstOrDefault() })
          .Select(g => new ManagerDto() {
              Id = g.Entity.Id,
              FirstPersonId = g.FirstPerson != null ? (int?)g.FirstPerson.Id : null,
            })
          .ToList();

        Assert.That(list.Count, Is.EqualTo(2));
        foreach(var dto in list) {
          var constraint = (dto.Id == looser.Id)
            ? Is.Null
            : Is.Not.Null;
          Assert.That(dto.FirstPersonId, constraint);
        }
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