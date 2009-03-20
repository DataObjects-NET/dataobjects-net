// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Core.Testing;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class StorageInfoTest
  {
    protected StorageInfo storage;
    protected PrimaryIndexInfo pi1;
    protected PrimaryIndexInfo pi2;
    protected SecondaryIndexInfo si1;
    protected SecondaryIndexInfo si2;
    protected ColumnInfo column1;
    protected ColumnInfo column2;
    protected ColumnInfo column3;
    protected ColumnInfo column4;
    protected ColumnInfo column5;

    [TestFixtureSetUp]
    public void CreateModel()
    {
      storage = new StorageInfo("storage");
      //Primary 1
      pi1 = new PrimaryIndexInfo(storage, "pi1");
      column1 = new ColumnInfo(pi1, "col1");
      column2 = new ColumnInfo(pi1, "col2");
      column3 = new ColumnInfo(pi1, "col3");
      column4 = new ColumnInfo(pi1, "col4");
      column5 = new ColumnInfo(pi1, "col5");
      new PrimaryKeyColumnRef(pi1, column1, 0, ColumnDirection.Positive);
      new PrimaryKeyColumnRef(pi1, column2, 1, ColumnDirection.Negative);
      new PrimaryKeyColumnRef(pi1, column3, 2, ColumnDirection.Positive);
      new PrimaryValueColumnRef(pi1, column4, 0);
      new PrimaryValueColumnRef(pi1, column5, 1);
      si1 = new SecondaryIndexInfo(pi1, "si1");
      si2 = new SecondaryIndexInfo(pi1, "si2");

      //Primary 2
      pi2 = new PrimaryIndexInfo(storage, "pi2");
      new SecondaryKeyColumnRef(si1, column4, 0, ColumnDirection.Positive);
      new SecondaryKeyColumnRef(si1, column1, 1, ColumnDirection.Negative);
      new SecondaryKeyColumnRef(si2, column1, 0, ColumnDirection.Positive);
    }

    

    [Test]
    public void Dump()
    {
      storage.Dump();
    }

  }
}