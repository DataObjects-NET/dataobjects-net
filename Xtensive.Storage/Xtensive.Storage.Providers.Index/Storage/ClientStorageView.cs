// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using Xtensive.Integrity.Transactions;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers.Index.Storage;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  public class ClientStorageView : IStorageView
  {
    private IndexStorage storage;
    
    private Guid transactionId;

    protected IndexStorageView RealStorageView { get; private set; }

    public StorageInfo Model
    {
      get
      {
        EnsureRealViewIsAlive();
        return RealStorageView.Model;
      }
    }

    public void Update(ActionSequence sequence)
    {
      EnsureRealViewIsAlive();
      RealStorageView.Update(sequence);
    }

    public CommandResult Execute(Command command)
    {
      EnsureRealViewIsAlive();
      return RealStorageView.Execute(command);
    }

    public Dictionary<int, CommandResult> Execute(List<Command> commands)
    {
      EnsureRealViewIsAlive();
      return RealStorageView.Execute(commands);
    }

    public ITransaction Transaction
    {
      get
      {
        EnsureRealViewIsAlive();
        return RealStorageView.Transaction;
      }
    }

    private void EnsureRealViewIsAlive()
    {
      try {
        RealStorageView.Ping();
      }
      catch (SocketException) {
        Refresh();
      }
    }

    private void Refresh()
    {
      var newClientView = storage.GetView(transactionId) as ClientStorageView;
      RealStorageView = newClientView.RealStorageView;
    }

    public ClientStorageView(IndexStorageView realStorageView)
    {
      RealStorageView = realStorageView;
      transactionId = realStorageView.Transaction.Identifier;
      storage = realStorageView.Storage;
    }
  }
}