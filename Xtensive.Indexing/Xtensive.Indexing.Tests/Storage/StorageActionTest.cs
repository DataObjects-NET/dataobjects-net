// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Indexing.Storage.Model;
using NUnit.Framework;
using Xtensive.Modelling.Actions;

namespace Xtensive.Indexing.Tests.Storage
{
  [TestFixture]
  public class StorageActionTest
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
    //  storage = new StorageInfo("STORAGE") { Actions = new ActionSequence() };
    //  primary = new PrimaryIndexInfo(storage, "PK");
    //  secondary = new SecondaryIndexInfo(primary, "IX");
    //  primaryKey = new ColumnInfo(primary, "ID", typeof(int));
    //  primaryValue1 = new ColumnInfo(primary, "AGE", typeof(string));
    //  primaryValue2 = new ColumnInfo(primary, "NAME", typeof(string));
    //  new PrimaryKeyColumnRef(primary, primaryKey, 0, Direction.Positive);
    //  new PrimaryValueColumnRef(primary, primaryValue1, 0);
    //  new PrimaryValueColumnRef(primary, primaryValue2, 1);
    //  new SecondaryKeyColumnRef(secondary, primaryValue1, 0, Direction.Positive);

    //  storage.Dump();
    //}

    //[Test]
    //public void ApplyActionsTest()
    //{
    //  var newStorage = new StorageInfo("STORAGE") { Actions = new ActionSequence() };
    //  Log.Info("Actions: \n{0}", storage.Actions);
    //  storage.Actions.Apply(newStorage);
    //  newStorage.Dump();

    //  storage.Actions = new ActionSequence();
    //  primaryValue1.ColumnType = typeof(int);
    //  storage.Actions.Apply(newStorage);
    //  newStorage.Dump();
    //}
  }
}