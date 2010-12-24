// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Helpers;
using Xtensive.Linq;
using Xtensive.Linq.Normalization;
using Xtensive.Parameters;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers.Indexing;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using IndexProvider=Xtensive.Storage.Rse.Providers.Compilable.IndexProvider;

namespace Xtensive.Orm.Tests.Rse
{
  [TestFixture, Category("Rse")]
  public class IndexOptimizerTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Index);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    [Test]
    public void SingleSecondaryIndexTest()
    {
      Expression<Func<Order, bool>> predicate = order => order.OrderDate > new DateTime(1997, 11, 1)
        && order.OrderDate < new DateTime(1997, 11, 30);
      var expected = Session.Query.All<Order>().ToList().Where(predicate.CachingCompile()).OrderBy(o => o.Id);
      var query = Session.Query.All<Order>().Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model,
        IndexOptimizerTestHelper.GetIndexForField<Order>("OrderDate", Domain.Model));
      IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
    }

    [Test]
    public void MultipleSecondaryIndexesTest()
    {
      Expression<Func<Order, bool>> predicate = order => order.OrderDate > new DateTime(1997, 11, 1)
        && order.Freight > 0 || order.OrderDate > new DateTime(1997, 11, 1) && order.Employee.Id == 125;
      var expected = Session.Query.All<Order>().ToList().Where(predicate.CachingCompile()).OrderBy(o => o.Id);
      var query = Session.Query.All<Order>().Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model,
        IndexOptimizerTestHelper.GetIndexForField<Order>("OrderDate", Domain.Model),
        IndexOptimizerTestHelper.GetIndexForForeignKey<Order>("Employee", Domain.Model));
      IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
    }

    [Test]
    public void MultipleIndexesTest()
    {
      Expression<Func<Employee, bool>> predicate = employee => employee.Id > 0
        && employee.FirstName.GreaterThan("S")
        && employee.BirthDate < new DateTime(1960, 1, 1) || employee.Title.GreaterThan("Vice")
        && employee.BirthDate > new DateTime(1950, 1, 1) && employee.BirthDate < new DateTime(1960, 1, 1);
      var expected = Session.Query.All<Employee>().ToList().Where(predicate.CachingCompile()).OrderBy(empl => empl.Id);
      var query = Session.Query.All<Employee>().Where(predicate).OrderBy(empl => empl.Id);
      var actual = query.ToList();
      IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model,
        IndexOptimizerTestHelper.GetIndexForField<Employee>("FirstName", Domain.Model),
        IndexOptimizerTestHelper.GetIndexForField<Employee>("Title", Domain.Model));
      IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
    }

    [Test]
    public void PrimaryIndexIsSelectedTest()
    {
      var firstEmployee = Session.Query.All<Employee>().ToList().First();
      Expression<Func<Employee, bool>> predicate = employee => employee.Key == firstEmployee.Key
        && employee.BirthDate < new DateTime(1960, 1, 1);
      var expected = Session.Query.All<Employee>().ToList().Where(predicate.CachingCompile()).OrderBy(empl => empl.Id);
      var query = Session.Query.All<Employee>().Where(predicate).OrderBy(empl => empl.Id);
      var actual = query.ToList();
      var optimizedProvider = IndexOptimizerTestHelper.GetOptimizedProvider(query);
      var secondaryIndexes = new List<IndexInfo>();
      IndexOptimizerTestHelper.FindSecondaryIndexProviders(optimizedProvider, secondaryIndexes,
        Domain.Model);
      Assert.AreEqual(0, secondaryIndexes.Count);
      IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
    }

    [Test]
    public void MultiColumnIndexTest()
    {
      var targetSupplier = Session.Query.All<Supplier>().Where(supplier => supplier.CompanyName=="Pavlova, Ltd.")
        .Single();
      var targetCategory = Session.Query.All<Category>().Where(cat => cat.CategoryName=="Confections").Single();
      Expression<Func<Product, bool>> predicate = product => product.UnitPrice > 10m
        && product.Supplier.Key == targetSupplier.Key && product.Category.Key == targetCategory.Key
        && product.ProductName.GreaterThan("a")
        || product.UnitPrice > 10m && product.ProductName.GreaterThan("S");
      var expected = Session.Query.All<Product>().ToList().Where(predicate.CachingCompile()).OrderBy(p => p.Id);
      var query = Session.Query.All<Product>().Where(predicate).OrderBy(p => p.Id);
      var actual = query.ToList();
      IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model,
        GetMultiColumnIndex<Product>("Category", "Supplier", "UnitPrice"),
        IndexOptimizerTestHelper.GetIndexForField<Product>("ProductName", Domain.Model));
      IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
    }

    [Test]
    public void JoinDifferentEntitiesTest()
    {
      Expression<Func<Order, bool>> orderPredicate = order => order.OrderDate > new DateTime(1997, 11, 1)
        && order.OrderDate < new DateTime(1997, 11, 30);
      Expression<Func<Employee, bool>> employeePredicate = employee => employee.Title.GreaterThan("Sales");
      var expected = Session.Query.All<Order>().ToList().Where(orderPredicate.CachingCompile())
        .Join(Session.Query.All<Employee>().Where(employeePredicate.CachingCompile()),
          order => order.Employee.Key, empl => empl.Key,
          (order, empl) => new Pair<Order, Employee>(order, empl));
      var query = Session.Query.All<Order>().Where(orderPredicate)
        .Join(Session.Query.All<Employee>().Where(employeePredicate),
          order => order.Employee.Key, empl => empl.Key,
          (order, empl) => new {order, empl});
      var actual = query.ToList()
        .Select(arg => new Pair<Order, Employee>(arg.order, arg.empl)).ToList();
      IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model,
        IndexOptimizerTestHelper.GetIndexForField<Order>("OrderDate", Domain.Model),
        IndexOptimizerTestHelper.GetIndexForField<Employee>("Title", Domain.Model));
      ValidateQueryResultForJoinTest(expected, actual);
    }

    [Test]
    public void AtLeastOneExpressionAlwaysProducesFullRangeSetTest()
    {
      var primaryIndex = Domain.Model.Types[typeof (Order)].Indexes.PrimaryIndex;
      var recordSetHeader = primaryIndex.GetRecordSetHeader();
      var requiredDateFieldIndex = GetFieldIndex(recordSetHeader, "RequiredDate");
      var orderDateFieldIndex = GetFieldIndex(recordSetHeader, "OrderDate");
      var rootProvider = new FilterProvider(
        IndexProvider.Get(primaryIndex),
        t => t.GetValueOrDefault<DateTime?>(requiredDateFieldIndex) == DateTime.Now
          || t.GetValueOrDefault<DateTime?>(orderDateFieldIndex) == DateTime.Now);
      var domainHandler = (DomainHandler) (Domain.Handlers.DomainHandler);
      var realResolver = new OptimizationInfoProviderResolver(domainHandler);
      var indexOptimizer = new IndexOptimizer(Domain.Model, realResolver);
      var optimizedProviderTree = indexOptimizer.Process(rootProvider);
      Assert.AreSame(rootProvider, optimizedProviderTree);
    }

    [Test]
    public void NormalizationFailedTest()
    {
      Expression<Func<Employee, bool>> predicate =
        BuildCnfPredicate<Employee>(41, employee => employee.FirstName.GreaterThan("B"));
      var normalizer = new DisjunctiveNormalizer(100);
      AssertEx.ThrowsInvalidOperationException(() => normalizer.Normalize(predicate));
      var expected = Session.Query.All<Employee>().ToList().Where(predicate.CachingCompile()).OrderBy(o => o.Id);
      var query = Session.Query.All<Employee>().Where(predicate).OrderBy(o => o.Id);
      var actual = query.ToList();
      IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model,
        IndexOptimizerTestHelper.GetIndexForField<Employee>("FirstName", Domain.Model));
      IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
    }

    [Test]
    public void UsingOfParametersTest()
    {
      var orderDateParam = new Parameter<DateTime>(new DateTime(1997, 11, 1));
      var freightParam = new Parameter<Decimal>(0);
      using (new ParameterContext().Activate()) {
        orderDateParam.Value = new DateTime(1900, 1, 1);
        freightParam.Value = 100000;
        Expression<Func<Order, bool>> predicate = order => order.OrderDate > orderDateParam.Value
          && order.ShipName.LessThan("K") || order.ShipName.GreaterThan("W")
            && order.Freight < freightParam.Value;
        var expected = Session.Query.All<Order>().ToList().Where(predicate.CachingCompile()).OrderBy(o => o.Id);
        var query = Session.Query.All<Order>().Where(predicate).OrderBy(o => o.Id);
        var actual = query.ToList();
        IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model,
          IndexOptimizerTestHelper.GetIndexForField<Order>("OrderDate", Domain.Model),
          IndexOptimizerTestHelper.GetIndexForField<Order>("Freight", Domain.Model));
        IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
      }
    }

    private static void ValidateQueryResultForJoinTest(IEnumerable<Pair<Order, Employee>> expected,
      List<Pair<Order, Employee>> actual)
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

    private static Expression<Func<T, bool>> BuildCnfPredicate<T>(int termCount,
      Expression<Func<T, bool>> term)
    {
      var termBody = term.Body;
      var result = termBody;
      for (int i = 0; i < (termCount - 1)/2; i++)
        result = Expression.AndAlso(result, Expression.OrElse(termBody, termBody));
      return Expression.Lambda<Func<T, bool>>(result, term.Parameters[0]);
    }

    private IndexInfo GetMultiColumnIndex<T>(params string[] fieldNames)
    {
      var sb = new StringBuilder();
      sb.Append(Domain.Model.Types[typeof(T)].Name);
      sb.Append(".");
      sb.Append("IX_");
      foreach (var name in fieldNames)
        sb.Append(name);
      return Domain.Model.Types[typeof (T)].Indexes.GetIndexesContainingAllData()
        .Where(indexInfo => indexInfo.Name.Contains(sb.ToString())).Single();
    }

    private static int GetFieldIndex(RecordSetHeader rsHeader, string fieldName)
    {
      return rsHeader.IndexOf(fieldName);
    }
  }
}