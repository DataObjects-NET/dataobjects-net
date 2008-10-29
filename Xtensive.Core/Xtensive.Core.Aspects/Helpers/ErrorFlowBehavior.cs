// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.10.29

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Specifies how the control flow will behave after throwed exception in the aspected method.
  /// </summary>
  public enum ErrorFlowBehavior
  {
    /// <summary>
    /// Default behavior is <see cref="Rethrow"/>.
    /// </summary>
    Default = Rethrow,
    /// <summary>
    /// Rethrow the original exception.
    /// </summary>
    Rethrow = 0,
    /// <summary>
    /// Reprocess aspected method.
    /// </summary>
    Reprocess,
    /// <summary>
    /// Skip the exception.
    /// </summary>
    Skip
  }
}