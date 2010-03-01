// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Modelling.Comparison;
using NUnit.Framework;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Core.Testing;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class StorageInfoTest
  {
    protected StorageInfo storage;
    protected TableInfo table1;
    protected TableInfo table2;
    protected PrimaryIndexInfo pi1;
    protected PrimaryIndexInfo pi2;
    protected SecondaryIndexInfo si1;
    protected SecondaryIndexInfo si2;
    protected ForeignKeyInfo fk1;
    protected ColumnInfo column1;
    protected ColumnInfo column2;
    protected ColumnInfo column3;
    protected ColumnInfo column4;
    protected ColumnInfo column5;

    [TestFixtureSetUp]
    public void CreateModel()
    {
      storage = new StorageInfo("storage") { Actions = new ActionSequence() };
      // Table 1
      table1 = new TableInfo(storage, "table1");
      pi1 = new PrimaryIndexInfo(table1, "pk1");
      column1 = new ColumnInfo(table1, "col1", new TypeInfo(typeof(string)));
      column2 = new ColumnInfo(table1, "col2", new TypeInfo(typeof(string)));
      column3 = new ColumnInfo(table1, "col3", new TypeInfo(typeof(string)));
      new KeyColumnRef(pi1, column1, 0, Direction.Positive);
      new ValueColumnRef(pi1, column2, 0);
      new ValueColumnRef(pi1, column3, 1);
      si1 = new SecondaryIndexInfo(table1, "ix1");
      new KeyColumnRef(si1, column2, 0, Direction.Positive);


      // Table 2
      table2 = new TableInfo(storage, "table2");
      pi2 = new PrimaryIndexInfo(table2, "pk2");
      column4 = new ColumnInfo(table2, "col4", new TypeInfo(typeof(int)));
      column5 = new ColumnInfo(table2, "col5", new TypeInfo(typeof(string)));
      new KeyColumnRef(pi2, column4, 0, Direction.Negative);
      new ValueColumnRef(pi2, column5, 0);
      si2 = new SecondaryIndexInfo(table2, "ix2");
      new KeyColumnRef(si2, column5, 0, Direction.Positive);

      // Foreign keys
      fk1 = new ForeignKeyInfo(table1, "fk1") { ReferencedIndex = si2 };
    }

    [Test]
    public void ValidateModel()
    {
      storage.Validate();
    }

    [Test]
    public void StorageLogTest()
    {
      Log.Info("Actions:");
      Log.Info("{0}", storage.Actions);
    }

    [Test]
    public void RemoveReferencedColumnTest()
    {
      column5.Remove();
      AssertEx.Throws<Exception>(storage.Validate);
    }

    [Test]
    public void RemoveReferencedSecondaryIndexTest()
    {
      si2.Remove();
      AssertEx.Throws<Exception>(storage.Validate);
    }

    [TearDown]
    public void Dump()
    {
      storage.Dump();
    }

  }
}