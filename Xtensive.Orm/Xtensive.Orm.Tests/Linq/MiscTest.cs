// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.02


using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  [Serializable]
  public class MiscTest : NorthwindDOModelTest
  {
    [Test]
    public void MainTest()
    {
      var query =
        (from customer in Session.Query.All<Customer>()
        join order in Session.Query.All<Order>() on customer equals order.Customer into orderJoins
        from orderJoin in orderJoins.DefaultIfEmpty()
        select new {customer.Address, Order = orderJoin})
          .GroupBy(x => x.Address.City)
          .Select(g => new {City = g.Key, MaxFreight = g.Max(o => o.Order.Freight)});
      foreach (var queryable in query) {
        var city = queryable.City;
        var maxFreight = queryable.MaxFreight;
      }
    }

    [Test]
    public void Main2Test()
    {
      var query =
        from grouping in
          from customerOrderJoin in
            from customer in Session.Query.All<Customer>()
            join order in Session.Query.All<Order>()
              on customer equals order.Customer into orderJoins
            from orderJoin in orderJoins.DefaultIfEmpty()
            select new {customer.Address, Order = orderJoin}
          group customerOrderJoin by customerOrderJoin.Address.City
        select new {
          City = grouping.Key
          , MaxFreight = grouping.Max(o => o.Order.Freight)
          , MinFreight = grouping.Min(o => o.Order.Freight)
          , AverageFreight = grouping.Average(o => o.Order.Freight)
        };

      foreach (var queryable in query) {
        var city = queryable.City;
        var maxFreight = queryable.MaxFreight;
      }
    }

    [Test]
    public void MakePropertyAccessSwitchingToGeneric()
    {
      IQueryable queryable = Session.Query.All<Customer>();
      var expected = Session.Query.All<Customer>().Select(c => c.ContactName).ToList();
      var result1 = ((IQueryable<string>)SelectPropertySwitchingToGeneric(queryable, "ContactName")).ToList();
      var result2 = ((IQueryable<string>)SelectPropertyBuildingExpression(queryable, "ContactName")).ToList();
      Assert.AreEqual(expected.Count, result1.Count);
      Assert.AreEqual(expected.Count, result2.Count);
      Assert.AreEqual(0, expected.Except(result1).Count());
      Assert.AreEqual(0, expected.Except(result2).Count());
    }

    public IQueryable SelectPropertySwitchingToGeneric(IQueryable queryable, string propertyName)
    {
      Type entityType = queryable.ElementType;
      var propertyInfo = entityType.GetProperty(propertyName);

      var parameter = Expression.Parameter(entityType, "paramName");
      var body = Expression.MakeMemberAccess(parameter, propertyInfo);
      var lambda = FastExpression.Lambda(body, parameter); // paramName=>paramName.Name
      
      var getPropertyValuesMethodInfo = typeof (MiscTest).GetMethod("SelectPropertyGeneric");
      var genericMethod = getPropertyValuesMethodInfo.MakeGenericMethod(entityType, propertyInfo.PropertyType);
      return (IQueryable) genericMethod.Invoke(null, new object[] {queryable, lambda});
    }

    public static IQueryable<TResult> SelectPropertyGeneric<T, TResult>(IQueryable<T> queryable, Expression<Func<T,TResult>> lambda) 
      where T : class, IEntity
    {
      return queryable.Select(lambda);
    }

    public IQueryable SelectPropertyBuildingExpression(IQueryable queryable, string propertyName)
    {
      Type entityType = queryable.ElementType;
      var propertyInfo = entityType.GetProperty(propertyName);
      var queryAllExpression = Expression.Constant(queryable); 

      var selectMethodInfo = typeof (System.Linq.Queryable).GetMethods()
        .Single(methodInfo=>methodInfo.Name==Xtensive.Reflection.WellKnown.Queryable.Select
        && methodInfo.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length==2);

      var parameter = Expression.Parameter(entityType, "paramName");
      var body = Expression.MakeMemberAccess(parameter, propertyInfo);
      var lambda = FastExpression.Lambda(body, parameter); // paramName=>paramName.Name

      var genericSelectMethodInfo = selectMethodInfo.MakeGenericMethod(entityType, propertyInfo.PropertyType);

      var selectExpression = Expression.Call(genericSelectMethodInfo, queryAllExpression, lambda);

      return (IQueryable) FastExpression
        .Lambda(selectExpression, EnumerableUtils<ParameterExpression>.Empty)
        .Compile()
        .DynamicInvoke();
    }
  }
}