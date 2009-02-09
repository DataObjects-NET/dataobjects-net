// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.02.09

using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public class ExpressionWriterTest
  {
    [Test]
    public void CombinedTest()
    {
      IEnumerable<string> enumerable = new[] {"A", "B", "C"};
      var queryable = enumerable.AsQueryable();

      Dump(from i in queryable select i.Length);
      Dump(from i in queryable select new {i.Length});
    }

    private void Dump(IQueryable query)
    {
      Log.Info("Default: {0}", query.Expression.ToString());
      Log.Info("C#:      {0}", query.Expression.ToString(true));
    }
  }
}