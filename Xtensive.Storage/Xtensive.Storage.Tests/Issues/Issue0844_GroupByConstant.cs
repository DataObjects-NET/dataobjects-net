﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues_Issue0844_GroupByConstant;

namespace Xtensive.Storage.Tests.Issues_Issue0844_GroupByConstant
{
  [HierarchyRoot]
  public class Line : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Length { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0844_GroupByConstant : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Line).Assembly, typeof (Line).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transactionn = Transaction.Open()) {
          var query = Query.All<Line>().GroupBy(x => 0);
          var result = query.ToList();
        }
      }
    }
  }
}