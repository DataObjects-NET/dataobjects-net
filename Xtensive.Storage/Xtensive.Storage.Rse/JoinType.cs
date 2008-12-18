// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.17

namespace Xtensive.Storage.Rse
{
  ///<summary>
  /// Join provider type.
  ///</summary>
  public enum JoinType
  {
    ///<summary>
    /// Default join operation.
    ///</summary>
    Default = 0,

    ///<summary>
    /// Loop join operation.
    ///</summary>
    Loop = 1,

    ///<summary>
    /// Nested Loop join operation.
    ///</summary>
    NestedLoop = 2,

    ///<summary>
    /// Merge join operation.
    ///</summary>
    Merge = 4,

    ///<summary>
    /// Hash join operation.
    ///</summary>
    Hash = 8,
  }
}