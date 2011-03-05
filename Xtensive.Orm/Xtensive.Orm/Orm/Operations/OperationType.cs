// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Defines possible operation types.
  /// </summary>
  [Serializable]
  public enum OperationType
  {
    /// <summary>
    /// Custom (user-defined) operation.
    /// </summary>
    Custom,
    /// <summary>
    /// System (pre-defined) operation.
    /// </summary>
    System,
  }
}