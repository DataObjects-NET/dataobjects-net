// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.16

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Advanced.CustomLinqCompiler
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public DateTime BirthDay { get; set; }

    public string FullName
    {
      get { return $"{FirstName} {LastName}"; }
    }

    public string FullName2
    {
      get { return $"{FirstName} {LastName}"; }
    }

    public string AddPrefix(string prefix)
    {
      return $"{prefix}{LastName}";
    }

    public Person(Session session)
      : base(session)
    {}
  }

  #endregion

  #region Compiler container

  [CompilerContainer(typeof (Expression))]
  public static class CustomLinqCompilerContainer
  {
    [Compiler(typeof (Person), "FullName", TargetKind.PropertyGet)]
    public static Expression FullName(Expression personExpression)
    {
      var spaceExpression = Expression.Constant(" ");
      var firstNameExpression = Expression.Property(personExpression, "FirstName");
      var lastNameExpression = Expression.Property(personExpression, "lastName");
      var methodInfo = typeof (string).GetMethod("Concat", 
        new[] {typeof (string), typeof (string), typeof (string)} );
      var concatExpression = Expression.Call(
        methodInfo, 
        firstNameExpression, 
        spaceExpression, 
        lastNameExpression);
      return concatExpression;
    }

    [Compiler(typeof (Person), "FullName2", TargetKind.PropertyGet)]
    public static Expression FullName2(Expression personExpression)
    {
      // FullName logic. Since "ex" expression type exactly specified, 
      // C# compiler allows to use "Person" properties.
      Expression<Func<Person, string>> ex = 
        person => person.FirstName + " " + person.LastName;

      // Binding lambda parameters replaces parameter usage in lambda,
      // so that binding expression becomes like this:
      // personExpression.FirstName + " " + personExpression.LastName
      return ex.BindParameters(personExpression);
    }

    [Compiler(typeof (Person), "AddPrefix", TargetKind.Method)]
    public static Expression AddPrefix(Expression personExpression, Expression prefixExpression)
    {
      Expression<Func<Person, string, string>> ex = 
        (person, prefix) => prefix + person.LastName;
      return ex.BindParameters(personExpression, prefixExpression);
    }
  }

  #endregion

  [TestFixture]
  public class CustomLinqCompilerTest
  {
    private Domain existingDomain;

    [Test]
    public void PropertyTest()
    {
      var domain = GetDomain();
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var expectedFullNames = session.Query.All<Person>().AsEnumerable()
            .OrderBy(p => p.Id)
            .Select(p => p.FullName);
          Assert.Greater(expectedFullNames.Count(), 0);
          var fullNames = session.Query.All<Person>()
            .OrderBy(p => p.Id)
            .Select(p => p.FullName);
          Assert.IsTrue(expectedFullNames.SequenceEqual(fullNames));
        }
      }
    }

    [Test]
    public void Property2Test()
    {
      var domain = GetDomain();
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var expectedFullNames = session.Query.All<Person>().AsEnumerable()
            .OrderBy(p => p.Id)
            .Select(p => p.FullName2);
          Assert.Greater(expectedFullNames.Count(), 0);
          var fullNames = session.Query.All<Person>()
            .OrderBy(p => p.Id)
            .Select(p => p.FullName2);
          Assert.IsTrue(expectedFullNames.SequenceEqual(fullNames));
        }
      }
    }

    [Test]
    public void MethodTest()
    {
      var domain = GetDomain();
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var expectedStrings = session.Query.All<Person>().AsEnumerable()
            .OrderBy(p => p.Id)
            .Select(p => p.AddPrefix("Mr. "));
          var resultStrings = session.Query.All<Person>()
            .OrderBy(p => p.Id)
            .Select(p => p.AddPrefix("Mr. "));
          Assert.IsTrue(expectedStrings.SequenceEqual(resultStrings));
        }
      }
    }

    [Test]
    public void Method2Test()
    {
      var domain = GetDomain();
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var expectedStrings = session.Query.All<Person>().AsEnumerable()
            .OrderBy(p => p.Id)
            .Select(p => p.AddPrefix(p.Id.ToString()));
          var resultStrings = session.Query.All<Person>()
            .OrderBy(p => p.Id)
            .Select(p => p.AddPrefix(p.Id.ToString()));
          Assert.IsTrue(expectedStrings.SequenceEqual(resultStrings));
        }
      }
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
        config.UpgradeMode = DomainUpgradeMode.Recreate;
        config.Types.Register(typeof (Person).Assembly, typeof(Person).Namespace);
        config.Types.Register(typeof (CustomLinqCompilerContainer));

        var domain = Domain.Build(config);
        using (var session = domain.OpenSession()) {
          using (var transactionScope = session.OpenTransaction()) {
            // Creating initial content
            new Person(session) {FirstName = "Ivan", LastName = "Semenov"};
            new Person(session) {FirstName = "John", LastName = "Smith"};
            new Person(session) {FirstName = "Andrew", LastName = "Politkovsky"};

            transactionScope.Complete();
          }
        }
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}