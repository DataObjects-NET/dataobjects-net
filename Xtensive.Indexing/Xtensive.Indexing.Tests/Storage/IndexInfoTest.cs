// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Core;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class IndexInfoTest
  {
    private StorageInfo storage;

    [SetUp]
    public void CreateStorage()
    {
      storage = new StorageInfo("storage");
    }

    [Test]
    public void ConstructorTest()
    {
      var index = new IndexInfo(storage, "index1");
    }

    [Test]
    public void AddColumnsTest()
    {
      var index = new SecondaryIndexInfo(storage, "pi1");
      storage.Indexes.Add(index);

      var column1 = new ColumnInfo(index, "c1", ColumnType.PrimaryKey, Direction.None);
      var column2 = new ColumnInfo(index, "c2", ColumnType.SecondaryKey, Direction.None);
      var column3 = new ColumnInfo(index, "c3", ColumnType.Value, Direction.None);
      index.Columns.Add(column1);
      index.Columns.Add(column2);
      index.Columns.Add(column3);

      Assert.AreEqual(2, index.KeyColumns.Count);
      Assert.AreEqual(column1, index.KeyColumns[0]);
      Assert.AreEqual(column2, index.KeyColumns[1]);
      Assert.AreEqual(1, index.PrimaryKeyColumns.Count);
      Assert.AreEqual(column1, index.PrimaryKeyColumns[0]);
      Assert.AreEqual(1, index.SecondaryKeyColumns.Count);
      Assert.AreEqual(column2, index.SecondaryKeyColumns[0]);
      Assert.AreEqual(1, index.ValueColumns.Count);
      Assert.AreEqual(column3, index.ValueColumns[0]);
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void DenyAddColumnsTest()
    {
      IndexInfo index = CreateIndex();

      var column1 = new ColumnInfo(index, "c1", ColumnType.PrimaryKey, Direction.Positive);
      var column2 = new ColumnInfo(index, "c1", ColumnType.Value, Direction.None);
      index.Columns.Add(column1);
      index.Columns.Add(column2);
    }

    [Test]
    [ExpectedException(typeof(InstanceIsLockedException))]
    public void RenameLockedTest()
    {
      var index = CreateIndex();
      index.Lock(false);
      index.Name = "newIndex";
    }

    [Test]
    [ExpectedException(typeof(InstanceIsLockedException))]
    public void AddColumnsToLockedTest()
    {
      var index = CreateIndex();
      index.Lock(false);
      var column = new ColumnInfo(index, "c", ColumnType.PrimaryKey, Direction.None);
      index.Columns.Add(column);
    }

    public IndexInfo CreateIndex()
    {
      var indexInfo = new IndexInfo(storage, "index1");
      storage.Indexes.Add(indexInfo);
      return indexInfo;
    }

  }
}