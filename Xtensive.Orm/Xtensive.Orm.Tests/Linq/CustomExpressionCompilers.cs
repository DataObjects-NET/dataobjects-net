// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.13

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.CustomExpressionCompilersModel;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq.CustomExpressionCompilersModel
{
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

    public string Fullname
    {
      get { return string.Format("{0} {1}", FirstName, LastName); }
    }

    public string AddPrefix(string prefix)
    {
      return string.Format("{0}{1}", prefix, LastName);
    }
  }

  [HierarchyRoot]
  public class Assignment : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    [Field]
    public bool Active { get; set; }
    [Field]
    public DateTime Start { get; set; }
    [Field]
    public DateTime? End { get; set; }
    public bool Current
    {
      get { return Active && Start <= DateTime.Now && (End == null || End.Value >= DateTime.Now); }
    }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  [CompilerContainer(typeof (Expression))]
  internal static class CustomLinqCompilerContainer
  {
    [Compiler(typeof (Person), "Fullname", TargetKind.PropertyGet)]
    public static Expression FullName(Expression personExpression)
    {
      Expression<Func<Person, string>> ex = person => person.FirstName + " " + person.LastName;
      return ex.BindParameters(personExpression);
    }

    [Compiler(typeof (Person), "AddPrefix", TargetKind.Method)]
    public static Expression AddPrefix(Expression personExpression, Expression prefixExpression)
    {
      Expression<Func<Person, string, string>> ex =  (person, prefix) => prefix + person.LastName;
      return ex.BindParameters(personExpression, prefixExpression);
    }

    [Compiler(typeof(Assignment), "Current", TargetKind.PropertyGet)]
    public static Expression Current(Expression assignmentExpression)
    {
      Expression<Func<Assignment, bool>> ex = a => a.Active && (a.Start <= DateTime.Now) && (a.End == null || a.End.Value >= DateTime.Now);
      return ex.BindParameters(assignmentExpression);
    }
  }

  public class CustomExpressionCompilers : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      config.Types.Register(typeof (CustomLinqCompilerContainer));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Fill();
          var expected1 = session.Query.All<Person>().AsEnumerable().OrderBy(p => p.Id).Select(p => p.Fullname).ToList();
          Assert.Greater(expected1.Count, 0);
          var fullNames1 = session.Query.All<Person>().OrderBy(p => p.Id).Select(p => p.Fullname).ToList();
          Assert.IsTrue(expected1.SequenceEqual(fullNames1));

          var expected2 = session.Query.All<Person>().AsEnumerable().OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. ")).ToList();
          var fullNames2 = session.Query.All<Person>().OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. ")).ToList();
          Assert.IsTrue(expected2.SequenceEqual(fullNames2));
          // Rollback
        }
      }
    }

    [Test]
    public void AssignmentCurrentTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Assignment() {Active = true, Start = new DateTime(2009, 11, 23), End = null};
        new Assignment() {Active = false, Start = new DateTime(2009, 10, 3), End = null};
        new Assignment() {Active = false, Start = new DateTime(2020, 01, 10), End = new DateTime(2044, 12, 3)};
        new Assignment() {Active = true, Start = new DateTime(2026, 01, 10), End = new DateTime(2045, 11, 3)};
        new Assignment() {Active = true, Start = new DateTime(2010, 01, 10), End = new DateTime(2035, 11, 3)};

        var currentCount = session.Query.All<Assignment>().Count(a => a.Current);
        Assert.AreEqual(2, currentCount);
        // Rollback
      }
    }

    private void Fill()
    {
      new Person {FirstName = "Ivan", LastName = "Semenov"};
      new Person {FirstName = "John", LastName = "Smith"};
      new Person {FirstName = "Andrew", LastName = "Politkovsky"};
    }
  }
}