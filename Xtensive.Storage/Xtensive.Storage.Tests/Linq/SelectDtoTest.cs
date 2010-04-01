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
using Xtensive.Storage.Tests.Linq.Dto;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  namespace Dto
  {
    public enum BudgetType
    {
      Default,
      Regional
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
      public int? Tag { get; set; }
      [Field]
      public BudgetType? BudgetType { get; set; }
    }

    public class PersonDto
    {
      public int Id { get; set; }
      public string Name { get; set; }
      public int? Tag { get; set; }
      public BudgetType? BudgetType { get; set; }
    }
  }

  [Serializable]
  public class SelectDtoTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Person));
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
        t.Complete();
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
        t.Complete();
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

        t.Complete();
      }
    }
  }
}