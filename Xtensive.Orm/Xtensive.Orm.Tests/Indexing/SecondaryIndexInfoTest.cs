// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

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
      primaryKey = new ColumnInfo(table, "key", new TypeInfo(typeof(int), null));
      primaryValue1 = new ColumnInfo(table, "value1", new TypeInfo(typeof(int), null));
      primaryValue2 = new ColumnInfo(table, "value2", new TypeInfo(typeof(int), null));
      new KeyColumnRef(primary, primaryKey, Direction.Positive);
      new ValueColumnRef(primary, primaryValue1);
      new ValueColumnRef(primary, primaryValue2);
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
      int countBefore = table.SecondaryIndexes.Count;
      AssertEx.Throws<ArgumentException>(() => new SecondaryIndexInfo(null, "secondary2"));
      AssertEx.Throws<ArgumentException>(() => new SecondaryIndexInfo(table, ""));
      Assert.AreEqual(countBefore, table.SecondaryIndexes.Count);
    }

    [Test]
    public void AddRemoveKeyColumnsTest()
    {
      new KeyColumnRef(secondary, primaryValue1, Direction.Negative);
      new KeyColumnRef(secondary, primaryValue2, Direction.Negative);
      Assert.AreEqual(2, secondary.KeyColumns.Count);
      Dump();
      secondary.KeyColumns[0].Remove();
      Assert.AreEqual(1, secondary.KeyColumns.Count);
    }

    [Test]
    public void ValidateEmptyKey()
    {
      AssertEx.Throws<AggregateException>(secondary.Validate);
    }

    [Test]
    public void ValidateDoubleKeys()
    {
      new KeyColumnRef(secondary, primaryValue1, Direction.Negative);
      new KeyColumnRef(secondary, primaryValue1, Direction.Positive);

      AssertEx.Throws<AggregateException>(secondary.Validate);
    }

    [Test]
    public void ValidateKeyIsNotPrimaryValue()
    {
      new KeyColumnRef(secondary, primaryKey, Direction.Positive);
    }

    [TearDown]
    public void Dump()
    {
      storage.Dump();
    }
  }
}