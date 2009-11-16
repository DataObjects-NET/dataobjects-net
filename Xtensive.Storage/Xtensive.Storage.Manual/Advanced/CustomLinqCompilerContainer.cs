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
}