// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.09.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0593_AggregateForSingleColumnModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0593_AggregateForSingleColumnModel
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public sbyte SByteField { get; set; }

      [Field]
      public sbyte? NullableSByteField { get; set; }

      [Field]
      public byte ByteField { get; set; }

      [Field]
      public byte? NullableByteField { get; set; }

      [Field]
      public short ShortField { get; set; }

      [Field]
      public short? NullableShortField { get; set; }

      [Field]
      public ushort UShortField { get; set; }

      [Field]
      public ushort? NullableUShortField { get; set; }

      [Field]
      public int IntField { get; set; }

      [Field]
      public int? NullableIntField { get; set; }

      [Field]
      public uint UIntField { get; set; }

      [Field]
      public uint? NullableUIntField { get; set; }

      [Field]
      public long LongField { get; set; }

      [Field]
      public long? NullableLongField { get; set; }

      [Field]
      public ulong ULongField { get; set; }

      [Field]
      public ulong? NullableULongField { get; set; }

      [Field]
      public float FloatField { get; set; }

      [Field]
      public float? NullableFloatField { get; set; }

      [Field]
      public double DoubleField { get; set; }

      [Field]
      public double? NullableDoubleField { get; set; }

      [Field]
      public decimal DecimalField { get; set; }

      [Field]
      public decimal? NullableDecimalField { get; set; }

      [Field]
      public TimeSpan TimeSpanField { get; set; }

      [Field]
      public TimeSpan? NullableTimeSpanField { get; set; }

      [Field]
      public DateTime DateTimeField { get; set; }

      [Field]
      public DateTime? NullableDateTimeField { get; set; }

      [Field]
      public string StringField { get; set; }

      [Field]
      public Guid GuidField { get; set; }

      [Field]
      public Guid? NullableGuidField { get; set; }
    }
  }

  public class IssueJira0593_AggregateForSingleColumnTest : AutoBuildTest
  {
    private const int EntityCount = 20;
    private const string FilterCondition = "3";

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (TestEntity));
      return config;
    }

    protected override void PopulateData()
    {
      var now = DateTime.Now.Date;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for (var i = 0; i < EntityCount; ++i) {
          var entity = new TestEntity {
            Name = "Entity" + i,
            SByteField = -1,
            ByteField = 1,
            ShortField = -2,
            UShortField = 2,
            IntField = -3,
            UIntField = 3,
            LongField = -4,
            ULongField = 4,
            FloatField = -5.55f,
            DoubleField = 6.66d,
            DecimalField = -7.77m,
            TimeSpanField = TimeSpan.FromMinutes(i),
            DateTimeField = now.AddDays(i),
            GuidField = Guid.NewGuid(),
            StringField = "StringField" + i,
          };

          if (i % 2==0) {
            entity.NullableSByteField = entity.SByteField;
            entity.NullableByteField = entity.ByteField;
            entity.NullableShortField = entity.ShortField;
            entity.NullableUShortField = entity.UShortField;
            entity.NullableIntField = entity.IntField;
            entity.NullableUIntField = entity.UIntField;
            entity.NullableLongField = entity.LongField;
            entity.NullableULongField = entity.ULongField;
            entity.NullableFloatField = entity.FloatField;
            entity.NullableDoubleField = entity.DoubleField;
            entity.NullableDecimalField = entity.DecimalField;
            entity.NullableTimeSpanField = entity.TimeSpanField;
            entity.NullableDateTimeField = entity.DateTimeField;
            entity.NullableGuidField = entity.GuidField;
          }
        }

        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var localSum = Query.All<TestEntity>().Select(c => c.DecimalField).ToArray().Sum();
        Assert.AreEqual(Query.All<TestEntity>().Sum(c => c.DecimalField), localSum);
        Assert.AreEqual(Query.All<TestEntity>().Select(c => c.DecimalField).Sum(c => c), localSum);
        Assert.AreEqual(Query.All<TestEntity>().Select(c => c.DecimalField).Sum(), localSum);

        var query = Query.All<TestEntity>().Select(c => c.Name.Contains("3") ? c.DecimalField : -c.DecimalField);
        localSum = query.ToArray().Sum();
        Assert.AreEqual(query.Sum(c => c), localSum);
        Assert.AreEqual(query.Sum(), localSum);

        var nullableQuery = Query.All<TestEntity>().Select(c => c.Name.Contains("3") ? c.NullableDecimalField : -c.NullableDecimalField);
        var nullableLocalSum = nullableQuery.ToArray().Sum();
        Assert.AreEqual(nullableQuery.Sum(c => c), nullableLocalSum);
        Assert.AreEqual(nullableQuery.Sum(), nullableLocalSum);
      }
    }

    [Test]
    public void DifferentColumnsTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains("3") ? c.IntField : -c.ShortField);
        var localSum = query.ToArray().Sum();
        Assert.AreEqual(query.Sum(c => c), localSum);
        Assert.AreEqual(query.Sum(), localSum);

        var nullableQuery = Query.All<TestEntity>().Select(c => c.Name.Contains("3") ? c.DecimalField : -c.NullableDecimalField);
        var nullableLocalSum = nullableQuery.ToArray().Sum();
        Assert.AreEqual(nullableQuery.Sum(c => c), nullableLocalSum);
        Assert.AreEqual(nullableQuery.Sum(), nullableLocalSum);
      }
    }

    #region Primitive numeric type tests (nonnullable and nullable)

    [Test]
    public void SByteTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.SByteField : -c.SByteField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableSByteTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableSByteField : -c.NullableSByteField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void ByteTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.ByteField : -c.ByteField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableByteTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableByteField : -c.NullableByteField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void ShortTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.ShortField : -c.ShortField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableShortTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableShortField : -c.NullableShortField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void UShortTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.UShortField : -c.UShortField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableUShortTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableUShortField : -c.NullableUShortField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void IntTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.IntField : -c.IntField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableIntTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableIntField : -c.NullableIntField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void UIntTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.UIntField : -c.UIntField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableUIntTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableUIntField : -c.NullableUIntField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void LongTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.LongField : -c.LongField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableLongTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableLongField : -c.NullableLongField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void ULongTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? -(long) c.ULongField : (long) c.ULongField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableULongTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? -(long?) c.NullableULongField : (long?) c.NullableULongField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void FloatTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.FloatField : -c.FloatField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableFloatTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableFloatField : -c.NullableFloatField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void DoubleTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.DoubleField : -c.DoubleField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableDoubleTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableDoubleField : -c.NullableDoubleField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void DecimalTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.DecimalField : -c.DecimalField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableDecimalTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableDecimalField : -c.NullableDecimalField);
        CheckQueryable(query);
      }
    }

    #endregion

    #region Nonnumeric type test (nonnulable and nullable)

    [Test]
    public void TimeSpanTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.TimeSpanField : -c.TimeSpanField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableTimeSpanTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableTimeSpanField : -c.NullableTimeSpanField);
        CheckQueryable(query);
      }
    }

    [Test]
    public void DateTimeTest()
    {
      var now = DateTime.Now;
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.DateTimeField : now);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableDateTimeTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableDateTimeField : null);
        CheckQueryable(query);
      }
    }

    [Test]
    public void GuidTest()
    {
      var newGuid = Guid.NewGuid();
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.GuidField : newGuid);
        CheckQueryable(query);
      }
    }

    [Test]
    public void NullableGuidTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.NullableGuidField : null);
        CheckQueryable(query);
      }
    }

    [Test]
    public void StringTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var query = Query.All<TestEntity>().Select(c => c.Name.Contains(FilterCondition) ? c.StringField : c.Name);
        CheckQueryable(query);
      }
    }

    #endregion

    #region CheckQueryable methods

    private static void CheckQueryable(IQueryable<int> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<int?> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<long> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<long?> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<float> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<float?> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<double> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<double?> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<decimal> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<decimal?> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Sum(), query.Sum(c => c));
      Assert.AreEqual(localArray.Sum(), query.Sum());
      Assert.AreEqual(localArray.Average(), query.Average(c => c));
      Assert.AreEqual(localArray.Average(), query.Average());
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<TimeSpan> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<TimeSpan?> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<DateTime> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<DateTime?> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<Guid> query)
    {
      var localArray = query.ToArray();
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Min(c => c)));
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Min()));
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Max(c => c)));
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Max()));
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<Guid?> query)
    {
      var localArray = query.ToArray();
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Min(c => c)));
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Min()));
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Max(c => c)));
      Assert.Throws(typeof (QueryTranslationException), () => Assert.AreEqual(localArray.Min(), query.Max()));
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    private static void CheckQueryable(IQueryable<string> query)
    {
      var localArray = query.ToArray();
      Assert.AreEqual(localArray.Min(), query.Min(c => c));
      Assert.AreEqual(localArray.Min(), query.Min());
      Assert.AreEqual(localArray.Max(), query.Max(c => c));
      Assert.AreEqual(localArray.Max(), query.Max());
      Assert.AreEqual(localArray.Count(), query.Count());
    }

    #endregion
  }
}
