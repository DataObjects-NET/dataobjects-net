// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.13

using System;
using Xtensive.Core;
using Xtensive.Integrity.Transactions;

namespace Xtensive.TransactionLog
{
  /// <summary>
  /// Basic implementation for <see cref="ITransactionInfo{TKey}"/>.
  /// </summary>
  /// <typeparam name="TKey">Type of key.</typeparam>
  [Serializable]
  public sealed class TransactionInfo<TKey> : ITransactionInfo<TKey> 
    where TKey : IComparable<TKey>
  {
    private readonly TKey identifier;
    private readonly TransactionState state;
    private readonly object data;


    /// <inheritdoc/>
    public TKey Identifier
    {
      get { return identifier; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      get { return identifier; }
    }

    /// <inheritdoc/>
    public TransactionState State
    {
      get { return state; }
    }

    /// <inheritdoc/>
    public object Data
    {
      get { return data; }
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="TransactionInfo{TKey}"/>.
    /// </summary>
    /// <param name="identifier">Transaction identifier.</param>
    /// <param name="state">Transaction state.</param>
    /// <param name="data">Transaction data.</param>
    public TransactionInfo(TKey identifier, TransactionState state, object data)
    {
      this.identifier = identifier;
      this.state = state;
      this.data = data;
    }
  }
}