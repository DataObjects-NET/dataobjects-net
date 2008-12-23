// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.22

using System.Transactions;

namespace Xtensive.Storage.Providers
{
  ///<summary>
  /// Converts <see cref="IsolationLevel"/> to <see cref="System.Data.IsolationLevel"/>.
  ///</summary>
  public static class IsolationLevelConverter
  {
    ///<summary>
    /// Converts <see cref="IsolationLevel"/> to <see cref="System.Transactions.IsolationLevel"/>.
    ///</summary>
    ///<param name="level">The specified <see cref="IsolationLevel"/>.</param>
    ///<returns></returns>
    public static System.Data.IsolationLevel Convert(IsolationLevel level)
    {
      switch (level) {
        case IsolationLevel.Chaos:
          return System.Data.IsolationLevel.Chaos;
        case IsolationLevel.ReadCommitted:
          return System.Data.IsolationLevel.ReadCommitted;
        case IsolationLevel.ReadUncommitted:
          return System.Data.IsolationLevel.ReadUncommitted;
        case IsolationLevel.RepeatableRead:
          return System.Data.IsolationLevel.RepeatableRead;
        case IsolationLevel.Serializable:
          return System.Data.IsolationLevel.Serializable;
        case IsolationLevel.Snapshot:
          return System.Data.IsolationLevel.Snapshot;
        default:
          return System.Data.IsolationLevel.Unspecified;
      }
    }
  }
}