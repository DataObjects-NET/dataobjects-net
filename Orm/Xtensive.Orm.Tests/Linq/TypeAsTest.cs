// Copyright (C) 2019 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2018.01.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.TypeAsTestModels;

namespace Xtensive.Orm.Tests.Linq
{
  public class TypeAsTest : AutoBuildTest
  {
    #region Nested types

    public class CustomExpressionReplacer : Xtensive.Linq.ExpressionVisitor
    {
      private readonly Func<Expression, Func<Expression, Expression>, Expression> visit;

      public static Expression Visit(Expression expression, Func<Expression, Func<Expression, Expression>, Expression> visit) =>
        new CustomExpressionReplacer(visit).Visit(expression);

      protected override Expression Visit(Expression exp) => visit(exp, base.Visit);

      private CustomExpressionReplacer(Func<Expression, Func<Expression, Expression>, Expression> visit)
      {
        this.visit = visit;
      }
    }

    private sealed class ComparisonComparer<T> : Comparer<T>
    {
      private readonly Comparison<T> comparison;

      public static new Comparer<T> Create(Comparison<T> comparison) =>
        comparison == null ? throw new ArgumentNullException("comparison") : new ComparisonComparer<T>(comparison);

      public override int Compare(T x, T y) => comparison(x, y);

      private ComparisonComparer(Comparison<T> comparison)
      {
        this.comparison = comparison;
      }
    }
    #endregion

    [Test]
    public void Test1()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      using(var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        QueryExpressionTest(
          () => session.Query.All<TestEntity1>().SelectMany(
            x => x.EntitySet.SelectMany(
              y => y.EntitySet.Select(
                z => (x.Value1 as TestEntity3).EntitySet.Any()))));
      }
    }

    [Test]
    public void Test2()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        QueryExpressionTest(
          () => session.Query.All<TestEntity1>().SelectMany(
            x => x.EntitySet.SelectMany(
              y => y.EntitySet.Select(
                z => (y.Value1 as TestEntity3).EntitySet.Any()))));
      }
    }

    [Test]
    public void Test3()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Apply);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        QueryExpressionTest(
          () =>
            session.Query.All<TestEntity1>().SelectMany(
              x => x.EntitySet.SelectMany(y => y.EntitySet.Select(z => (z.Value1 as TestEntity3).EntitySet.Any()))));
      }
    }

    [Test]
    public void Test4()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        QueryExpressionTest(
          () =>
            session.Query.All<TestEntity2>()
              .Where(x => ((x.Value1 as TestEntity3).Value1 as TestEntity3).EntitySet.Any()).Select(x => x.Id2));
      }
    }

    [Test]
    public void Test5()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        QueryExpressionTest(
          () => session.Query.All<TestEntity2>().Where(
            x => x.EntitySet.Any(y => ((x.Value1 as TestEntity3).Value1 as TestEntity3).EntitySet.Any())));
      }
    }

    private void QueryExpressionTest<TResult>(Expression<Func<TResult>> queryExpression)
    {
      var result1 = RewriteQueryExpressionAndInvoke(queryExpression, false);
      var result2 = RewriteQueryExpressionAndInvoke(queryExpression, true);

      if (!(result1 is IEnumerable) || !(result2 is IEnumerable)) {
        Assert.That(result1, Is.EqualTo(result2));
        return;
      }

      var result1Array = ((IEnumerable) result1).Cast<object>().ToArray();
      var result2Array = ((IEnumerable) result2).Cast<object>().ToArray();

      Assert.That(result1Array.SequenceEqual(result2Array));
    }

    private TResult RewriteQueryExpressionAndInvoke<TResult>(Expression<Func<TResult>> expression, bool asEnumerable)
    {
      var orderByMethod = (asEnumerable ? typeof(Enumerable) : typeof(Queryable)).GetMethods()
        .Single(x => x.Name == "OrderBy" && x.GetParameters().Length == (asEnumerable ? 3 : 2));
      var toArrayMethod = typeof(Enumerable).GetMethod("ToArray");
      var asQueryableMethod = typeof(Queryable).GetMethods().Single(x => x.Name == "AsQueryable" && x.IsGenericMethod);
      var keyPropertyInfo = typeof(IEntity).GetProperty("Key");

      expression = ((Expression<Func<TResult>>) CustomExpressionReplacer.Visit(
        expression,
        (e, visit) => {
          var result = (e = visit(e));

          if (result != null && typeof(IQueryable<IEntity>).IsAssignableFrom(result.Type)) {
            var isOrderedQueryable = typeof(IOrderedQueryable).IsAssignableFrom(result.Type);
            var entityType = result.Type.GetGenericArguments().Single();

            if (asEnumerable)
              result = Expression.Call(toArrayMethod.MakeGenericMethod(entityType), result);

            if (!isOrderedQueryable) {
              var keyParameter = Expression.Parameter(entityType);
              var keyProperty = Expression.Property(keyParameter, keyPropertyInfo);
              var orderByMethodGeneric = orderByMethod.MakeGenericMethod(entityType, keyPropertyInfo.PropertyType);
              var parameters = orderByMethodGeneric.GetParameters();
              var keySelectorType = asEnumerable
                ? parameters[1].ParameterType
                : parameters[1].ParameterType.GetGenericArguments().Single();
              var keySelector = (Expression) Expression.Lambda(keySelectorType, keyProperty, keyParameter);

              if (asEnumerable) {
                keySelector = Expression.Constant(((LambdaExpression) keySelector).Compile());
                var comparer = Expression.Constant(
                  ComparisonComparer<Key>.Create(
                    (k1, k2) => Comparer.Default.Compare(k1.Value.GetValue(0), k2.Value.GetValue(0))));
                result = Expression.Call(orderByMethodGeneric, result, keySelector, comparer);
                result = Expression.Call(toArrayMethod.MakeGenericMethod(entityType), result);
              }
              else {
                result = Expression.Call(orderByMethodGeneric, result, keySelector);
              }
            }

            if (asEnumerable) {
              result = Expression.Call(asQueryableMethod.MakeGenericMethod(entityType), result);
            }
          }

          if (asEnumerable && (e is MemberExpression || e is MethodCallExpression)) {
            Expression obj;

            var methodCall = e as MethodCallExpression;
            if (methodCall != null) {
              obj = methodCall.Object ?? methodCall.Arguments.FirstOrDefault();
            }
            else {
              obj = ((MemberExpression) e).Expression;
            }

            if (obj != null && (typeof(IQueryable<IEntity>).IsAssignableFrom(obj.Type)
                              || typeof(IEntity).IsAssignableFrom(obj.Type)
                              || typeof(Structure).IsAssignableFrom(obj.Type))) {
              result = Expression.Condition(
                Expression.Equal(obj, Expression.Constant(null, obj.Type)),
                Expression.Default(result.Type),
                result);
              return result;
            }
          }

          return result;
        }));

      return expression.Compile()();
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity1a = new TestEntity1(1);

        var entity2a = new TestEntity2(2);
        var entity2b = new TestEntity2(3);
        var entity2c = new TestEntity2(4);
        var entity2d = new TestEntity2(5);

        var entity3a = new TestEntity3(6);
        var entity3b = new TestEntity3(7);
        var entity3c = new TestEntity3(8);

        _ = entity1a.EntitySet.Add(entity2a);
        _ = entity1a.EntitySet.Add(entity2b);
        _ = entity1a.EntitySet.Add(entity2c);
        _ = entity1a.EntitySet.Add(entity2d);
        entity1a.Value1 = entity3a;

        _ = entity2a.EntitySet.Add(entity3a);
        entity2a.Value1 = entity3a;
        _ = entity2b.EntitySet.Add(entity3b);
        entity2b.Value1 = entity3b;
        _ = entity2c.EntitySet.Add(entity3c);
        entity2d.Value1 = entity3c;

        _ = entity3a.EntitySet.Add(entity1a);
        entity3a.Value1 = entity3c;
        _ = entity3b.EntitySet.Add(entity2a);
        entity3b.Value1 = entity3a;
        entity3c.Value1 = entity3b;

        tx.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ITestEntity).Assembly, typeof(ITestEntity).Namespace);
      return config;
    }
  }
}

namespace Xtensive.Orm.Tests.Linq.TypeAsTestModels
{
  public interface ITestEntity : IEntity
  {

  }

  [HierarchyRoot]
  public class TestEntity1 : Entity, ITestEntity
  {
    [Field]
    public EntitySet<TestEntity2> EntitySet { get; set; }

    [Field]
    public ITestEntity Value1 { get; set; }

    [Field, Key]
    public int Id { get; set; }

    [Field]
    public int Id2 { get; set; }

    public TestEntity1(int id2)
    {
      Id2 = id2;
    }
  }

  [HierarchyRoot]
  public class TestEntity2 : Entity, ITestEntity
  {
    [Field]
    public EntitySet<TestEntity3> EntitySet { get; set; }

    [Field, Key]
    public int Id { get; set; }

    [Field]
    public ITestEntity Value1 { get; set; }

    [Field]
    public int Id2 { get; set; }

    public TestEntity2(int id2)
    {
      Id2 = id2;
    }
  }

  [HierarchyRoot]
  public class TestEntity3 : Entity, ITestEntity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public EntitySet<ITestEntity> EntitySet { get; set; }

    [Field]
    public ITestEntity Value1 { get; set; }

    [Field]
    public int Id2 { get; set; }

    public TestEntity3(int id2)
    {
      Id2 = id2;
    }
  }
}
