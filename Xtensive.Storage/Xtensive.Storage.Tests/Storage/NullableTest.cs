// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.30

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.NullableTestModel;

namespace Xtensive.Storage.Tests.Storage.NullableTestModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class MyEntity : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public int? Field1 { get; set; }

    [Field]
    public double? Field2 { get; set; }

    [Field]
    public DateTime? Field3 { get; set; }

    [Field]
    public int? Field4 { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  [Explicit("Requires PostgreSQL and MSSQL servers")]
  [TestFixture]
  public class NullableTest : CrossStorageTest
  {
    protected override DomainConfiguration BuildConfiguration(string provider)
    {
      var config = base.BuildConfiguration(provider);
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(MyEntity).Namespace);
      return config;
    }

    protected override void FillData(Domain domain)
    {
      new MyEntity {Field1 = 1, Field2 = 5.0, Field3 = new DateTime(2005, 5, 5), Field4 = null};
      new MyEntity {Field1 = 3, Field2 = 15.0, Field3 = new DateTime(2008, 8, 8), Field4 = null};
      new MyEntity {Field1 = null, Field2 = null, Field3 = null, Field4 = null};
    }

    [Test]
    public void AggregateTest()
    {
      RunTest(AggregateTest);
    }

    [Test]
    public void FilterTest()
    {
      RunTest(FilterTest);
    }

    private static RecordSet GetEntityRecordSet(Domain domain, Type entityType)
    {
      return domain.Model.Types[entityType].Indexes.PrimaryIndex.ToRecordSet();
    }

    private static RecordSet AggregateTest(Domain domain)
    {
      var primary = GetEntityRecordSet(domain, typeof (MyEntity));

      var descriptors = new[] {
          new AggregateColumnDescriptor("min_1", primary.Header.IndexOf("Field1"), AggregateType.Min),
          new AggregateColumnDescriptor("min_2", primary.Header.IndexOf("Field2"), AggregateType.Min),
          new AggregateColumnDescriptor("min_3", primary.Header.IndexOf("Field3"), AggregateType.Min),
          new AggregateColumnDescriptor("min_4", primary.Header.IndexOf("Field4"), AggregateType.Max),

          new AggregateColumnDescriptor("max_1", primary.Header.IndexOf("Field1"), AggregateType.Max),
          new AggregateColumnDescriptor("max_2", primary.Header.IndexOf("Field2"), AggregateType.Max),
          new AggregateColumnDescriptor("max_3", primary.Header.IndexOf("Field3"), AggregateType.Max),
          new AggregateColumnDescriptor("max_4", primary.Header.IndexOf("Field4"), AggregateType.Max),

          new AggregateColumnDescriptor("sum_1", primary.Header.IndexOf("Field1"), AggregateType.Sum),
          new AggregateColumnDescriptor("sum_2", primary.Header.IndexOf("Field2"), AggregateType.Sum),
          new AggregateColumnDescriptor("sum_4", primary.Header.IndexOf("Field4"), AggregateType.Sum),

          new AggregateColumnDescriptor("avg_1", primary.Header.IndexOf("Field1"), AggregateType.Avg),
          new AggregateColumnDescriptor("avg_2", primary.Header.IndexOf("Field2"), AggregateType.Avg),
          new AggregateColumnDescriptor("avg_4", primary.Header.IndexOf("Field4"), AggregateType.Avg),
        };

      return primary.Aggregate(null, descriptors);
    }

    private static RecordSet FilterTest(Domain domain)
    {
      var primary = GetEntityRecordSet(domain, typeof(MyEntity));
      return primary.Filter(
        t => !(t.GetValueOrDefault<int?>(primary.Header.IndexOf("Field1")) > 3)
        );
    }
  }
}
