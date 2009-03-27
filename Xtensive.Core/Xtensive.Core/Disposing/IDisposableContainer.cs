// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.26

using System;

namespace Xtensive.Core.Disposing
{
  /// <summary>
  /// Defines a <see cref="DisposingState"/> property for <see cref="IDisposable"/> implementors.
  /// </summary>
  public interface IDisposableContainer : IDisposable
  {
    /// <summary>
    /// Gets the state of the disposing.
    /// </summary>
    DisposingState DisposingState { get; }
  }
}