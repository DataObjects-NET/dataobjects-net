// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.22

using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage
{
  /// <summary>
  /// <see cref="TransactionScope"/> related extensions.
  /// </summary>
  public static class TransactionScopeExtensions
  {
    /// <summary>
    /// Indicates that all operations within the scope are completed successfully.
    /// Does nothing if scope is null.
    /// </summary>
    /// <param name="scope">The scope to complete .</param>
    public static void Complete(this TransactionScope scope)
    {
      TransactionScope.Complete(scope);
    }
  }
}