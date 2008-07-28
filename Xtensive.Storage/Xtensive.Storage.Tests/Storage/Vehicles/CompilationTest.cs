// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.24

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class CompilationTest
  {
    [Test]
    public void ExpressionParseTest()
    {
      Tuple tuple = Tuple.Create(1, 2, 3);
      Expression<Func<Tuple, bool>> expr = (t => t.GetValue<int>(1) == 2 && (t.GetValue<int>(2) == 3 || t.GetValue<int>(0) > -1));
      Console.Out.WriteLine(expr);
    }
  }
}