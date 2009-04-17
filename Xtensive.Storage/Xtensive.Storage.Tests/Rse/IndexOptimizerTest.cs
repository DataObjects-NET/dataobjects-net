// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Providers.Index;
using Xtensive.Storage.Rse.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using EnumerationScope=Xtensive.Storage.Rse.Providers.EnumerationScope;
using IndexProvider=Xtensive.Storage.Rse.Providers.Compilable.IndexProvider;

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
    public void SingleSecondaryIndexTest()
    {
      Expression<Func<Order, bool>> predicate = order => order.OrderDate > new DateTime(1997, 11, 1)
        && order.OrderDate < new DateTime(1997, 11, 30);
      var expected = Query<Order>.All.AsEnumerable().Where(predicate.Compile()).OrderBy(o => o.Id);
      var query = Query<Order>.All.Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      ValidateUsedIndex(query, GetIndexForField<Order>("OrderDate"));
      ValidateQueryResult(expected, actual);
    }

    [Test]
    public void MultipleSecondaryIndexesTest()
    {
      Expression<Func<Order, bool>> predicate = order => order.OrderDate > new DateTime(1997, 11, 1)
        && order.Freight > 0 || order.OrderDate > new DateTime(1997, 11, 1) && order.Employee.Id == 125;
      var expected = Query<Order>.All.AsEnumerable().Where(predicate.Compile()).OrderBy(o => o.Id);
      var query = Query<Order>.All.Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      ValidateUsedIndex(query, GetIndexForField<Order>("OrderDate"),
        GetIndexForForeignKey<Order>("Employee"));
      ValidateQueryResult(expected, actual);
    }

    [Test]
    public void MultipleIndexesTest()
    {
      Expression<Func<Employee, bool>> predicate = employee => employee.Id > 0
        && employee.FirstName.GreaterThan("S")
        && employee.BirthDate < new DateTime(1960, 1, 1) || employee.Title.StartsWith("Vice")
        && employee.BirthDate > new DateTime(1950, 1, 1) && employee.BirthDate < new DateTime(1960, 1, 1);
      var expected = Query<Employee>.All.AsEnumerable().Where(predicate.Compile()).OrderBy(empl => empl.Id);
      var query = Query<Employee>.All.Where(predicate).OrderBy(empl => empl.Id);
      var actual = query.ToList();
      ValidateUsedIndex(query, GetIndexForField<Employee>("FirstName"),
        GetIndexForField<Employee>("Title"));
      ValidateQueryResult(expected, actual);
    }

    [Test]
    public void PrimaryIndexIsSelectedTest()
    {
      var firstEmployee = Query<Employee>.All.AsEnumerable().First();
      Expression<Func<Employee, bool>> predicate = employee => employee.Id == firstEmployee.Id
        && employee.BirthDate < new DateTime(1960, 1, 1);
      var expected = Query<Employee>.All.AsEnumerable().Where(predicate.Compile()).OrderBy(empl => empl.Id);
      var query = Query<Employee>.All.Where(predicate).OrderBy(empl => empl.Id);
      var actual = query.ToList();
      var optimizedProvider = GetOptimizedProvider(query);
      var secondaryIndexes = new List<IndexInfo>();
      FindSecondaryIndexProviders(optimizedProvider, secondaryIndexes);
      Assert.AreEqual(0, secondaryIndexes.Count);
      ValidateQueryResult(expected, actual);
    }

    [Test]
    public void MultiColumnIndexTest()
    {
      Expression<Func<Product, bool>> predicate = product => product.UnitPrice == 17.45m
        && product.Supplier.Id == 7 && product.Category.Id == 3
        && product.ProductName.GreaterThan("a")
        || product.UnitPrice > 10m && product.ProductName.StartsWith("S");
      var expected = Query<Product>.All.AsEnumerable().Where(predicate.Compile()).OrderBy(p => p.Id);
      var query = Query<Product>.All.Where(predicate).OrderBy(p => p.Id);
      var actual = query.ToList();
      ValidateUsedIndex(query, GetMultiColumnIndex<Product>("Category", "Supplier", "UnitPrice"),
        GetIndexForField<Product>("ProductName"));
      ValidateQueryResult(expected, actual);
    }

    [Test]
    public void JoinDifferentEntitiesTest()
    {
      Expression<Func<Order, bool>> orderPredicate = order => order.OrderDate > new DateTime(1997, 11, 1)
        && order.OrderDate < new DateTime(1997, 11, 30);
      Expression<Func<Employee, bool>> employeePredicate = employee => employee.Title.StartsWith("Sales");
      var expected = Query<Order>.All.AsEnumerable().Where(orderPredicate.Compile())
        .Join(Query<Employee>.All.Where(employeePredicate.Compile()),
          order => order.Employee.Id, empl => empl.Id,
          (order, empl) => new Pair<Order, Employee>(order, empl));
      var query = Query<Order>.All.Where(orderPredicate)
        .Join(Query<Employee>.All.Where(employeePredicate),
          order => order.Employee.Id, empl => empl.Id,
          (order, empl) => new Pair<Order, Employee>(order, empl));
      var actual = query.ToList();
      /*ValidateUsedIndex<Pair<Order,Employee>>(query, GetMultiColumnIndex<Product>("Category", "Supplier", "UnitPrice"),
        GetIndexForField<Product>("ProductName"));*/
      ValidateQueryResultForJoinTest(expected, actual);
    }

    private static void ValidateQueryResultForJoinTest(IEnumerable<Pair<Order, Employee>> expected, List<Pair<Order, Employee>> actual)
    {
      Assert.Greater(expected.Count(), 0);
      var equalityComparer = MockRepository.GenerateMock<IEqualityComparer<Pair<Order, Employee>>>();
      equalityComparer.Stub(comparer =>
        comparer.Equals(Arg<Pair<Order, Employee>>.Is.Anything, Arg<Pair<Order, Employee>>.Is.Anything))
        .Return(false).WhenCalled(invocation =>
          invocation.ReturnValue = (
            ((Pair<Order, Employee>) invocation.Arguments[0]).First.Key
              ==((Pair<Order, Employee>) invocation.Arguments[0]).First.Key
                &&
                ((Pair<Order, Employee>) invocation.Arguments[0]).Second.Key
                  ==((Pair<Order, Employee>) invocation.Arguments[0]).Second.Key));
      Assert.AreEqual(0, expected.Except(actual, equalityComparer).Count());
    }

    [Test]
    public void AtLeastOnExpressionAlwaysProducesFullRangeSetTest()
    {
      var rootProvider = new FilterProvider(
        IndexProvider.Get(Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex),
        t => t.GetValueOrDefault<decimal>(9) > 0 || t.GetValueOrDefault<DateTime?>(7) == DateTime.Now);
      var domainHandler = (Providers.Index.DomainHandler) (Domain.Handlers.DomainHandler);
      var realResolver = new StatisticsProviderResolver(domainHandler);
      var indexOptimizer = new IndexOptimizer(Domain.Model, realResolver);
      var optimizedProviderTree = indexOptimizer.Optimize(rootProvider);
      Assert.AreSame(rootProvider, optimizedProviderTree);
    }

    private IndexInfo GetIndexForField<T>(string fieldName)
    {
      return Domain.Model.Types[typeof(T)].Fields[fieldName].Column.Indexes.First();
    }

    private IndexInfo GetIndexForForeignKey<T>(string fieldName)
    {
      return Domain.Model.Types[typeof(T)].Fields[fieldName].Fields[fieldName + ".Id"].Column.Indexes.First();
    }

    private IndexInfo GetMultiColumnIndex<T>(params string[] fieldNames)
    {
      var sb = new StringBuilder();
      sb.Append(Domain.Model.Types[typeof(T)].MappingName);
      sb.Append(".");
      sb.Append("IX_");
      foreach (var name in fieldNames)
        sb.Append(name);
      return Domain.Model.Types[typeof (T)].Indexes[sb.ToString()];
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
    {
      var optimizedProvider = GetOptimizedProvider(query);
      var secondaryIndexProviders = new List<IndexInfo>();
      FindSecondaryIndexProviders(optimizedProvider, secondaryIndexProviders);
      Assert.Greater(secondaryIndexProviders.Count, 0);
      Assert.AreEqual(0, secondaryIndexProviders.Except(expectedIndexes).Count());
    }

    private static CompilableProvider GetOptimizedProvider<T>(IQueryable<T> query)
    {
      CompilableProvider optimizedProvider;
      using (EnumerationScope.Open()) {
        var recordSet = ((Query<T>) query).Compiled;
        optimizedProvider = CompilationContext.Current.Compile(recordSet.Provider).Origin;
      }
      return optimizedProvider;
    }

    private void FindSecondaryIndexProviders(Provider provider, List<IndexInfo> result)
    {
      foreach (var source in provider.Sources) {
        var indexProvider = source as IndexProvider;
        if (indexProvider!=null) {
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