// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.12.17

namespace Xtensive.Orm.Rse
{
  ///<summary>
  /// Join algorithm.
  ///</summary>
  public enum JoinAlgorithm
  {
    ///<summary>
    /// Default join algorithm.
    ///</summary>
    Default = 0,

    ///<summary>
    /// Loop join algorithm.
    ///</summary>
    Loop = 1,

    ///<summary>
    /// Nested Loop join algorithm.
    ///</summary>
    NestedLoop = 2,

    ///<summary>
    /// Merge join algorithm.
    ///</summary>
    Merge = 4,

    ///<summary>
    /// Hash join algorithm.
    ///</summary>
    Hash = 8,
  }
}