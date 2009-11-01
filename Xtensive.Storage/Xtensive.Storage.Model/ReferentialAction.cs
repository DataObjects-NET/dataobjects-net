// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.24

using System;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Defines all possible actions applicable in the case
  /// of foreign key conflict.
  /// </summary>
  [Serializable]
  public enum ReferentialAction
  {
    /// <summary>
    /// Indicates that ORM is capable to assign <b>NULL</b> to a field
    /// referenced by foreign key constraint in order to preserve referential 
    /// integrity if it violated by some executed instruction.
    /// </summary>
    SetNull = 0,

    /// <summary>
    /// The same as <see cref="Restrict"/>.
    /// </summary>
    Default = Restrict,

    /// <summary>
    /// Indicates that ORM is capable to block execution of any instruction
    /// leading to foreign key conflict.
    /// </summary>
    Restrict = 1,

    /// <summary>
    /// Indicates that ORM is capable to perform delete cascading operation
    /// in order to preserve referential integrity if
    /// it violated by some executed instruction.
    /// </summary>
    Cascade = 2,
  }
}