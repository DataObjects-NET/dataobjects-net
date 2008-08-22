// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage
{
  /// <summary>
  /// Transaction scope.
  /// </summary>
  public class TransactionScope : TransactionScopeBase<TransactionScope, Transaction>
  {

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public TransactionScope(Transaction transaction)
      : base(transaction)
    {
    }

//    /// <summary>
//    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
//    /// </summary>
//    public TransactionScope()
//      : this(new Transaction())
//    {
//    }
  }
}