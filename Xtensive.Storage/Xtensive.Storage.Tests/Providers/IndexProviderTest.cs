// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.08

using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Storage;
using Xtensive.Core.Indexing;
using IndexProvider = Xtensive.Storage.Rse.Providers.IndexProvider;

namespace Xtensive.Storage.Tests.Query.Providers
{
  [TestFixture]
  public class IndexProviderTest
  {
    private TupleDescriptor pKeyDescriptor;
    private TupleDescriptor entityValueDescriptor;
    private MemoryIndex primaryIndex;
    private MemoryIndex secondaryIndex;
    const int itemCount = 10000;

    [TestFixtureSetUp]
    public void SetUp()
    {
      pKeyDescriptor = new TupleDescriptor("TestEntityPK");
      pKeyDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("id", typeof(int)));
      pKeyDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("typeID", typeof(Guid)));
      pKeyDescriptor.Lock();
      entityValueDescriptor = new TupleDescriptor("TestEntity");
      entityValueDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("name", typeof(string)));
      entityValueDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("tag", typeof(Guid)));
      entityValueDescriptor.Lock();

      using (new Measurement(itemCount))
      {
        RecordHeader keyHeader = new RecordHeader(pKeyDescriptor, CultureInfo.InvariantCulture);
        RecordHeader entityValueHeader = new RecordHeader(entityValueDescriptor, CultureInfo.InvariantCulture);
        RecordHeader recordHeader = new RecordHeader(keyHeader, entityValueHeader);

        DirectionCollection<RecordColumn> pkSortOrder = new DirectionCollection<RecordColumn>();
        pkSortOrder.Add(recordHeader.RecordColumnCollection["id"], Direction.Positive);
        pkSortOrder.Add(recordHeader.RecordColumnCollection["typeID"], Direction.Negative);

        DirectionCollection<RecordColumn> fkSortOrder = new DirectionCollection<RecordColumn>();
        fkSortOrder.Add(recordHeader.RecordColumnCollection["name"], Direction.Positive);

        primaryIndex = new MemoryIndex(recordHeader, pkSortOrder);
        secondaryIndex = new MemoryIndex(recordHeader, fkSortOrder, pkSortOrder);
        for (int i = 0; i < itemCount; i++)
        {
          ITuple keyTuple = pKeyDescriptor.CreateTuple();
          ITuple valueTuple = entityValueDescriptor.CreateTuple();
          keyTuple.SetValue("id", i / 10);
          keyTuple.SetValue("typeID", Guid.NewGuid());
          valueTuple.SetValue("name", "name_" + i / 10);
          valueTuple.SetValue("tag", Guid.NewGuid());
          Record record = new Record(recordHeader, keyTuple, valueTuple);
          primaryIndex.Add(record);
          secondaryIndex.Add(record);
        }
      }
    }

    [Test]
    public void Iterate()
    {
      using (new Measurement()) {
        int count = 0;
        IndexProvider indexProvider = new IndexProvider(primaryIndex);
        RecordComparer comparer = new RecordComparer(primaryIndex.SortOrder);
        Record previousRecord = null;
        foreach (Record record in indexProvider.Result) {
          int result = comparer.Compare(previousRecord, record);
          Assert.IsTrue(result<=0);
          count++;
        }
        Assert.AreEqual(itemCount, count);
      }
    }

    [Test]
    public void Range()
    {
      using (new Measurement()) {
        IndexProvider indexProvider = new IndexProvider(primaryIndex);
        Record from = new Record(primaryIndex.KeyHeader, 25);
        Record to = new Record(primaryIndex.KeyHeader, 26);
        int count = 0;
        foreach (Record record in new IndexRangeProvider(indexProvider.Result , new Range<Record>(from, to)).Result) {
          count++;
        }
        Assert.AreEqual(20,count);

   
        count = 0;
        foreach (Record record in new IndexRangeProvider(new IndexRangeProvider(indexProvider.Result , new Range<Record>(to, to)).Result, new Range<Record>(to, from)).Result)
        {
          count++;
        }
        Assert.AreEqual(0, count);

        IndexProvider secondaryIndexProvider = new IndexProvider(secondaryIndex);

        Record from1 = new Record(secondaryIndex.KeyHeader, "name_10");
        Record to1 = new Record(secondaryIndex.KeyHeader, "name_199");
        RecordSet recordSetFK1 = new IndexRangeProvider(secondaryIndexProvider.Result, new Range<Record>(from1, to1)).Result;
        foreach (Record record in recordSetFK1) {
          //Console.Out.WriteLine(record);
        }

        IEnumerable<KeyValuePair<Record, Record>> items = secondaryIndex.GetItems(new Range<Record>(from1, to1));
        IList<KeyValuePair<Record, Record>> result = new List<KeyValuePair<Record, Record>>(items);
        Assert.AreNotEqual(0, result.Count);
      }
    }
  }
}