// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.10.29

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Specifies the control flow mode on catching an exception in the aspected method.
  /// </summary>
  public enum ErrorFlowBehavior
  {
    /// <summary>
    /// Rethrow the original exception.
    /// This is default behavior.
    /// Value is <see langword="0" />.
    /// </summary>
    Rethrow = 0,
    /// <summary>
    /// Reprocess aspected method.
    /// Value is <see langword="1" />.
    /// </summary>
    Reprocess = 1,
    /// <summary>
    /// Skip the exception.
    /// Value is <see langword="2" />.
    /// </summary>
    Skip = 2,
  }
}