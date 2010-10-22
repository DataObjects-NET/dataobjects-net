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
using Xtensive.Storage.StorageModel;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Storage.Tests.Indexing
{
  [TestFixture]
  public class PrimaryIndexInfoTest
  {
    private StorageInfo storage;
    private TableInfo table;
    private PrimaryIndexInfo index;

    [SetUp]
    public void CreateStorage()
    {
      storage = new StorageInfo("s1");
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
      var column = new ColumnInfo(table, "c");
      Assert.AreEqual(1, table.Columns.Count);
      column.Remove();
      Assert.AreEqual(0, table.Columns.Count);
    }

    [Test]
    public void DenyAddColumnTest()
    {
      var column = new ColumnInfo(table, "c");
      AssertEx.Throws<ArgumentException>(() => new ColumnInfo(table, "c"));
    }

    [Test]
    public void AddRemoveKeyColumnRefs()
    {
      var column = new ColumnInfo(table, "col1");
      var colRef = new KeyColumnRef(index, column, Direction.Positive);
      Assert.AreEqual(1, index.KeyColumns.Count);
      colRef.Remove();
      Assert.AreEqual(0, index.KeyColumns.Count);
      column.Remove();
    }

    [Test]
    public void AddRemoveValueColumnRefs()
    {
      var column = new ColumnInfo(table, "col1");
      var colRef = new ValueColumnRef(index, column);
      Assert.AreEqual(1, index.ValueColumns.Count);
      colRef.Remove();
      Assert.AreEqual(0, index.ValueColumns.Count);
    }

    [Test]
    public void ValidateEmptyKeys()
    {
      new ColumnInfo(table, "c1", new TypeInfo(typeof(string), null));
      new ColumnInfo(table, "c2", new TypeInfo(typeof(string), null));

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateNullableKeyColumns()
    {
      var col = new ColumnInfo(table, "c2", new TypeInfo(typeof (string), true, null));
      new KeyColumnRef(index, col, Direction.Positive);

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateDoubleColumnRefs()
    {
      var column = new ColumnInfo(table, "c");
      new KeyColumnRef(index, column, Direction.Positive);
      new ValueColumnRef(index, column);

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateNotReferencedColumns()
    {
      new KeyColumnRef(index, new ColumnInfo(table, "key"), Direction.Positive);
      new ColumnInfo(table, "col");

      AssertEx.Throws<AggregateException>(index.Validate);
    }

    [Test]
    public void ValidateDoubleKeysAndValuesColumnRefs()
    {
      var key = new ColumnInfo(table, "key");
      var value = new ColumnInfo(table, "value");
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
      var key = new ColumnInfo(anoterTable, "key");
      var value = new ColumnInfo(anoterTable, "value");
      
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