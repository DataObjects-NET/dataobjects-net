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
using Xtensive.Core.Helpers;

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
    public void ValidateEmptyKeys()
    {
      new ColumnInfo(index, "c1");
      new ColumnInfo(index, "c2");

      AssertEx.Throws<Exception>(index.Validate);
    }

    [Test]
    public void ValidateDoubleColumnRefs()
    {
      var column = new ColumnInfo(index, "c");
      new PrimaryKeyColumnRef(index, column, 0, ColumnDirection.Positive);
      new PrimaryValueColumnRef(index, column, 0);

      AssertEx.Throws<Exception>(index.Validate);
    }

    [Test]
    public void ValidateNotReferencedColumns()
    {
      new PrimaryKeyColumnRef(index, new ColumnInfo(index, "key"), 0, ColumnDirection.Positive);
      new ColumnInfo(index, "col");

      AssertEx.Throws<Exception>(index.Validate);
    }

    [Test]
    public void ValidateDoubleKeysAndValuesColumnRefs()
    {
      var key = new ColumnInfo(index, "key");
      var value = new ColumnInfo(index, "value");
      new PrimaryKeyColumnRef(index, key, 0, ColumnDirection.Positive);
      new PrimaryKeyColumnRef(index, key, 1, ColumnDirection.Negative);
      new PrimaryValueColumnRef(index, value, 0);
      new PrimaryValueColumnRef(index, value, 1);

      AssertEx.Throws<Exception>(index.Validate);
    }

    [Test]
    public void ValidateRefToColumnFromAnotherIndex()
    {
      var anotherIndex = new PrimaryIndexInfo(storage, "i2");
      var key = new ColumnInfo(anotherIndex, "key");
      var value = new ColumnInfo(anotherIndex, "value");
      new PrimaryKeyColumnRef(index, key, 0, ColumnDirection.Positive);
      new PrimaryValueColumnRef(index, value, 0);

      AssertEx.Throws<Exception>(index.Validate);
    }


    [TearDown]
    public void Dump()
    {
      storage.Dump();
    }

  }
}