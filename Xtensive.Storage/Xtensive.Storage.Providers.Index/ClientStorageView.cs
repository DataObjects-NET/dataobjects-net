// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Integrity.Transactions;
using Xtensive.Modelling.Actions;
using Xtensive.Storage.Indexing;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// Client-side storage view proxy.
  /// </summary>
  [Serializable]
  public class ClientStorageView : IStorageView
  {
    private IndexStorage storage;
    private Guid transactionId;

    /// <summary>
    /// Gets the real storage view.
    /// </summary>
    protected IndexStorageView RealStorageView { get; private set; }

    /// <inheritdoc/>
    public StorageInfo Model {
      get {
        EnsureRealViewIsAlive();
        return RealStorageView.Model;
      }
    }

    /// <inheritdoc/>
    public ITransaction Transaction {
      get {
        EnsureRealViewIsAlive();
        return RealStorageView.Transaction;
      }
    }

    /// <inheritdoc/>
    public CommandResult Execute(Command command)
    {
      EnsureRealViewIsAlive();
      return RealStorageView.Execute(command);
    }

    /// <inheritdoc/>
    public Dictionary<int, CommandResult> Execute(List<Command> commands)
    {
      EnsureRealViewIsAlive();
      return RealStorageView.Execute(commands);
    }

    /// <inheritdoc/>
    public void Update(ActionSequence sequence)
    {
      EnsureRealViewIsAlive();
      RealStorageView.Update(sequence);
    }

    /// <inheritdoc/>
    public IUniqueOrderedIndex<Tuple, Tuple> GetIndex(IndexInfo indexInfo)
    {
      EnsureRealViewIsAlive();
      return RealStorageView.GetIndex(indexInfo);
    }

    #region Private / internal methods

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

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="realStorageView">The real storage view.</param>
    public ClientStorageView(IndexStorageView realStorageView)
    {
      RealStorageView = realStorageView;
      transactionId = realStorageView.Transaction.Identifier;
      storage = realStorageView.Storage;
    }
  }
}