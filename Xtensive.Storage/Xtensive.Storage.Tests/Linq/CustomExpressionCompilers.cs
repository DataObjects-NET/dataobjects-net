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
  public static class CustomLinqCompilerContainer
  {
    [Compiler(typeof (Person), "Fullname", TargetKind.PropertyGet)]
    public static Expression FullName(Expression _this)
    {
      Expression<Func<Person, string>> ex = person => person.FirstName + " " + person.LastName;
      return BindLambdaParameters(ex, _this);
    }

    [Compiler(typeof (Person), "AddPrefix", TargetKind.Method)]
    public static Expression AddPrefix(Expression _this, Expression name)
    {
      Expression<Func<Person, string, string>> ex =  (person, prefix) => prefix + person.LastName;
      return BindLambdaParameters(ex, _this, name);
    }

    private static Expression BindLambdaParameters(LambdaExpression _this, params Expression[] expressions)
    {
      if (_this.Parameters.Count!=expressions.Length)
        throw new InvalidOperationException("parameters count incorrect");
      var parameters = new Expression[expressions.Length];
      for (int i = 0; i < _this.Parameters.Count; i++) {
        if (_this.Parameters[i].Type.IsAssignableFrom(expressions[i].Type))
          parameters[i] = _this.Parameters[i].Type==expressions[i].Type
            ? expressions[i]
            : Expression.Convert(expressions[i], _this.Parameters[i].Type);
        else
          throw new InvalidOperationException("type incorrect");
      }
      return ExpressionReplacer.ReplaceAll(_this.Body, _this.Parameters.ToArray(), parameters);
    }
  }

  public class CustomExpressionCompilers : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      config.CompilerContainers.Register(typeof (CustomLinqCompilerContainer));
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Fill();
          var expected1 = Query<Person>.All.AsEnumerable().OrderBy(p => p.Id).Select(p => p.Fullname).ToList();
          Assert.Greater(expected1.Count, 0);
          var fullNames1 = Query<Person>.All.OrderBy(p => p.Id).Select(p => p.Fullname).ToList();
          Assert.IsTrue(expected1.SequenceEqual(fullNames1));

          var expected2 = Query<Person>.All.AsEnumerable().OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. ")).ToList();
          var fullNames2 = Query<Person>.All.OrderBy(p => p.Id).Select(p => p.AddPrefix("Mr. ")).ToList();
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