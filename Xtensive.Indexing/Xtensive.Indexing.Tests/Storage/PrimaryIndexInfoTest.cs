// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Core;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class PrimaryIndexInfoTest
  {

    private StorageInfo storage;
    private PrimaryIndexInfo index;

    [SetUp]
    public void CreateStorage()
    {
      storage = new StorageInfo("s1");
      index= new PrimaryIndexInfo(storage, "i");
    }

    [Test]
    public void ConstructorTest()
    {
      var countBefore = storage.PrimaryIndexes.Count;
      new PrimaryIndexInfo(storage, "i2");
      Assert.AreEqual(countBefore + 1, storage.PrimaryIndexes.Count);
    }

    [Test]
    public void DenyIndexInfoConstructor()
    {
      int countBefore = storage.PrimaryIndexes.Count;
      AssertEx.Throws<ArgumentException>(() => new PrimaryIndexInfo(null, "i2"));
      AssertEx.Throws<ArgumentException>(() => new PrimaryIndexInfo(storage, ""));
      Assert.AreEqual(countBefore, storage.PrimaryIndexes.Count);
    }

    [Test]
    public void AddRemoveColumnsTest()
    {
      var column = new ColumnInfo(index, "c");
      Assert.AreEqual(1, index.Columns.Count);
      column.Remove();
      Assert.AreEqual(0, index.Columns.Count);
    }

    [Test]
    public void DenyAddColumnTest()
    {
      var column = new ColumnInfo(index, "c");
      AssertEx.Throws<ArgumentException>(() => new ColumnInfo(index, "c"));
    }

    [Test]
    public void AddRemoveKeyColumnRefs()
    {
      var column = new ColumnInfo(index, "col1");
      var colRef = new PrimaryKeyColumnRef(index, column, 0, ColumnDirection.Positive);
      Assert.AreEqual(1, index.KeyColumns.Count);
      colRef.Remove();
      Assert.AreEqual(0, index.KeyColumns.Count);
    }

    [Test]
    public void AddRemoveValueColumnRefs()
    {
      var column = new ColumnInfo(index, "col1");
      var colRef = new PrimaryValueColumnRef(index, column, 0);
      Assert.AreEqual(1, index.ValueColumns.Count);
      colRef.Remove();
      Assert.AreEqual(0, index.ValueColumns.Count);
    }

    [Test]
    public void DenyAddDoubleColumnRefs()
    {
      var key = new ColumnInfo(index, "key");
      var value = new ColumnInfo(index, "value");
      new PrimaryKeyColumnRef(index, key, 0, ColumnDirection.Positive);
      new PrimaryValueColumnRef(index, value, 0);

      AssertEx.Throws<ArgumentException>(() => new PrimaryKeyColumnRef(index, key, 1, ColumnDirection.Positive));
      Assert.AreEqual(1, index.KeyColumns.Count);
      AssertEx.Throws<ArgumentException>(() => new PrimaryValueColumnRef(index, value, 1));
      Assert.AreEqual(1, index.ValueColumns.Count);
    }

    [Test]
    public void DenyAddRefToColumnFromAnotherIndex()
    {
      var anotherIndex = new PrimaryIndexInfo(storage, "i2");
      var key = new ColumnInfo(anotherIndex, "key");
      var value = new ColumnInfo(anotherIndex, "value");

      AssertEx.Throws<ArgumentException>(() =>
        new PrimaryKeyColumnRef(index, key, 0, ColumnDirection.Positive));
      Assert.AreEqual(0, index.KeyColumns.Count);
      AssertEx.Throws<ArgumentException>(() =>
        new PrimaryValueColumnRef(index, value, 1));
      Assert.AreEqual(0, index.ValueColumns.Count);
    }

    [Test]
    public void DenyAddKeyAndValueColumnRef()
    {
      var column = new ColumnInfo(index, "c");
      new PrimaryKeyColumnRef(index, column, 0, ColumnDirection.Positive);
      AssertEx.Throws<ArgumentException>(()=>new PrimaryValueColumnRef(index, column, 0));
    }

    [TearDown]
    public void Dump()
    {
      storage.Dump();
    }

  }
}