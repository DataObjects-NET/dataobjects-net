// Copyright (C) 2019-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2019.02.14

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0761_ReadingAvgAndSumByDecimalFieldModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0761_ReadingAvgAndSumByDecimalFieldModel
{
  [HierarchyRoot]
  public class DecimalValueStructureEntityByRefCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DecimalValueStructureCase Ref { get; set; }

    public DecimalValueStructureEntityByRefCase(Session session)
      : base(session)
    {
    }
  }

  public class DecimalValueStructure : Structure
  {
    [Field(Precision = 19, Scale = 5)]
    public decimal Value { get; set; }

    [Field]
    public int Code { get; set; }

    public DecimalValueStructure(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DecimalValueStructureCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DecimalValueStructure Struct { get; set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal AdditionalValue { get; set; }

    public DecimalValueStructureCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class KeyValueByEntityRefCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public KeyExpressionCase Ref { get; set; }

    public KeyValueByEntityRefCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class KeyExpressionCase : Entity
  {
    [Field(Precision = 19, Scale = 5), Key]
    public decimal Id { get; private set; }

    [Field]
    public int SomeValue { get; set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal AdditionalValue { get; set; }

    public KeyExpressionCase(Session session, decimal id)
      : base(session, id)
    {
    }
  }

  [HierarchyRoot]
  public class ValueByEntityRefCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DirectFieldValueCase Ref { get; set; }

    public ValueByEntityRefCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DirectFieldValueCase : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal Accepted { get; set; }

    [Field(Precision = 19, Scale = 5)]
    public decimal AdditionalValue { get; set; }

    public DirectFieldValueCase(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Precision = 29, Scale = 2)]
    public decimal Sum { get; set; }

    [Field(Precision = 29, Scale = 1)]
    public decimal Sum2 { get; set; }

    [Field(Precision = 28, Scale = 2)]
    public decimal Sum3 { get; set; }

    [Field(Precision = 27, Scale = 3)]
    public decimal Sum4 { get; set; }

    [Field(Precision = 26, Scale = 4)]
    public decimal Sum5 { get; set; }

    [Field(Precision = 25, Scale = 5)]
    public decimal Sum6 { get; set; }

    [Field(Precision = 24, Scale = 6)]
    public decimal Sum7 { get; set; }

    [Field(Precision = 23, Scale = 7)]
    public decimal Sum8 { get; set; }

    [Field(Precision = 22, Scale = 8)]
    public decimal Sum9 { get; set; }

    [Field(Precision = 21, Scale = 9)]
    public decimal Sum10 { get; set; }

    [Field(Precision = 10, Scale = 0)]
    public decimal Count { get; set; }

    public Order(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0761_ReadingAvgAndSumByDecimalField : AutoBuildTest
  {
    private const int OrderCount = 100;

    protected override bool InitGlobalSession => true;

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.SqlServer | StorageProvider.PostgreSql);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Order).Assembly, typeof(Order).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      for (var i = 0; i < OrderCount; i++) {
        _ = new Order(GlobalSession) {
          Sum = (i % 2 == 0) ? 100000000000000000000000000.11m : 100000000000000000000000000.12m,
          Sum2 = 100000000000000000000000000.3m,
          Sum3 = 10000000000000000000000000.33m,
          Sum4 = 100000000000000000000000.333m,
          Sum5 = 1000000000000000000000.3333m,
          Sum6 = 10000000000000000000.33333m,
          Sum7 = 100000000000000000.333333m,
          Sum8 = 1000000000000000.3333333m,
          Sum9 = 10000000000000.33333333m,
          Sum10 = 100000000000.333333333m,
          Count = OrderCount
        };
      }

      foreach (var i in Enumerable.Range(1, 1000)) {
        _ = new ValueByEntityRefCase(GlobalSession) {
          Ref = new DirectFieldValueCase(GlobalSession) {
            Accepted = 163767
          }
        };

        _ = new KeyValueByEntityRefCase(GlobalSession) {
          Ref = new KeyExpressionCase(GlobalSession, 163767 + i)
        };

        _ = new DecimalValueStructureEntityByRefCase(GlobalSession) {
          Ref = new DecimalValueStructureCase(GlobalSession) {
            Struct = new DecimalValueStructure(GlobalSession) {
              Value = 163767,
              Code = i
            }
          }
        };
      }

      GlobalSession.SaveChanges();

#if DEBUG
      // just to keep data in database
      GlobalTransaction.Complete();
#endif
    }

    #region Avg tests

    [Test]
    public void AverageComplexTest()
    {
      var queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum);
      var fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.11m).Or.EqualTo(0.12m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum2);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.3m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum3);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.33m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum4);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.333m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum5);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.3333m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum6);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.33333m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum7);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.333333m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum8);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.3333333m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum9);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.33333333m));

      queryResult = GlobalSession.Query.All<Order>().Average(o => o.Sum10);
      fraction = queryResult - Math.Floor(queryResult);
      Assert.That(fraction, Is.EqualTo(0.333333333m));
    }

    [Test]
    public void AvgDirectFieldValueCase()
    {
      TestAverage<DirectFieldValueCase>(a => a.Accepted);
      TestAverage<DirectFieldValueCase>(a => a.Accepted + a.AdditionalValue);
      TestAverage<DirectFieldValueCase>(a => a.Accepted + 1m);
    }

    [Test]
    public void AvgValueByEntityRefCase()
    {
      TestAverage<ValueByEntityRefCase>(a => a.Ref.Accepted);
      TestAverage<ValueByEntityRefCase>(a => a.Ref.Accepted + a.Ref.AdditionalValue);
      TestAverage<ValueByEntityRefCase>(a => a.Ref.Accepted + 1m);
    }

    [Test]
    public void AvgKeyExpressionCase()
    {
      TestAverage<KeyExpressionCase>(a => a.Id);
      TestAverage<KeyExpressionCase>(a => a.Id + a.AdditionalValue);
      TestAverage<KeyExpressionCase>(a => a.Id + 1m);
    }

    [Test]
    public void AvgKeyValueByEntityRefCase()
    {
      TestAverage<KeyValueByEntityRefCase>(a => a.Ref.Id);
      TestAverage<KeyValueByEntityRefCase>(a => a.Ref.Id + a.Ref.AdditionalValue);
      TestAverage<KeyValueByEntityRefCase>(a => a.Ref.Id + 1m);
    }

    [Test]
    public void AvgDecimalValueStructureCase()
    {
      TestAverage<DecimalValueStructureCase>(a => a.Struct.Value);
      TestAverage<DecimalValueStructureCase>(a => a.Struct.Value + a.AdditionalValue);
      TestAverage<DecimalValueStructureCase>(a => a.Struct.Value + 1m);
    }

    [Test]
    public void AvgDecimalValueStructureEntityByRefCase()
    {
      TestAverage<DecimalValueStructureEntityByRefCase>(a => a.Ref.Struct.Value);
      TestAverage<DecimalValueStructureEntityByRefCase>(a => a.Ref.Struct.Value + a.Ref.AdditionalValue);
      TestAverage<DecimalValueStructureEntityByRefCase>(a => a.Ref.Struct.Value + 1m);
    }

    [Test]
    public void AvgDecimalExpressionInSourceExpressionsCase()
    {
      var results = GlobalSession.Query.All<DirectFieldValueCase>()
        .GroupBy(e => e.Id, e => new { Split = e.Accepted * 0.01M })
        .Select(g => g.Select(x => x.Split).Distinct().Average()).ToList();
      var localResults = GlobalSession.Query.All<DirectFieldValueCase>().AsEnumerable()
        .GroupBy(e => e.Id, e => new { Split = e.Accepted * 0.01M })
        .Select(g => g.Select(x => x.Split).Distinct().Average()).ToList();

      Assert.That(results.Count, Is.EqualTo(localResults.Count));
      Assert.That(results.SequenceEqual(localResults), Is.True);
    }

    private void TestAverage<TEntity>(Expression<Func<TEntity, decimal>> selector) where TEntity : Entity
    {
      var results = GlobalSession.Query.All<TEntity>().Average(selector);
      var localResults = GlobalSession.Query.All<TEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(results, Is.EqualTo(localResults), $"Failed on Average({selector})");

      results = GlobalSession.Query.All<TEntity>().Select(selector).Average();
      localResults = GlobalSession.Query.All<TEntity>().AsEnumerable().Select(selector.Compile()).Average();
      Assert.That(results, Is.EqualTo(localResults), $"Failed on Select({selector}).Average()");

      results = GlobalSession.Query.All<TEntity>().Select(selector).Distinct().Average();
      localResults = GlobalSession.Query.All<TEntity>().AsEnumerable().Select(selector.Compile()).Distinct().Average();
      Assert.That(results, Is.EqualTo(localResults), $"Failed on Select({selector}).Distinct().Average()");
    }

    #endregion

    #region Sum tests

    [Test]
    public void SumComplexTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer, " MS SQL Server has scale reduction algorithm, PgSql doesn't");

      var queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum);
      var localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum);
      Assert.That(queryResult, Is.EqualTo(localResult + 3));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum2);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum2);
      Assert.That(queryResult, Is.EqualTo(localResult + 6));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum3);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum3);
      Assert.That(queryResult, Is.EqualTo(localResult + 0.6m));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum4);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum4);
      Assert.That(queryResult, Is.EqualTo(localResult));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum5);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum5);
      Assert.That(queryResult, Is.EqualTo(localResult));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum6);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum6);
      Assert.That(queryResult, Is.EqualTo(localResult));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum7);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum7);
      Assert.That(queryResult, Is.EqualTo(localResult));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum8);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum8);
      Assert.That(queryResult, Is.EqualTo(localResult));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum9);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum9);
      Assert.That(queryResult, Is.EqualTo(localResult));

      queryResult = GlobalSession.Query.All<Order>().Sum(o => o.Sum10);
      localResult = GlobalSession.Query.All<Order>().ToArray().Sum(o => o.Sum10);
      Assert.That(queryResult, Is.EqualTo(localResult));
    }

    [Test]
    public void SumDirectFieldValueCase()
    {
      TestSum<DirectFieldValueCase>(a => a.Accepted);
      TestSum<DirectFieldValueCase>(a => a.Accepted + a.AdditionalValue);
      TestSum<DirectFieldValueCase>(a => a.Accepted + 1m);
    }

    [Test]
    public void SumValueByEntityRefCase()
    {
      TestSum<ValueByEntityRefCase>(a => a.Ref.Accepted);
      TestSum<ValueByEntityRefCase>(a => a.Ref.Accepted + a.Ref.AdditionalValue);
      TestSum<ValueByEntityRefCase>(a => a.Ref.Accepted + 1m);
    }

    [Test]
    public void SumKeyExpressionCase()
    {
      TestSum<KeyExpressionCase>(a => a.Id);
      TestSum<KeyExpressionCase>(a => a.Id + a.AdditionalValue);
      TestSum<KeyExpressionCase>(a => a.Id + 1m);
    }

    [Test]
    public void SumKeyValueByEntityRefCase()
    {
      TestSum<KeyValueByEntityRefCase>(a => a.Ref.Id);
      TestSum<KeyValueByEntityRefCase>(a => a.Ref.Id + a.Ref.AdditionalValue);
      TestSum<KeyValueByEntityRefCase>(a => a.Ref.Id + 1m);
    }

    [Test]
    public void SumDecimalValueStructureCase()
    {
      TestSum<DecimalValueStructureCase>(a => a.Struct.Value);
      TestSum<DecimalValueStructureCase>(a => a.Struct.Value + a.AdditionalValue);
      TestSum<DecimalValueStructureCase>(a => a.Struct.Value + 1m);
    }

    [Test]
    public void SumDecimalValueStructureEntityByRefCase()
    {
      TestSum<DecimalValueStructureEntityByRefCase>(a => a.Ref.Struct.Value);
      TestSum<DecimalValueStructureEntityByRefCase>(a => a.Ref.Struct.Value + a.Ref.AdditionalValue);
      TestSum<DecimalValueStructureEntityByRefCase>(a => a.Ref.Struct.Value + 1m);
    }

    [Test]
    public void SumDecimalExpressionInSourceExpressionsCase()
    {
      var results = GlobalSession.Query.All<DirectFieldValueCase>()
        .GroupBy(e => e.Id, e => new { Split = e.Accepted * 0.01M })
        .Select(g => g.Select(x => x.Split).Distinct().Sum()).ToList();
      var localResults = GlobalSession.Query.All<DirectFieldValueCase>().AsEnumerable()
        .GroupBy(e => e.Id, e => new { Split = e.Accepted * 0.01M })
        .Select(g => g.Select(x => x.Split).Distinct().Sum()).ToList();

      Assert.That(results.Count, Is.EqualTo(localResults.Count));
      Assert.That(results.SequenceEqual(localResults), Is.True);
    }

    private void TestSum<TEntity>(Expression<Func<TEntity, decimal>> selector) where TEntity : Entity
    {
      var results = GlobalSession.Query.All<TEntity>().Sum(selector);
      var localResults = GlobalSession.Query.All<TEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(results, Is.EqualTo(localResults), $"Failed on Sum({selector})");

      results = GlobalSession.Query.All<TEntity>().Select(selector).Sum();
      localResults = GlobalSession.Query.All<TEntity>().AsEnumerable().Select(selector.Compile()).Sum();
      Assert.That(results, Is.EqualTo(localResults), $"Failed on Select({selector}).Sum()");

      results = GlobalSession.Query.All<TEntity>().Select(selector).Distinct().Sum();
      localResults = GlobalSession.Query.All<TEntity>().AsEnumerable().Select(selector.Compile()).Distinct().Sum();
      Assert.That(results, Is.EqualTo(localResults), $"Failed on Select({selector}).Distinct().Sum()");
    }

    #endregion
  }
}