// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.16

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Transactions;

namespace Xtensive.Orm.Providers.Indexing
{
  /// <summary>
  /// An abstract base implementation of <see cref="ITransaction"/>
  /// for indexing engines.
  /// </summary>
  [Serializable]
  public abstract class IndexTransaction : ITransaction
  {
    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      get { return Identifier; }
    }

    /// <inheritdoc/>
    public Guid Identifier { get; protected set; }

    /// <inheritdoc/>
    public IsolationLevel IsolationLevel { get; protected set; }

    /// <inheritdoc/>
    public abstract DateTime TimeStamp { get; }

    /// <inheritdoc/>
    public abstract void Commit();

    /// <inheritdoc/>
    public abstract void Rollback();

    /// <inheritdoc/>
    public abstract TransactionState State { get; }
  }
}