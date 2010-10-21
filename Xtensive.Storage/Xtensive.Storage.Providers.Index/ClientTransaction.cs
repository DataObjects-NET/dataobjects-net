// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.15

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Transactions;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// Client-side transaction object proxy.
  /// </summary>
  [Serializable]
  public class ClientTransaction : ITransaction
  {
    protected ITransaction realTransaction;

    /// <inheritdoc/>
    object IIdentified.Identifier {
      get { return Identifier; }
    }

    /// <inheritdoc/>
    public Guid Identifier {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public void Commit()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Rollback()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public TransactionState State
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public IsolationLevel IsolationLevel
    {
      get { throw new NotImplementedException(); }
    }

    /// <inheritdoc/>
    public DateTime TimeStamp
    {
      get { throw new NotImplementedException(); }
    }
  }
}