// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.10

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Indexing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Tests.Providers
{
  [TestFixture]
  public class SortProviderTest
  {
    private TupleDescriptor pKeyDescriptor;
    private TupleDescriptor entityValueDescriptor;
    private IIndex primaryIndex;
    private DirectionCollection<Column> fkSortOrder;
    private const int itemCount = 10000;

    [TestFixtureSetUp]
    public void SetUp()
    {
      pKeyDescriptor = new TupleDescriptor("TestEntityPK");
      pKeyDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("id", typeof (int)));
      pKeyDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("typeID", typeof (Guid)));
      pKeyDescriptor.Lock();
      entityValueDescriptor = new TupleDescriptor("TestEntity");
      entityValueDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("name", typeof (string)));
      entityValueDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("tag", typeof (Guid)));
      entityValueDescriptor.Lock();

      using (new Measurement(itemCount)) {
        RecordHeader keyHeader = new RecordHeader(pKeyDescriptor, CultureInfo.InvariantCulture);
        RecordHeader entityValueHeader = new RecordHeader(entityValueDescriptor, CultureInfo.InvariantCulture);
        RecordHeader recordHeader = new RecordHeader(keyHeader, entityValueHeader);

        DirectionCollection<RecordColumn> pkSortOrder = new DirectionCollection<RecordColumn>();
        pkSortOrder.Add(recordHeader.RecordColumnCollection["id"], Direction.Positive);
        pkSortOrder.Add(recordHeader.RecordColumnCollection["typeID"], Direction.Negative);

        fkSortOrder = new DirectionCollection<RecordColumn>();
        fkSortOrder.Add(recordHeader.RecordColumnCollection["name"], Direction.Positive);

        primaryIndex = new MemoryIndex(recordHeader, pkSortOrder);
        for (int i = 0; i < itemCount; i++) {
          ITuple keyTuple = pKeyDescriptor.CreateTuple();
          ITuple valueTuple = entityValueDescriptor.CreateTuple();
          keyTuple.SetValue("id", i/10);
          keyTuple.SetValue("typeID", Guid.NewGuid());
          valueTuple.SetValue("name", "name_" + i/10);
          valueTuple.SetValue("tag", Guid.NewGuid());
          Record record = new Record(recordHeader, keyTuple, valueTuple);
          primaryIndex.Add(record);
        }
      }
    }

    [Test]
    public void Iterate()
    {
      int count = 0;
      Rse.Providers.IndexProvider indexProvider = new Rse.Providers.IndexProvider(primaryIndex);
      SortProvider sortProvider = new SortProvider(indexProvider.Result, fkSortOrder);

      using (new Measurement()) {
        RecordComparer comparer = new RecordComparer(fkSortOrder);
        Record previousRecord = null;
        foreach (Record record in sortProvider.Result) {
          int result = comparer.Compare(previousRecord, record);
          Assert.IsTrue(result <= 0);
          count++;
        }
      }
      Assert.AreEqual(itemCount, count);
    }

    [Test]
    public void Range()
    {
      int count = 0;
      Rse.Providers.IndexProvider indexProvider = new Rse.Providers.IndexProvider(primaryIndex);
      SortProvider sortProvider = new SortProvider(indexProvider.Result, fkSortOrder);

      using (new Measurement()) {
        Collection<KeyValuePair<string, object>> values = new Collection<KeyValuePair<string, object>>();
        values.Add(new KeyValuePair<string, object>("name", "name_20"));
        Record from = new Record(primaryIndex.RecordHeader, values);
        values.Clear();
        values.Add(new KeyValuePair<string, object>("name", "name_21"));
        Record to = new Record(primaryIndex.RecordHeader, values);
        foreach (Record record in  new IndexRangeProvider(sortProvider.Result, new Range<Record>(from, to)).Result) {
          count++;
        }
      }
      Assert.AreEqual(120, count);
    }
  }
}