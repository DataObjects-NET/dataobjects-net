// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Tests.Indexing
{
  [TestFixture]
  public class StorageActionTest
  {
    private StorageModel storage;
    private TableInfo table;
    private PrimaryIndexInfo primary;
    private SecondaryIndexInfo secondary;
    private StorageColumnInfo primaryKey;
    private StorageColumnInfo primaryValue1;
    private StorageColumnInfo primaryValue2;

    [SetUp]
    public void CreateModel()
    {
      storage = new StorageModel("STORAGE") { Actions = new ActionSequence() };
      table = new TableInfo(storage, "TABLE");
      primary = new PrimaryIndexInfo(table, "PK");
      secondary = new SecondaryIndexInfo(table, "IX");
      primaryKey = new StorageColumnInfo(table, "ID", new StorageTypeInfo(typeof(int), null));
      primaryValue1 = new StorageColumnInfo(table, "AGE", new StorageTypeInfo(typeof(int), null));
      primaryValue2 = new StorageColumnInfo(table, "NAME", new StorageTypeInfo(typeof(int), null));
      new KeyColumnRef(primary, primaryKey, Direction.Positive);
      new ValueColumnRef(primary, primaryValue1);
      new ValueColumnRef(primary, primaryValue2);
      new KeyColumnRef(secondary, primaryValue1, Direction.Positive);

      storage.Dump();
    }

    [Test]
    public void ApplyActionsTest()
    {
      var newStorage = new StorageModel("STORAGE") { Actions = new ActionSequence() };
      Log.Info("Actions: \n{0}", storage.Actions);
      storage.Actions.Apply(newStorage);
      newStorage.Dump();
      storage.Actions = new ActionSequence();
      primaryValue1.Type = new StorageTypeInfo(typeof(string), null);
      storage.Actions.Apply(newStorage);
      newStorage.Dump();
    }
  }
}