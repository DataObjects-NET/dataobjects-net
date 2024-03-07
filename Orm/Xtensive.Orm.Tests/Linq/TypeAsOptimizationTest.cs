﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.08.28

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.TypeAsOptimizationTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace TypeAsOptimizationTestModel
  {
    [HierarchyRoot]
    public class Parent : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class Child1 : Parent
    {
      [Field]
      public int IntValue { get; set; }
    }

    public class Child2 : Child1
    {
      [Field]
      public string StringValue { get; set; }
    }

    public class OtherChild : Parent
    {
      [Field]
      public DateTime DateValue { get; set; }
    }

    public class UniversalResult
    {
      public Child2 Child2 { get; set; }
      public OtherChild Other { get; set; }
    }
  }

  public class TypeAsOptimizationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Parent).Assembly, typeof (Parent).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Child1 {Name = "Child1", IntValue = 10};
        new Child2 {Name = "Child2", StringValue = "Hello"};
        var result = session.Query.All<Parent>()
          .Select(p => new UniversalResult {Child2 = p as Child2, Other = p as OtherChild})
          .Where(i => i.Child2.IntValue==10)
          .ToList();
        tx.Complete();
      }
    }
  }
}