// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Orm.Upgrade.Model;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Orm.Tests.Indexing
{
  [TestFixture]
  public class PrimaryIndexInfoTest
  {
    private StorageModel storage;
    private TableInfo table;
    private PrimaryIndexInfo index;

    [SetUp]
    public void CreateStorage()
    {
      storage = new StorageModel("s1");
      table = new TableInfo(storage, "table");
      index = new PrimaryIndexInfo(table, "i");
    }

    [Test]
    public void DenyIndexInfoConstructor()
    {
      var indexBefore = table.PrimaryIndex;
      AssertEx.Throws<ArgumentException>(() => new PrimaryIndexInfo(null, "i2"));
      AssertEx.Throws<ArgumentException>(() => new PrimaryIndexInfo(table, ""));
      Assert.AreEqual(indexBefore, table.PrimaryIndex);
    }

    [Test]
    public void AddRemoveColumnsTest()
    {
      var column = new StorageColumnInfo(table, "c");
      Assert.AreEqual(1, table.Columns.Count);
      column.Remove();
      Assert.AreEqual(0, table.Columns.Count);
    }

    [Test]
    public void DenyAddColumnTest()
    {
      var column = new StorageColumnInfo(table, "c");
      AssertEx.Throws<ArgumentException>(() => new StorageColumnInfo(table, "c"));
    }

    [Test]
    public void AddRemoveKeyColumnRefs()
    {
      var column = new StorageColumnInfo(table, "col1");
      var colRef = new KeyColumnRef(index, column, Direction.Positive);
      Assert.AreEqual(1, index.KeyColumns.Count);
      colRef.Remove();
      Assert.AreEqual(0, index.KeyColumns.Count);
      column.Remove();
    }

    [Test]
    public void AddRemoveValueColumnRefs()
    {
      var column = new StorageColumnInfo(table, "col1");
      var colRef = new ValueColumnRef(index, column);
      Assert.AreEqual(1, index.ValueColumns.Count);
      colRef.Remove();
      Assert.AreEqual(0, index.ValueColumns.Count);
    }

    [Test]
    public void ValidateEmptyKeys()
    {
      new StorageColumnInfo(table, "c1", new StorageTypeInfo(typeof(string), null));
      new StorageColumnInfo(table, "c2", new StorageTypeInfo(typeof(string), null));

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateNullableKeyColumns()
    {
      var col = new StorageColumnInfo(table, "c2", new StorageTypeInfo(typeof (string), true, null));
      new KeyColumnRef(index, col, Direction.Positive);

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateDoubleColumnRefs()
    {
      var column = new StorageColumnInfo(table, "c");
      new KeyColumnRef(index, column, Direction.Positive);
      new ValueColumnRef(index, column);

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateNotReferencedColumns()
    {
      new KeyColumnRef(index, new StorageColumnInfo(table, "key"), Direction.Positive);
      new StorageColumnInfo(table, "col");

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateDoubleKeysAndValuesColumnRefs()
    {
      var key = new StorageColumnInfo(table, "key");
      var value = new StorageColumnInfo(table, "value");
      new KeyColumnRef(index, key, Direction.Positive);
      new KeyColumnRef(index, key, Direction.Negative);
      new ValueColumnRef(index, value);
      new ValueColumnRef(index, value);

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateRefToColumnFromAnotherIndex()
    {
      var anoterTable = new TableInfo(storage, "t2");
      var key = new StorageColumnInfo(anoterTable, "key");
      var value = new StorageColumnInfo(anoterTable, "value");
      
      new KeyColumnRef(index, key, Direction.Positive);
      new ValueColumnRef(index, value);

      AssertEx.Throws<AggregateException>(index.Validate);
    }


    [TearDown]
    public void Dump()
    {
      storage.Dump();
    }
  }
}