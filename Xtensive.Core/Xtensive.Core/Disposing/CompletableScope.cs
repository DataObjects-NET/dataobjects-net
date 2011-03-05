// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Disposing
{
  /// <summary>
  /// An <see cref="ICompletableScope"/> implementation.
  /// </summary>
  public class CompletableScope : ICompletableScope
  {
    /// <inheritdoc/>
    public bool IsCompleted { get; protected set; }

    /// <inheritdoc/>
    public virtual void Complete()
    {
      IsCompleted = true;
    }

    
    // Disposal

    /// <see cref="DisposableDocTemplate.Dispose()" copy="true" />
    public virtual void Dispose()
    {
    }
  }
}