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
    private static readonly TransactionScope VoidRegionInstance = new TransactionScope();

    /// <summary>
    /// <see cref="TransactionScope"/> instance that is used for all <see cref="IsVoid">nested</see> scopes.
    /// </summary>
    public static TransactionScope VoidScopeInstance { get { return VoidRegionInstance; } }

    /// <summary>
    /// Gets a value indicating whether this scope is void,
    /// i.e. is included into another <see cref="TransactionScope"/> 
    /// and therefore does nothing on opening and disposing.
    /// </summary>
    public bool IsVoid { get { return this==VoidScopeInstance; } }

    /// <summary>
    /// Gets the transaction this scope controls.
    /// </summary>
    public new Transaction Transaction { get { return (Transaction) base.Transaction; } }

    /// <summary>
    /// Marks the scope as successfully completed 
    /// (i.e. all operations within the scope are completed successfully).
    /// </summary>
    public void Complete()
    {
      IsCompleted = true;
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