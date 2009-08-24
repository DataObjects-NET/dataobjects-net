// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.30

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of <see cref="Integrity.Transactions.TransactionScope"/>
  /// suitable for storage.
  /// </summary>
  public class TransactionScope : Integrity.Transactions.TransactionScope
  {
    /// <summary>
    /// Gets the transaction this scope controls.
    /// </summary>
    public new Transaction Transaction
    {
      get { return (Transaction) base.Transaction; }
    }


    // Constructors

    internal TransactionScope(Transaction transaction)
      : base(transaction)
    {
    }
  }
}