// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contract for <see cref="IDisposable"/> implementation providing <see cref="Complete"/> method.
  /// </summary>
  public interface ICompletableScope : IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether this instance is <see cref="Complete"/>d.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Completes this scope. 
    /// This method can be called multiple times; if so, only the first call makes sense.
    /// </summary>
    void Complete();
  }
}