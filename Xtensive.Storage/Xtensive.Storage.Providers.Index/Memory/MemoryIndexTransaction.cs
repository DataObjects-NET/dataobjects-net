// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using System;
using System.Transactions;
using Xtensive.Internals.DocTemplates;
using Xtensive.Transactions;

namespace Xtensive.Storage.Providers.Index.Memory
{
  /// <summary>
  /// Indexing storage transaction.
  /// </summary>
  [Serializable]
  public class MemoryIndexTransaction : IndexTransaction
  {
    private TransactionState state;

    private DateTime timeStamp;

    /// <inheritdoc/>
    public override DateTime TimeStamp
    {
      get { return timeStamp; }
    }

    /// <inheritdoc/>
    public override void Commit()
    {
      if (state != TransactionState.Active)
        throw new InvalidOperationException();

      state = TransactionState.Committed;
    }

    /// <inheritdoc/>
    public override void Rollback()
    {
      state = TransactionState.RolledBack;
    }

    /// <inheritdoc/>
    public override TransactionState State
    {
      get { return state; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    public MemoryIndexTransaction(Guid identifier, IsolationLevel isolationLevel)
    {
      Identifier = identifier;
      IsolationLevel = isolationLevel;
      state = TransactionState.Active;
      timeStamp = DateTime.Now;
    }
  }
}