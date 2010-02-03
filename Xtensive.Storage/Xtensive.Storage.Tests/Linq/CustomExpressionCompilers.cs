// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.13

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Linq.CustomExpressionCompilersModel;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq.CustomExpressionCompilersModel
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
}

namespace Xtensive.Storage.Tests.Linq
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
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Fill();
          var expected1 = Query.All<Person>().AsEnumerable().OrderBy(p => p.Id).Select(p => p.Fullname).ToList();
          Assert.Greater(expected1.Count, 0);
          var fullNames1 = Query.All<Person>().OrderBy(p => p.Id).Select(p => p.Fullname).ToList();
          Assert.IsTrue(expected1.SequenceEqual(fullNames1));

          var expected2 = Query.All<Person>().AsEnumerable().OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. ")).ToList();
          var fullNames2 = Query.All<Person>().OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. ")).ToList();
          Assert.IsTrue(expected2.SequenceEqual(fullNames2));
          // Rollback
        }
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