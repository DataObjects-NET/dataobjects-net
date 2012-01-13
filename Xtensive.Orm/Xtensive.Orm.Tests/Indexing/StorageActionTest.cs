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
      storage = new StorageInfo("STORAGE") { Actions = new ActionSequence() };
      table = new TableInfo(storage, "TABLE");
      primary = new PrimaryIndexInfo(table, "PK");
      secondary = new SecondaryIndexInfo(table, "IX");
      primaryKey = new ColumnInfo(table, "ID", new TypeInfo(typeof(int), null));
      primaryValue1 = new ColumnInfo(table, "AGE", new TypeInfo(typeof(int), null));
      primaryValue2 = new ColumnInfo(table, "NAME", new TypeInfo(typeof(int), null));
      new KeyColumnRef(primary, primaryKey, Direction.Positive);
      new ValueColumnRef(primary, primaryValue1);
      new ValueColumnRef(primary, primaryValue2);
      new KeyColumnRef(secondary, primaryValue1, Direction.Positive);

      storage.Dump();
    }

    [Test]
    public void ApplyActionsTest()
    {
      var newStorage = new StorageInfo("STORAGE") { Actions = new ActionSequence() };
      Log.Info("Actions: \n{0}", storage.Actions);
      storage.Actions.Apply(newStorage);
      newStorage.Dump();
      storage.Actions = new ActionSequence();
      primaryValue1.Type = new TypeInfo(typeof(string), null);
      storage.Actions.Apply(newStorage);
      newStorage.Dump();
    }
  }
}