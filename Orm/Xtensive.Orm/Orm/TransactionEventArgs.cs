// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Provides data for <see cref="Session"/> transaction-related events.
  /// </summary>
  public readonly struct TransactionEventArgs
  {
    /// <summary>
    /// Gets the transaction.
    /// </summary>
    public Transaction Transaction { get; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public TransactionEventArgs(Transaction transaction)
    {
      Transaction = transaction;
    }
  }
}
