// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.21

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  namespace SubqueryWithCalculatedFieldTestModel
  {
    [HierarchyRoot]
    public class Subqueried : Entity
    {
      [Field]
      [Key]
      public int Id { get; set; }

      [Field]
      public decimal Money { get; set; }

      [Field]
      public decimal Price { get; set; }

      [Field]
      public int Amount { get; set; }

      public decimal Total { get; set; }
    }

    [CompilerContainer(typeof (Expression))]
    public static class SubqueriedCompiler
    {
      private static readonly Expression<Func<Subqueried, decimal>> MyEntTextExpression = a => a.Price * a.Amount;

      [Compiler(typeof (Subqueried), "Total", TargetKind.PropertyGet)]
      public static Expression Total(Expression assignmentExpression)
      {
        return MyEntTextExpression.BindParameters(assignmentExpression);
      }

      public class SubqueryWithCalculatedFieldTest : AutoBuildTest
      {
        protected override Configuration.DomainConfiguration BuildConfiguration()
        {
          var configuration = base.BuildConfiguration();
          configuration.Types.Register(typeof (Subqueried).Assembly, typeof (Subqueried).Namespace);
          return configuration;
        }

        protected override void PopulateData()
        {
          using (var session = Domain.OpenSession())
          using (var tx = session.OpenTransaction()) {
            new Subqueried {Money = 5, Price = 4, Amount = 2};
            new Subqueried {Money = 3, Price = 2, Amount = 1};
            tx.Complete();
          }
        }

        [Test]
        public void AggregateWithCustomCompilerTest()
        {
          using (var session = Domain.OpenSession())
          using (var tx = session.OpenTransaction()) {
            var query =
              from item in session.Query.All<Subqueried>()
              select new {Item = item, FakeKey = 0}
              into i group i by i.FakeKey;

            var aggregateQuery =
              from item in query
              select new {
                SumTotal = item.Sum(b => b.Item.Total),
                SumMoney = item.Sum(b => b.Item.Money)
              };

            var result = aggregateQuery.ToArray()[0];

            Assert.That(result.SumTotal, Is.EqualTo(10m));
            Assert.That(result.SumMoney, Is.EqualTo(8m));
          }
        }

        [Test]
        public void AggregateWithoutCustomCompilerTest()
        {
          using (var session = Domain.OpenSession())
          using (var tx = session.OpenTransaction()) {
            var query =
              from item in session.Query.All<Subqueried>()
              select new {Item = item, FakeKey = 0}
              into i group i by i.FakeKey;

            var aggregateQuery =
              from item in query
              select new {
                SumTotal = item.Sum(b => b.Item.Amount * b.Item.Price),
                SumMoney = item.Sum(b => b.Item.Money)
              };

            var result = aggregateQuery.ToArray()[0];

            Assert.That(result.SumTotal, Is.EqualTo(10m));
            Assert.That(result.SumMoney, Is.EqualTo(8m));
          }
        }
      }
    }
  }
}