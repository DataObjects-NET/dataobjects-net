// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.17

namespace Xtensive.Distributed.Test
{
  /// <summary>
  /// Possible results of task process execution.
  /// </summary>
  public enum ExecutionResultType
  {
    /// <summary>
    /// Default value.
    /// The same as <see cref="Success"/>.
    /// </summary>
    Default = Success,
    /// <summary>
    /// Task process is started sucessfully.
    /// </summary>
    Success = 0,
    /// <summary>
    /// An error has occured on starting the task process.
    /// </summary>
    Error = 1,
  }
}