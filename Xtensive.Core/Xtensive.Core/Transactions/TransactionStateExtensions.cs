// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.02

namespace Xtensive.Transactions
{
  /// <summary>
  /// <see cref="TransactionState"/> related extension methods.
  /// </summary>
  public static class TransactionStateExtensions
  {
    /// <summary>
    /// Determines whether the specified transaction state describes active transaction.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>
    /// <see langword="True"/> if the specified state describes active transaction;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsActive(this TransactionState state)
    {
      return (state & TransactionState.Completed)==0;
    }
  }
}