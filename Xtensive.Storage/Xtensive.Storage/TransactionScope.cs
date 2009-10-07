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
    private static readonly TransactionScope hollowRegionInstance = new TransactionScope();

    /// <summary>
    /// <see cref="TransactionScope"/> instance that is used for all <see cref="IsHollow">nested</see> scopes.
    /// </summary>
    public static TransactionScope HollowScopeInstance
    {
      get { return hollowRegionInstance; }
    }

    /// <summary>
    /// Gets a value indicating whether this scope is hollow,
    /// i.e. is included into another <see cref="TransactionScope"/> 
    /// and therefore does nothing on opening and disposing.
    /// </summary>
    public bool IsHollow
    {
      get { return this==HollowScopeInstance; }
    }

    /// <summary>
    /// Gets the transaction this scope controls.
    /// </summary>
    public new Transaction Transaction
    {
      get { return (Transaction) base.Transaction; }
    }

    // Constructors

    private TransactionScope()
      : base(null)
    {
    }

    internal TransactionScope(Transaction transaction)
      : base(transaction)
    {
    }
  }
}