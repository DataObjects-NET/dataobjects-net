// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  public partial class Session 
  {
    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can not get validation context: There is no active transaction.</exception>
    public ValidationContext ValidationContext {
      get
      {
        var transaction = GetTransactionFromSessionOrDisconnectedState();
        if (transaction==null)
          throw new InvalidOperationException(Strings.ExCanNotGetValidationContextThereIsNoActiveTransaction);
        return transaction.ValidationContext;
      }
    }
  }
}