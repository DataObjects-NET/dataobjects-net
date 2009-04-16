// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rhino.Mocks;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse.Optimization;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Rse
{
  [TestFixture]
  public class IndexOptimizerTest : NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfiguration.Load("memory");
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    [Test]
    public void SingleIndexTest()
    {
      var employeeKey = Query<Employee>.All.AsEnumerable().Single(employee => employee.Id==125);
      Expression<Func<Order, bool>> predicate = order => order.OrderDate > new DateTime(1997, 11, 1)
        && order.OrderDate < new DateTime(1997, 11, 30) && order.Employee.Id == -1;
      var localOrders = Query<Order>.All.AsEnumerable().Where(predicate.Compile());
      var ordersQuery = Query<Order>.All.Where(predicate);
      var orders = ordersQuery.ToList();
      ValidateUsedIndex(ordersQuery, GetIndexForField("OrderDate"));
      ValidateQueryResult(localOrders, orders);
    }

    [Test]
    public void AtLeastOnExpressionAlwaysProducesFullRangeSetTest()
    {
      var rootProvider = new FilterProvider(
        IndexProvider.Get(Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex),
        t => t.GetValueOrDefault<decimal>(9) > 0 || t.GetValueOrDefault<int>(3) < 3);
      var domainHandler = (Providers.Index.DomainHandler) (Domain.Handlers.DomainHandler);
      var statisticsProviderResolver = MockRepository.GenerateStub<IStatisticsProviderResolver>();
      statisticsProviderResolver.Stub(resolver => resolver.Resolve(Arg<IndexInfo>.Is.Anything)).Return(null)
        .WhenCalled(invocation =>
          invocation.ReturnValue = domainHandler.GetRealIndex((IndexInfo) invocation.Arguments[0]));
      var indexOptimizer = new IndexOptimizer(Domain.Model, statisticsProviderResolver);
      var optimizedProviderTree = indexOptimizer.Optimize(rootProvider);
      Assert.AreSame(rootProvider, optimizedProviderTree);
    }

    private IndexInfo GetIndexForField(string fieldName)
    {
      return Domain.Model.Types[typeof(Order)].Fields[fieldName].Column.Indexes.First();
    }

    private static void ValidateQueryResult<T>(IEnumerable<T> expected, IEnumerable<T> actual)
      where T : Entity
    {
      Assert.Greater(expected.Count(), 0);
      var equalityComparer = MockRepository.GenerateStub<IEqualityComparer<T>>();
      equalityComparer.Stub(comparer => comparer.Equals(Arg<T>.Is.Anything, Arg<T>.Is.Anything))
        .Return(false).WhenCalled(invocation =>
          invocation.ReturnValue = ((Entity)invocation.Arguments[0]).Key
            == ((Entity)invocation.Arguments[1]).Key);
      Assert.AreEqual(0, expected.Except(actual, equalityComparer).Count());
    }

    private void ValidateUsedIndex<T>(IQueryable<T> query, params IndexInfo[] expectedIndexes)
      where T : Entity
    {
      var secondaryIndexProviders = new List<IndexInfo>();
      FindSecondaryIndexProviders(((Query<T>)query).Compiled.ExecutableProviderOrigin, secondaryIndexProviders);
      Assert.Greater(secondaryIndexProviders.Count, 0);
      Assert.AreEqual(0, secondaryIndexProviders.Except(expectedIndexes).Count());
    }

    private void FindSecondaryIndexProviders(Provider provider, List<IndexInfo> result)
    {
      foreach (var source in provider.Sources)
      {
        var indexProvider = source as IndexProvider;
        if (indexProvider != null)
        {
          var indexInfo = indexProvider.Index.Resolve(Domain.Model);
          if (!indexInfo.IsPrimary)
            result.Add(indexInfo);
        }
        else
          FindSecondaryIndexProviders(source, result);
      }
    }
  }
}