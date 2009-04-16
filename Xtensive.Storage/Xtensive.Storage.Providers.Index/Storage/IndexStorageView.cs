// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using Xtensive.Integrity.Transactions;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Index;

namespace Xtensive.Storage.Providers.Index.Storage
{
  public abstract class IndexStorageView : MarshalByRefObject, IStorageView
  {
    public IndexStorage Storage { get; private set; }

    public StorageInfo Model { get; private set; }

    public abstract void Update(ActionSequence sequence);

    public abstract CommandResult Execute(Command command);

    public abstract Dictionary<int, CommandResult> Execute(List<Command> commands);

    public abstract ITransaction Transaction { get; }

    public abstract void ClearSchema();

    public abstract void CreateNewSchema(StorageInfo newSchema);

    public void Ping()
    {
    }

    protected IndexStorageView(IndexStorage storage, StorageInfo model)
    {
      Storage = storage;
      Model = model;
    }
  }
}