// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.16

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Manual.Advanced.CustomLinqCompiler
{
  [CompilerContainer(typeof (Expression))]
  public static class CustomLinqCompilerContainer
  {
    [Compiler(typeof (Person), "FullName", TargetKind.PropertyGet)]
    public static Expression FullName(Expression personExpression)
    {
      var spaceExpression = Expression.Constant(" ");
      var firstNameExpression = Expression.Property(personExpression, "FirstName");
      var lastNameExpression = Expression.Property(personExpression, "lastName");
      var methodInfo = typeof (string).GetMethod("Concat", new[] {typeof (string), typeof (string), typeof (string)});
      var concatExpression = Expression.Call(Expression.Constant(null, typeof(string)), methodInfo, firstNameExpression, spaceExpression, lastNameExpression);
      return concatExpression;
    }

    [Compiler(typeof (Person), "FullName2", TargetKind.PropertyGet)]
    public static Expression FullName2(Expression personExpression)
    {
      // FullName logic. As of "ex" expression type exactly specified, 
      // C# compiler allows to use "Person" properties.
      Expression<Func<Person, string>> ex = person => person.FirstName + " " + person.LastName;

      // Binding lambda parameters replaces parameter usage in labda and returns labda body
      // after binding expression become to something like:
      // personExpression.FirstName + " " + personExpression.LastName
      return ex.BindParameters(personExpression);
    }

    [Compiler(typeof (Person), "AddPrefix", TargetKind.Method)]
    public static Expression AddPrefix(Expression personExpression, Expression prefixExpression)
    {
      Expression<Func<Person, string, string>> ex =  (person, prefix) => prefix + person.LastName;
      return ex.BindParameters(personExpression, prefixExpression);
    }
  }
}