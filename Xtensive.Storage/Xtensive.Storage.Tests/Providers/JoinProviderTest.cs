// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.15

using System;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Indexing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Storage;

namespace Xtensive.Storage.Tests.Query.Providers
{
  [TestFixture]
  public class JoinProviderTest
  {
    private TupleDescriptor pKeyDescriptor;
    private TupleDescriptor entityValueDescriptor;
    private MemoryIndex primaryIndex;
    private MemoryIndex fkIndex1;
    private MemoryIndex fkIndex2;
    private const int itemCount = 10000;

    [TestFixtureSetUp]
    public void SetUp()
    {
      pKeyDescriptor = new TupleDescriptor();
      pKeyDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("id", typeof (int)));
      pKeyDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("typeID", typeof (Guid)));
      pKeyDescriptor.Lock();
      entityValueDescriptor = new TupleDescriptor();
      entityValueDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("name", typeof (string)));
      entityValueDescriptor.FieldDescriptors.Add(FieldDescriptor.CreateFieldDescriptor("tag", typeof (int)));
      entityValueDescriptor.Lock();

      using (new Measurement(itemCount)) {
        RecordHeader keyHeader = new RecordHeader(pKeyDescriptor, CultureInfo.InvariantCulture);
        RecordHeader entityValueHeader = new RecordHeader(entityValueDescriptor, CultureInfo.InvariantCulture);
        RecordHeader recordHeader = new RecordHeader(keyHeader, entityValueHeader);

        DirectionCollection<RecordColumn> pkSortOrder = new DirectionCollection<RecordColumn>();
        pkSortOrder.Add(recordHeader.RecordColumnCollection["id"], Direction.Positive);
        pkSortOrder.Add(recordHeader.RecordColumnCollection["typeID"], Direction.Negative);

        DirectionCollection<RecordColumn> fk1SortOrder = new DirectionCollection<RecordColumn>();
        fk1SortOrder.Add(recordHeader.RecordColumnCollection["name"], Direction.Positive);

        DirectionCollection<RecordColumn> fk2SortOrder = new DirectionCollection<RecordColumn>();
        fk2SortOrder.Add(recordHeader.RecordColumnCollection["typeID"], Direction.Positive);

        primaryIndex = new MemoryIndex(recordHeader, pkSortOrder);
        fkIndex1 = new MemoryIndex(recordHeader, fk1SortOrder, pkSortOrder);
        fkIndex2 = new MemoryIndex(recordHeader, fk2SortOrder, pkSortOrder);
        Random random = new Random((int)DateTime.Now.Ticks);

        for (int i = 0; i < itemCount; i++) {
          ITuple keyTuple = pKeyDescriptor.CreateTuple();
          ITuple valueTuple = entityValueDescriptor.CreateTuple();
          keyTuple.SetValue("id", i);
          keyTuple.SetValue("typeID", Guid.NewGuid());
          valueTuple.SetValue("name", "name_" + i/10);
          valueTuple.SetValue("tag", random.Next(itemCount));
          Record record = new Record(recordHeader, keyTuple, valueTuple);
          primaryIndex.Add(record);
          fkIndex1.Add(record);
          fkIndex2.Add(record);
        }
      }
    }

    [Test]
    public void Join()
    {
      Rse.Providers.IndexProvider fk1Provider = new Rse.Providers.IndexProvider(fkIndex1);
      Rse.Providers.IndexProvider fk2Provider = new Rse.Providers.IndexProvider(fkIndex2);

      Record from1 = new Record(fkIndex1.KeyHeader, "name_10");
      Record to1 = new Record(fkIndex1.KeyHeader, "name_199");
      RecordSet recordSetFK1 = new IndexRangeProvider(fk1Provider.Result, new Range<Record>(from1, to1)).Result;

      Record from2 = new Record(fkIndex2.KeyHeader, null, 10);
      Record to2 = new Record(fkIndex2.KeyHeader, null, 1000);
      RecordSet recordSetFK2 = new IndexRangeProvider(fk2Provider.Result, new Range<Record>(from2, to2)).Result;

      JoinProvider joinProvider =
        new JoinProvider(recordSetFK1,
                         recordSetFK2,
                         fkIndex1.RecordHeader.RecordColumnCollection["id"],
                         fkIndex2.RecordHeader.RecordColumnCollection["id"],
                         false);
      RecordSet result = joinProvider.Result;
      int count = 0;
      foreach (Record record in result) {
        count++;
      }
      Assert.AreNotEqual(0, count);
    }
  }
}