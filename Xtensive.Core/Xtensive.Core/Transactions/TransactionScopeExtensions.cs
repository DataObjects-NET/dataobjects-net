// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

namespace Xtensive.Transactions
{
  /// <summary>
  /// <see cref="TransactionScope"/> related extension methods.
  /// </summary>
  public static class TransactionScopeExtensions
  {
    /// <summary>
    /// Marks the scope as successfully completed 
    /// (i.e. all operations within the scope are completed successfully).
    /// Does nothing if scope is <see langword="null" />.
    /// </summary>
    /// <param name="scope">The scope to mark as completed.</param>
    public static void Complete(this TransactionScope scope)
    {
      if (scope!=null)
        scope.IsCompleted = true;
    }
  }
}