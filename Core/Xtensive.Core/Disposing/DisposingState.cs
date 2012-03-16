// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.26

namespace Xtensive.Disposing
{
  /// <summary>
  /// State of disposable object.
  /// </summary>
  public enum DisposingState
  {
    /// <summary>
    /// The object is not disposed or being disposed; operational state.
    /// </summary>
    None = 0,
    /// <summary>
    /// The object is in disposing state.
    /// </summary>
    Disposing,
    /// <summary>
    /// The object has been disposed.
    /// </summary>
    Disposed
  }
}