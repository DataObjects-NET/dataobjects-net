// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.26

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.SavedDataContexts;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage
{
  public class TransactionSavedDataScope : TransactionScopeBase<TransactionScope, Transaction>
  {

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    public TransactionSavedDataScope(Transaction transaction)
      : base(transaction)
    {
    }

  }
}