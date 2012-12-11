// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.06

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.GroupByTwoEntitiesWithSameColumnsTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace GroupByTwoEntitiesWithSameColumnsTestModel
  {
    [HierarchyRoot]
    public class GroupedEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Ref1 Ref1 { get; set; }

      [Field]
      public Ref2 Ref2 { get; set; }
    }

    [HierarchyRoot]
    public class Ref1 : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }

    [HierarchyRoot]
    public class Ref2 : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }
  }

  public class GroupByTwoEntitiesWithSameColumnsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (GroupedEntity));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query =
          from e in session.Query.All<GroupedEntity>()
          group e by new {e.Ref1, e.Ref2, V1 = e.Ref1.Value, V2 = e.Ref2.Value}
          into g
          select g;
        var result = query.ToList();
        tx.Complete();
      }
    }
  }
}