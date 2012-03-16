// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.22

using System.Transactions;
using SD=System.Data;

namespace Xtensive.Orm.Providers
{
  ///<summary>
  /// Converts <see cref="IsolationLevel"/> to <see cref="SD.IsolationLevel"/>.
  ///</summary>
  public static class IsolationLevelConverter
  {
    ///<summary>
    /// Converts <see cref="IsolationLevel"/> to <see cref="SD.IsolationLevel"/>.
    ///</summary>
    ///<param name="level">The specified <see cref="IsolationLevel"/>.</param>
    ///<returns></returns>
    public static SD.IsolationLevel Convert(IsolationLevel level)
    {
      switch (level) {
        case IsolationLevel.Chaos:
          return SD.IsolationLevel.Chaos;
        case IsolationLevel.ReadCommitted:
          return SD.IsolationLevel.ReadCommitted;
        case IsolationLevel.ReadUncommitted:
          return SD.IsolationLevel.ReadUncommitted;
        case IsolationLevel.RepeatableRead:
          return SD.IsolationLevel.RepeatableRead;
        case IsolationLevel.Serializable:
          return SD.IsolationLevel.Serializable;
        case IsolationLevel.Snapshot:
          return SD.IsolationLevel.Snapshot;
        default:
          return SD.IsolationLevel.Unspecified;
      }
    }
  }
}