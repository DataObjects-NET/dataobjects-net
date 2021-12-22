// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.02


using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  [Serializable]
  public class MiscTest : ChinookDOModelTest
  {
    [Test]
    public void MainTest()
    {
      var query =
        (from customer in Session.Query.All<Customer>()
        join invoice in Session.Query.All<Invoice>() on customer equals invoice.Customer into invoiceJoins
        from invoiceJoin in invoiceJoins.DefaultIfEmpty()
        select new {customer.Address, Invoice = invoiceJoin})
          .GroupBy(x => x.Address.City)
          .Select(g => new {City = g.Key, MaxCommission = g.Max(i => i.Invoice.Commission)});

      Assert.That(query, Is.Not.Empty);
      foreach (var queryable in query) {
        var city = queryable.City;
        var maxCommission = queryable.MaxCommission;
      }
    }

    [Test]
    public void Main2Test()
    {
      var query =
        from grouping in
          from customerInvoiceJoin in
            from customer in Session.Query.All<Customer>()
            join invoice in Session.Query.All<Invoice>()
              on customer equals invoice.Customer into invoiceJoins
            from invoiceJoin in invoiceJoins.DefaultIfEmpty()
            select new {customer.Address, Invoice = invoiceJoin}
          group customerInvoiceJoin by customerInvoiceJoin.Address.City
        select new {
          City = grouping.Key,
          MaxCommission = grouping.Max(i => i.Invoice.Commission),
          MinCommission = grouping.Min(i => i.Invoice.Commission),
          AverageCommission = grouping.Average(i => i.Invoice.Commission)
        };

      Assert.That(query, Is.Not.Empty);
      foreach (var queryable in query) {
        var city = queryable.City;
        var maxCommission = queryable.MaxCommission;
      }
    }

    [Test]
    public void MakePropertyAccessSwitchingToGeneric()
    {
      IQueryable queryable = Session.Query.All<Customer>();
      var expected = Session.Query.All<Customer>().Select(c => c.FirstName).ToList();
      var result1 = ((IQueryable<string>)SelectPropertySwitchingToGeneric(queryable, "FirstName")).ToList();
      var result2 = ((IQueryable<string>)SelectPropertyBuildingExpression(queryable, "FirstName")).ToList();

      Assert.That(result1, Is.Not.Empty);
      Assert.That(result2, Is.Not.Empty);
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
        .Lambda(selectExpression, Enumerable.Empty<ParameterExpression>())
        .Compile()
        .DynamicInvoke();
    }
  }
}