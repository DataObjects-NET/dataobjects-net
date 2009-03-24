// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Core.Testing;
using Xtensive.Core;
using Xtensive.Modelling.Actions;

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
    protected ForeignKeyInfo fk1;
    protected ColumnInfo column1;
    protected ColumnInfo column2;
    protected ColumnInfo column3;
    protected ColumnInfo column4;
    protected ColumnInfo column5;

    [TestFixtureSetUp]
    public void CreateModel()
    {
      storage = new StorageInfo("storage") {Actions = new ActionSequence()};
      //Primary 1
      pi1 = new PrimaryIndexInfo(storage, "pi1");
      column1 = new ColumnInfo(pi1, "col1", typeof(string));
      column2 = new ColumnInfo(pi1, "col2", typeof(string));
      column3 = new ColumnInfo(pi1, "col3", typeof(string));
      new PrimaryKeyColumnRef(pi1, column1, 0, ColumnDirection.Positive);
      new PrimaryValueColumnRef(pi1, column2, 0);
      new PrimaryValueColumnRef(pi1, column3, 1);
      si1 = new SecondaryIndexInfo(pi1, "si1");
      new SecondaryKeyColumnRef(si1, column2, 0, ColumnDirection.Positive);
      

      //Primary 2
      pi2 = new PrimaryIndexInfo(storage, "pi2");
      column4 = new ColumnInfo(pi2, "col4", typeof(int));
      column5 = new ColumnInfo(pi2, "col5", typeof(int));
      new PrimaryKeyColumnRef(pi2, column4, 0, ColumnDirection.Negative);
      new PrimaryValueColumnRef(pi2, column5, 0);
      si2 = new SecondaryIndexInfo(pi2, "si2");
      new SecondaryKeyColumnRef(si2, column5, 0, ColumnDirection.Positive);

      //fk1 = new ForeignKeyInfo(pi1, si2, "fk1");
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