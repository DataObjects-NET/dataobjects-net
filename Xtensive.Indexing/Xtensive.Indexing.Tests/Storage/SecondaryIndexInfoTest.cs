// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Indexing.Storage.Model;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class SecondaryIndexInfoTest
  {
    private StorageInfo storage;
    private PrimaryIndexInfo primary;
    private SecondaryIndexInfo secondary;
    private ColumnInfo primaryKey;
    private ColumnInfo primaryValue1;
    private ColumnInfo primaryValue2;

    //[SetUp]
    //public void CreateModel()
    //{
    //  storage = new StorageInfo("storage1");
    //  primary = new PrimaryIndexInfo(storage, "primary1");
    //  secondary = new SecondaryIndexInfo(primary, "secondary1");
    //  primaryKey = new ColumnInfo(primary, "key");
    //  primaryValue1 = new ColumnInfo(primary, "value1");
    //  primaryValue2 = new ColumnInfo(primary, "value2");
    //  new PrimaryKeyColumnRef(primary, primaryKey, 0, Direction.Positive);
    //  new PrimaryValueColumnRef(primary, primaryValue1, 0);
    //  new PrimaryValueColumnRef(primary, primaryValue2, 1);
    //}

    //[Test]
    //public void ConstructorTest()
    //{
    //  var coutBefore = primary.SecondaryIndexes.Count;
    //  new SecondaryIndexInfo(primary, "secondary2");
    //  Assert.AreEqual(coutBefore + 1, primary.SecondaryIndexes.Count);
    //}

    //[Test]
    //public void DenyConstructor()
    //{
    //  int countBefore = storage.PrimaryIndexes.Count;
    //  AssertEx.Throws<ArgumentException>(() => new SecondaryIndexInfo(null, "secondary2"));
    //  AssertEx.Throws<ArgumentException>(() => new SecondaryIndexInfo(primary, ""));
    //  Assert.AreEqual(countBefore, storage.PrimaryIndexes.Count);
    //}

    //[Test]
    //public void AddRemoveKeyColumnsTest()
    //{
    //  new SecondaryKeyColumnRef(secondary, primaryValue1, 0, Direction.Negative);
    //  new SecondaryKeyColumnRef(secondary, primaryValue2, 1, Direction.Negative);
    //  Assert.AreEqual(2, secondary.SecondaryKeyColumns.Count);
    //  Dump();
    //  secondary.SecondaryKeyColumns[0].Remove();
    //  Assert.AreEqual(1, secondary.SecondaryKeyColumns.Count);
    //}

    //[Test]
    //public void ValidateEmptyKey()
    //{
    //  AssertEx.Throws<Exception>(secondary.Validate);
    //}

    //[Test]
    //public void ValidateDoubleKeys()
    //{
    //  new SecondaryKeyColumnRef(secondary, primaryValue1, 0, Direction.Negative);
    //  new SecondaryKeyColumnRef(secondary, primaryValue1, 1, Direction.Positive);

    //  AssertEx.Throws<Exception>(secondary.Validate);
    //}

    //[Test]
    //public void ValidateKeyIsNotPrimaryValue()
    //{
    //  new SecondaryKeyColumnRef(secondary, primaryKey, 0, Direction.Positive);
    //}

    //[TearDown]
    //public void Dump()
    //{
    //  storage.Dump();
    //}
  }
}