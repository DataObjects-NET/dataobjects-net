// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Tuples;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class GroupByTest : ChinookDOModelTest
  {
    [Test]
    public void NullableGroupingKeyTest()
    {
      var grouping = Session.Query.All<Invoice>().GroupBy(i => i.ProcessingTime).FirstOrDefault(g=>g.Key==null);
      Assert.That(grouping, Is.Not.Null);
      Assert.That(grouping.Count()> 0, Is.True);
    }

    [Test]
    public void EntityWithLazyLoadFieldTest()
    {
      var track = Session.Query.All<Track>()
        .GroupBy(t => t.MediaType)
        .First()
        .Key;
      var columnIndex = Domain.Model.Types[typeof (MediaType)].Fields["Name"].MappingInfo.Offset;
      Assert.That(track.State.Tuple.GetFieldState(columnIndex).IsAvailable(), Is.True);
    }

    [Test]
    public void AggregateAfterGroupingTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Track>()
        .GroupBy(t => t.MediaType)
        .Select(g => g.Where(p2 => p2.UnitPrice==g.Count()));

      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
    }

    [Test]
    public void AggregateAfterGroupingAnonymousTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Track>()
        .GroupBy(t => t.MediaType)
        .Select(g => new {
        CheapestTracks =
          g.Where(t => t.UnitPrice==g.Count())
      });

      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleTest()
    {
      var result = Session.Query.All<Track>().GroupBy(t => t.UnitPrice);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithWhereTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query =
        Session.Query.All<Track>()
        .GroupBy(track => track.TrackId)
        .Where(grouping => true)
        .Select(tracks => tracks.Count());

      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
    }

    [Test]
    public void GroupingAsQueryableTest()
    {
      var result = Session.Query.All<Track>().GroupBy(t => t.UnitPrice);
      Assert.That(result, Is.Not.Empty);
      foreach (IGrouping<decimal, Track> grouping in result)
        Assert.That(grouping.GetType().IsOfGenericInterface(typeof (IQueryable<>)), Is.True);
    }

    [Test]
    public void SimpleEntityGroupTest()
    {
      var result = Session.Query.All<Track>().GroupBy(t => t);
      Assert.That(result, Is.Not.Empty);
      DumpGrouping(result);
    }

    [Test]
    public void EntityGroupWithOrderTest()
    {
      var result = Session.Query.All<Track>().GroupBy(t =>t).OrderBy(g => g.Key.TrackId);
      var resultList = result.ToList();
      var expectedList = Tracks.GroupBy(t => t).OrderBy(g => g.Key.TrackId).ToList();
      Assert.That(resultList, Is.Not.Empty);
      Assert.That(expectedList.Count(), Is.EqualTo(resultList.Count));
      for (var i = 0; i < resultList.Count; i++) {
        Assert.That(resultList[i].Key, Is.EqualTo(expectedList[i].Key));
        Assert.That(resultList[i].Count(), Is.EqualTo(expectedList[i].Count()));
        Assert.That(resultList[i].AsQueryable().Count(), Is.EqualTo(expectedList[i].Count()));
      }
      DumpGrouping(result);
    }

    [Test]
    public void EntityGroupSimpleTest()
    {
      var groupByResult = Session.Query.All<Track>().GroupBy(t => t.MediaType);
      Assert.That(groupByResult, Is.Not.Empty);
      DumpGrouping(groupByResult);
    }

    [Test]
    public void EntityGroupTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var groupByResult = Session.Query.All<Track>().GroupBy(t => t.MediaType);
      IEnumerable<MediaType> result = groupByResult
        .ToList()
        .Select(g => g.Key);
      IEnumerable<MediaType> expectedKeys = Session.Query.All<Track>()
        .Select(p => p.MediaType)
        .Distinct()
        .ToList();

      Assert.That(result, Is.Not.Empty);
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Track>()
          .Where(t => t.MediaType==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      DumpGrouping(groupByResult);
    }

    [Test]
    public void EntityKeyGroupTest()
    {
      var groupByResult = Session.Query.All<Track>().GroupBy(t => t.MediaType.Key);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Track>()
        .Select(t => t.MediaType.Key)
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Track>()
          .Where(t => t.MediaType.Key==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void EntityFieldGroupTest()
    {
      var groupByResult = Session.Query.All<Track>().GroupBy(t => t.MediaType.Name);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Track>()
        .Select(t => t.MediaType.Name)
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Track>()
          .Where(t => t.MediaType.Name==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void StructureGroupTest()
    {
      var groupByResult = Session.Query.All<Customer>().GroupBy(customer => customer.Address);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Customer>()
        .Select(customer => customer.Address)
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Customer>()
          .Where(customer => customer.Address==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }


    [Test]
    public void StructureGroupWithNullFieldTest()
    {
      var customer = Session.Query.All<Customer>().First();
      customer.Address.Country = null;
      Session.Current.SaveChanges();
      StructureGroupTest();
    }

    [Test]
    public void AnonymousTypeGroupTest()
    {
      var groupByResult = Session.Query.All<Customer>().GroupBy(customer => new {customer.Address.City, customer.Address.Country});
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Customer>()
        .Select(customer => new {customer.Address.City, customer.Address.Country})
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Customer>()
          .Where(customer => new {customer.Address.City, customer.Address.Country}==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousTypeEntityAndStructureGroupTest()
    {
      var groupByResult = Session.Query.All<Employee>().GroupBy(employee => new {employee.Address});
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Employee>()
        .Select(employee => new {employee.Address})
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Employee>()
          .Where(employee => new {employee.Address}==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousStructureGroupTest()
    {
      var groupByResult = Session.Query.All<Customer>().GroupBy(customer => new {customer.Address});
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Customer>()
        .Select(customer => new {customer.Address})
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Customer>()
          .Where(customer => new {customer.Address}==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousTypeEntityAndFieldTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var groupByResult = Session.Query.All<Track>().GroupBy(t => new {
        t.MediaType,
        t.MediaType.Name,
      });
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Track>()
        .Select(t => new {
          t.MediaType,
          t.MediaType.Name,
        })
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Track>()
          .Where(t => new {
            t.MediaType,
            t.MediaType.Name,
          }==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousTypeEntityGroupTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var groupByResult = Session.Query.All<Track>().GroupBy(t => new {t.MediaType});
      var list = groupByResult.ToList();
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Track>()
        .Select(t => new {t.MediaType})
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Track>()
          .Where(t => new {t.MediaType}==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void DefaultTest()
    {
      var groupByResult = Session.Query.All<Customer>().GroupBy(customer => customer.Address.City);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Customer>()
        .Select(customer => customer.Address.City)
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Customer>()
          .Where(customer => customer.Address.City==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void FilterGroupingByKeyTest()
    {
      var groupByResult = Session.Query.All<Invoice>()
        .GroupBy(i => i.BillingAddress.City)
        .Where(grouping => grouping.Key.StartsWith("L"));
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Invoice>()
        .Select(i => i.BillingAddress.City)
        .Where(city => city.StartsWith("L"))
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Invoice>()
          .Where(i => i.BillingAddress.City==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void FilterGroupingByCountAggregateTest()
    {
      var groupByResult = Session.Query.All<Invoice>()
        .GroupBy(i => i.BillingAddress.City)
        .Where(g => g.Count() > 1);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Session.Query.All<Invoice>()
        .Select(i => i.BillingAddress.City)
        .Where(city => city.StartsWith("L"))
        .Distinct()
        .ToList();
      Assert.That(result, Is.Not.Empty);

      foreach (var grouping in groupByResult) {
        var items = Session.Query.All<Invoice>()
          .Where(i => i.BillingAddress.City==grouping.Key)
          .ToList();
        Assert.That(items.Except(grouping).Count(), Is.EqualTo(0));
      }
      Assert.That(expectedKeys.Except(result).Count(), Is.EqualTo(0));
      DumpGrouping(groupByResult);
    }

    [Test]
    public void FilterGroupingBySumAggregateTest()
    {
      var queryable = Session.Query.All<Invoice>().GroupBy(i => i.BillingAddress.City);

      var groupByResult = queryable.Where(city => city.Sum(i => i.Commission) > 2);

      var groupByAlternativeResult = queryable.Where(city => city.Sum(i => i.Commission) <= 2);

      Assert.That(groupByResult.Count() + groupByAlternativeResult.Count(), Is.EqualTo(queryable.Count()));

      Assert.That(groupByResult, Is.Not.Empty);
      Assert.That(groupByAlternativeResult, Is.Not.Empty);

      foreach (IGrouping<string, Invoice> grouping in groupByResult) {
        var sum = grouping.ToList().Sum(i => i.Commission);
        Assert.That(sum > 2, Is.True);
      }

      foreach (IGrouping<string, Invoice> grouping in groupByAlternativeResult) {
        var sum = grouping.ToList().Sum(i => i.Commission);
        Assert.That(sum <= 2, Is.True);
      }

      DumpGrouping(groupByResult);
    }

    [Test]
    public void GroupByWhereTest()
    {
      var queryable = Session.Query.All<Invoice>().GroupBy(i => i.BillingAddress.City);
      var result = queryable.Where(g => g.Key.StartsWith("L") && g.Count() > 2);
      var alternativeResult = queryable.Where(g => !g.Key.StartsWith("L") || g.Count() <= 2);

      Assert.That(result, Is.Not.Empty);
      Assert.That(result.Count() + alternativeResult.Count(), Is.EqualTo(queryable.Count()));

      foreach (IGrouping<string, Invoice> grouping in result) {
        var startsWithL = grouping.Key.StartsWith("L");
        var countGreater2 = grouping.ToList().Count() > 2;
        Assert.That(startsWithL && countGreater2, Is.True);
      }

      foreach (IGrouping<string, Invoice> grouping in alternativeResult) {
        var startsWithL = grouping.Key.StartsWith("L");
        var countGreater2 = grouping.ToList().Count() > 2;
        Assert.That(!(startsWithL && countGreater2), Is.True);
      }

      DumpGrouping(result);
    }

    [Test]
    public void GroupByWhere2Test()
    {
      IQueryable<IEnumerable<Invoice>> result = Session.Query.All<Invoice>()
        .GroupBy(o => o.BillingAddress.City)
        .Select(g => g.Where(o => o.Commission > 0));
      Assert.That(result, Is.Not.Empty);

      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectTest()
    {
      var groupBy = Session.Query.All<Track>().GroupBy(t => t.Name);
      var result = groupBy.Select(g => g);
      Assert.That(result, Is.Not.Empty);
      Assert.That(result.ToList().Count(), Is.EqualTo(groupBy.ToList().Count()));
      DumpGrouping(result);
    }

    [Test]
    public void GroupBySelectWithAnonymousTest()
    {
      var groupBy = Session.Query.All<Track>().GroupBy(t => t.Name);
      var result = groupBy.Select(g => new {g});
      Assert.That(result, Is.Not.Empty);
      Assert.That(result.ToList().Count(), Is.EqualTo(groupBy.ToList().Count()));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectKeyTest()
    {
      var groupBy = Session.Query.All<Track>().GroupBy(t => t.Name);
      IQueryable<string> result = groupBy.Select(g => g.Key);
      Assert.That(result, Is.Not.Empty);
      Assert.That(result.ToList().Count(), Is.EqualTo(groupBy.ToList().Count()));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectKeyWithSelectCalculableColumnTest()
    {
      //avoid great amout of data for grouping, some storages are painfully slow with such operation
      var result = Session.Query.All<Employee>().GroupBy(t => t.LastName).Select(g => g.Key + "String");
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectKeyWithCalculableColumnTest()
    {
      //avoid great amout of data for grouping, some storages are painfully slow with such operation
      var result = Session.Query.All<Employee>().GroupBy(t => t.LastName + "String");
      var list = result.ToList();
      Assert.That(result, Is.Not.Empty);
      DumpGrouping(result);
    }

    [Test]
    [IgnoreOnGithubActionsIfFailed(provider: StorageProvider.MySql, reason: "5.7 version and only this version have problems, the same query works in 5.6 and in 8.0+")]
    public void GroupByWithSelectFirstTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => g.First());
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectGroupingTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(o => o.Customer)
        .Select(g => g);
      Assert.That(result, Is.Not.Empty);
      DumpGrouping(result);
    }

    [Test]
    public void GroupBySelectManyTest()
    {
      var result = Session.Query.All<Customer>()
        .GroupBy(c => c.Address.City)
        .SelectMany(g => g);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectManyKeyTest()
    {
      var result = Session.Query.All<Customer>()
        .GroupBy(c => c.Address.City)
        .SelectMany(g => g.Key);
      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(result));
    }

    [Test]
    public void GroupByEntitySelectManyTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(o => o.Customer)
        .SelectMany(g => g);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySumTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer.FirstName)
        .Select(g => g.Sum(i => i.Commission));
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByCountTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(o => o.Customer)
        .Select(g => new {Customer = g.Key, InvoicesCount = g.Count()});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithFilterTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => new {Customer = g.Key, Invoices = g.Where(i => i.InvoiceDate < DateTime.Now)});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySumMinMaxAvgTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g =>
          new {
            Sum = g.Sum(o => o.Commission),
            Min = g.Min(o => o.Commission),
            Max = g.Max(o => o.Commission),
            Avg = g.Average(o => o.Commission)
          });
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithConstantResultSelectorTest()
    {
      var result = Session.Query.All<Invoice>().GroupBy(i => i.Customer, (c, g) =>
        new {
          ConstString = "ConstantString"
        });
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithConstantSelectorTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g =>
          new {
            ConstString = "ConstantString"
          });
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithAnonymousSelectTest()
    {
      IQueryable<IGrouping<Customer, Invoice>> groupings = Session.Query.All<Invoice>().GroupBy(o => o.Customer);
      var result = groupings.Select(g => new {g});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelectorTest()
    {
      var result = Session.Query.All<Invoice>().GroupBy(i => i.Customer, (c, g) =>c);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }
    [Test]
    public void GroupByWithEntityResultSelector2Test()
    {
      var result = Session.Query.All<Invoice>().GroupBy(i => i.Customer, (c, g) =>
        new {
          Customer = c,
        });
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector3Test()
    {
      IQueryable<IEnumerable<Invoice>> result = Session.Query.All<Invoice>().GroupBy(i => i.Customer, (c, g) => g);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByBooleanTest()
    {
      var result = Session.Query.All<Customer>().GroupBy(c => c.FirstName.StartsWith("A"));
      var expected = Session.Query.All<Customer>().AsEnumerable().GroupBy(c => c.FirstName.StartsWith("A"));

      Assert.That(result, Is.Not.Empty);
      Assert.That(expected.Select(g => g.Key).OrderBy(k => k)
        .SequenceEqual(result.AsEnumerable().Select(g => g.Key).OrderBy(k => k)), Is.True);
      foreach (var group in expected)
        Assert.That(expected.Where(g => g.Key==group.Key)
          .SelectMany(g => g).OrderBy(i => i.CustomerId)
          .SequenceEqual(result.AsEnumerable()
            .Where(g => g.Key==group.Key).SelectMany(g => g).OrderBy(i => i.CustomerId)), Is.True);
    }

    [Test]
    public void GroupByBooleanSubquery1Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>().GroupBy(c => c.Invoices.Count > 5);
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
      Assert.That(list.Any(c => c.Any(cc => cc.Invoices.Count > 5)));
      DumpGrouping(result);
    }

    [Test]
    public void GroupByBooleanSubquery2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Customer>()
        .Where(c => c.Invoices.Count > 0)
        .GroupBy(c => c.Invoices.Average(o => o.Commission) >= 0.4m);
      Assert.That(result, Is.Not.Empty);
      DumpGrouping(result);
    }

    [Test]
    public void GroupByWithAggregateResultSelectorTest()
    {
      IQueryable<int> result = Session.Query.All<Invoice>().GroupBy(i => i.Commission, (c, g) => g.Count());
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithAnonymousResultSelector5Test()
    {
      var result = Session.Query.All<Invoice>().GroupBy(i => i.Commission, (c, g) => new {Count = g.Count(), Customer = c});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector5BisTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Commission)
        .Select(g => new {Count = g.Count(), Commission = g.Key});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(o => o.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis20Test()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => g.Key.CompanyName);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis22Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key.CompanyName});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis23Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key})
        .Where(g => g.Customer.CompanyName!=null);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis24Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key})
        .OrderBy(g => g.Customer.CompanyName);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector5Bis212Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => new {Count = new {Count1 = g.Count(), Count2 = g.Count()}});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis21Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => new {Count = new {Count1 = g.Count(), Count2 = g.Count()}, Customer = g.Key});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector5Bis3Test()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => new {i.InvoiceDate, i.Commission})
        .Select(g => new {Count = g.Count(), InvoiceInfo = g.Key});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithResultSelectorTest2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>().GroupBy(i => i.Customer, (c, g) =>
        new {
          Customer = c,
          Sum = g.Sum(i => i.Commission),
          Min = g.Min(i => i.Commission),
          Max = g.Max(i => i.Commission),
          Avg = g.Average(i => i.Commission)
        });
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithResultSelectorTest3Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer)
        .Select(g => new {
          Sum = g.Sum(i => i.Commission),
          Min = g.Min(i => i.Commission),
          Max = g.Max(i => i.Commission),
          Avg = g.Average(i => i.Commission)
        });
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithResultSelectorTest4Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>().GroupBy(i => i.Customer, (c, g) =>
        new {
          // Customer = c,
          Sum = g.Sum(i => i.Commission),
          Min = g.Min(i => i.Commission),
          Max = g.Max(i => i.Commission),
          Avg = g.Average(i => i.Commission)
        });
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithElementSelectorSumTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer, i => i.Commission)
        .Select(g => g.Sum());
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithElementSelectorSumAnonymousTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer, i => i.Commission)
        .Select(g => new {A = g.Sum(), B = g.Sum()});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithElementSelectorTest()
    {
      var result = Session.Query.All<Invoice>().GroupBy(i => i.Customer, i => i.Commission);
      Assert.That(result, Is.Not.Empty);
      DumpGrouping(result);
    }

    [Test]
    public void GroupByWithElementSelectorSumMaxTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer.CustomerId, i => i.Commission)
        .Select(g => new {Sum = g.Sum(), Max = g.Max()});
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithAnonymousElementTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => i.Customer, i => new {i.Commission})
        .Select(g => g.Sum(i => i.Commission));
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithTwoPartKeyTest()
    {
      var result = Session.Query.All<Invoice>()
        .GroupBy(i => new {i.Customer.CustomerId, i.InvoiceDate})
        .Select(g => g.Sum(i => i.Commission));
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByGroupByTest()
    {
      // NOTE: Order-by is lost when group-by is applied (the sequence of groups is not ordered)
      var result = Session.Query.All<Invoice>()
        .OrderBy(i => i.InvoiceDate)
        .GroupBy(i => i.Customer.CustomerId)
        .Select(g => g.Sum(i => i.Commission));
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByGroupBySelectManyTest()
    {
      // NOTE: Order-by is preserved within grouped sub-collections
      var result = Session.Query.All<Invoice>()
        .OrderBy(i => i.InvoiceDate)
        .GroupBy(i => i.Customer.CustomerId)
        .SelectMany(g => g);
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void FilterGroupingTest()
    {
      var result = Session.Query.All<Track>()
        .GroupBy(i => i.Album)
        .Select(g => g.Where(i => i.Name.StartsWith("A")));
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupWithJoinTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var query = Session.Query.All<Customer>()
        .GroupBy(c => c.Address.Country)
        .Join(Session.Query.All<Customer>(),
          country => country.Key,
          c2 => c2.Address.Country,
          (country, c2) => new {
            country = country.Key,
            total = c2.Invoices.Sum(i => i.Commission)
          });

      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
    }

    [Test]
    public void GroupByBoolExpression()
    {
      var query = Session.Query.All<Invoice>();
      var falseResult = query.Count(c => c.Status != (InvoiceStatus) 1);
      var trueResult = query.Count(c => c.Status == (InvoiceStatus) 1);

      var result = query.GroupBy(c => c.Status == (InvoiceStatus) 1)
        .Select(c => new {Value = c.Key, Count = c.Count()})
        .ToArray();

      Assert.That(result.Single(i => !i.Value).Count, Is.EqualTo(falseResult));
      Assert.That(result.Single(i => i.Value).Count, Is.EqualTo(trueResult));
    }

    [Test]
    public void GroupByBoolExpressionComplex()
    {
      var query = Session.Query.All<Invoice>();
      var falseResult = query.Count(c => !(c.Status == (InvoiceStatus) 1 || c.Status == (InvoiceStatus) 2));
      var trueResult = query.Count(c => c.Status == (InvoiceStatus) 1 || c.Status == (InvoiceStatus) 2);

      var result = query
        .GroupBy(c => c.Status == (InvoiceStatus) 1 || c.Status == (InvoiceStatus) 2)
        .Select(c => new {Value = c.Key, Count = c.Count()})
        .ToArray();

      Assert.That(result.Single(i => !i.Value).Count, Is.EqualTo(falseResult));
      Assert.That(result.Single(i => i.Value).Count, Is.EqualTo(trueResult));
    }
    
    [Test]
    public void GroupByEnumTernaryWithNonNullConstTest()
    {
      var query = Session.Query.All<Invoice>()
        .GroupBy(c => c.Total < 0 ? (InvoiceStatus?) InvoiceStatus.Completed : c.Status).ToArray();
    }

    [Test]
    public void GroupByEnumTernaryWithNullConstTest()
    {
      var query = Session.Query.All<Invoice>()
        .GroupBy(c => c.Total < 0 ? (InvoiceStatus?) null : c.Status).ToArray();
    }

    private void DumpGrouping<TKey, TValue>(IQueryable<IGrouping<TKey, TValue>> result)
    {
      DumpGrouping(result, false);
    }

    private void DumpGrouping<TKey, TValue>(IQueryable<IGrouping<TKey, TValue>> result, bool logOutput)
    {
      // Just enumerate
      foreach (IGrouping<TKey, TValue> grouping in result) {
        foreach (var group in grouping) {
          Assert.That(group, Is.Not.Null);
        }
      }

      // Check
      var list = result.ToList();
      Assert.That(list.Count, Is.GreaterThan(0));
      Assert.That(result.Count(), Is.EqualTo(list.Count));
      foreach (var grouping in result) {
        Assert.That(grouping.Key, Is.Not.Null);
        var count = grouping.ToList().Count();
        Assert.That(grouping.Count(), Is.EqualTo(count));
        Assert.That(grouping.AsQueryable().Count(), Is.EqualTo(count));
      }
      if (logOutput)
        QueryDumper.Dump(result);
    }
  }
}