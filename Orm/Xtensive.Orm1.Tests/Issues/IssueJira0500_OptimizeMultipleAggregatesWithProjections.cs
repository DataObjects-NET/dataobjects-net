// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Issues.IssueJira0500_OptimizeMultipleAggregatesWithProjectionsModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0500_OptimizeMultipleAggregatesWithProjectionsModel
  {
    [HierarchyRoot]
    public class ReferencedEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public decimal Value { get; set; }
    }

    [HierarchyRoot]
    public class AggregatedEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public int Group { get; set; }

      [Field]
      public decimal Value { get; set; }

      [Field]
      public ReferencedEntity Ref { get; set; }
    }

    [HierarchyRoot]
    public class TwoSumEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public int Group { get; set; }

      [Field]
      public bool Condition { get; set; }

      [Field]
      public decimal Value { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0500_OptimizeMultipleAggregatesWithProjections : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (AggregatedEntity).Assembly, typeof (AggregatedEntity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ref1 = new ReferencedEntity {Value = 2};
        var ref2 = new ReferencedEntity {Value = 3};
        var ref3 = new ReferencedEntity {Value = 4};
        new AggregatedEntity {Value = 0, Ref = ref1};
        new AggregatedEntity {Value = 1, Ref = ref2};
        new AggregatedEntity {Value = 2, Ref = ref3};
        new AggregatedEntity {Value = 3};
        new TwoSumEntity {Condition = true, Value = 1};
        new TwoSumEntity {Condition = true, Value = 2};
        new TwoSumEntity {Condition = true, Value = 3};
        new TwoSumEntity {Condition = false, Value = 2};
        new TwoSumEntity {Condition = false, Value = 3};
        new TwoSumEntity {Condition = false, Value = 4};
        tx.Complete();
      }
    }

    [Test]
    public void GroupBySimpleTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<AggregatedEntity>()
          .GroupBy(e => e.Group)
          .Select(g => new {
            SumValue = g.Sum(e => e.Value),
            SumRefValue = g.Sum(e => e.Ref.Value),
          });
        Test(session, query, e => e.SumValue, e => e.SumRefValue);
        tx.Complete();
      }
    }

    [Test]
    public void GroupByWithElementSelectorTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<AggregatedEntity>()
          .GroupBy(e => e.Group, e => new {Element = e})
          .Select(g => new {
            SumValue = g.Sum(e => e.Element.Value),
            SumRefValue = g.Sum(e => e.Element.Ref.Value),
          });
        Test(session, query, e => e.SumValue, e => e.SumRefValue);
        tx.Complete();
      }
    }

    [Test]
    public void GroupByWithResultSelectorTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<AggregatedEntity>()
          .GroupBy(e => e.Group, (key, elements) => new {
            SumValue = elements.Sum(e => e.Value),
            SumRefValue = elements.Sum(e => e.Ref.Value),
          });
        Test(session, query, e => e.SumValue, e => e.SumRefValue);
        tx.Complete();
      }
    }

    [Test]
    public void GroupByWithElementAndResultSelectorsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<AggregatedEntity>()
          .GroupBy(e => e.Group, e => new {Element = e}, (key, elements) => new {
            SumValue = elements.Sum(e => e.Element.Value),
            SumRefValue = elements.Sum(e => e.Element.Ref.Value),
          });
        Test(session, query, e => e.SumValue, e => e.SumRefValue);
        tx.Complete();
      }
    }

    [Test]
    public void GroupByConditionalTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<TwoSumEntity>()
          .GroupBy(e => e.Group)
          .Select(item => new {
            Sum1 = item.Sum(e => e.Condition ? e.Value : 0),
            Sum2 = item.Sum(e => !e.Condition ? e.Value : 0),
          });
        Test(session, query, e => e.Sum1, e => e.Sum2);
      }
    }

    private void Test<T>(Session session, IQueryable<T> query,
      Func<T, decimal> firstSumSelector, Func<T, decimal> secondSumSelector)
    {
      var queryFormatter = session.Services.Demand<QueryFormatter>();
      var queryString = queryFormatter.ToSqlString(query);
      var firstSelectPosition = queryString.IndexOf("select",
        StringComparison.InvariantCultureIgnoreCase);
      Assert.That(firstSelectPosition, Is.GreaterThanOrEqualTo(0));
      var secondSelectPosition = queryString.IndexOf("select", firstSelectPosition + 1,
        StringComparison.InvariantCultureIgnoreCase);
      Assert.That(secondSelectPosition, Is.LessThan(0));
      var result = query.ToList();
      Assert.That(result.Count, Is.EqualTo(1));
      var sumValue = firstSumSelector.Invoke(result[0]);
      var sumRefValue = secondSumSelector.Invoke(result[0]);
      Assert.That(sumValue, Is.EqualTo(6m));
      Assert.That(sumRefValue, Is.EqualTo(9m));
    }
  }
}