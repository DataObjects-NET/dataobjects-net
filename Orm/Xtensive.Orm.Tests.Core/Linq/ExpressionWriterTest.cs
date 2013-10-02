// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.02.09

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Core.Linq
{
  [TestFixture]
  public class ExpressionWriterTest
  {
    [Test]
    public void CombinedTest()
    {
      IEnumerable<string> enumerable = new[] {"A", "B", "C"};
      var queryable = enumerable.AsQueryable();

      int p = 1;

      Dump(from i in queryable where i.Length==p select i);
      Dump(from i in queryable select new {i.Length});
      Dump(from i in queryable select i.Length);
      Dump(from i in queryable select i.GetType()==typeof(IComparer<string>));
      var pair = new Pair<string>("A","B");
      Dump(from i in queryable select i==pair.First);
    }

    private void Dump(IQueryable query)
    {
      TestLog.Info("Default: {0}", query.Expression.ToString());
      TestLog.Info("C#:      {0}", query.Expression.ToString(true));
    }
  }
}