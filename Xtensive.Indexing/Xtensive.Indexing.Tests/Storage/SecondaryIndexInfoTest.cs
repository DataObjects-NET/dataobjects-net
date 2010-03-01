// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Indexing.Storage;
using Xtensive.Indexing.Storage.Model;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class SecondaryIndexInfoTest
  {
    private StorageInfo storage;
    private TableInfo table;
    private PrimaryIndexInfo primary;
    private SecondaryIndexInfo secondary;
    private ColumnInfo primaryKey;
    private ColumnInfo primaryValue1;
    private ColumnInfo primaryValue2;

    [SetUp]
    public void CreateModel()
    {
      storage = new StorageInfo("storage1");
      table = new TableInfo(storage, "table");
      primary = new PrimaryIndexInfo(table, "primary1");
      secondary = new SecondaryIndexInfo(table, "secondary1");
      primaryKey = new ColumnInfo(table, "key", new TypeInfo(typeof(int)));
      primaryValue1 = new ColumnInfo(table, "value1", new TypeInfo(typeof(int)));
      primaryValue2 = new ColumnInfo(table, "value2", new TypeInfo(typeof(int)));
      new KeyColumnRef(primary, primaryKey, 0, Direction.Positive);
      new ValueColumnRef(primary, primaryValue1, 0);
      new ValueColumnRef(primary, primaryValue2, 1);
    }

    [Test]
    public void ConstructorTest()
    {
      var coutBefore = table.SecondaryIndexes.Count;
      new SecondaryIndexInfo(table, "secondary2");
      Assert.AreEqual(coutBefore + 1, table.SecondaryIndexes.Count);
    }

    [Test]
    public void DenyConstructor()
    {
      int countBefore = storage.PrimaryIndexes.Count;
      AssertEx.Throws<ArgumentException>(() => new SecondaryIndexInfo(null, "secondary2"));
      AssertEx.Throws<ArgumentException>(() => new SecondaryIndexInfo(table, ""));
      Assert.AreEqual(countBefore, storage.PrimaryIndexes.Count);
    }

    [Test]
    public void AddRemoveKeyColumnsTest()
    {
      new KeyColumnRef(secondary, primaryValue1, 0, Direction.Negative);
      new KeyColumnRef(secondary, primaryValue2, 1, Direction.Negative);
      Assert.AreEqual(2, secondary.KeyColumns.Count);
      Dump();
      secondary.KeyColumns[0].Remove();
      Assert.AreEqual(1, secondary.KeyColumns.Count);
    }

    [Test]
    public void ValidateEmptyKey()
    {
      AssertEx.Throws<IntegrityException>(secondary.Validate);
    }

    [Test]
    public void ValidateDoubleKeys()
    {
      new KeyColumnRef(secondary, primaryValue1, 0, Direction.Negative);
      new KeyColumnRef(secondary, primaryValue1, 1, Direction.Positive);

      AssertEx.Throws<IntegrityException>(secondary.Validate);
    }

    [Test]
    public void ValidateKeyIsNotPrimaryValue()
    {
      new KeyColumnRef(secondary, primaryKey, 0, Direction.Positive);
    }

    [TearDown]
    public void Dump()
    {
      storage.Dump();
    }
  }
}