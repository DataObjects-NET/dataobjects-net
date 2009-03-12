// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Parameters
{
  /// <summary>
  /// Binding item.
  /// </summary>
  [Serializable]
  public sealed class Binding : IDisposable
  {
    private readonly Action disposeAction;

    /// <inheritdoc/>
    public void Dispose()
    {
      disposeAction();
    }

    /// <summary>
    /// Combines two bindings.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    public static Binding operator &(Binding x, Binding y)
    {
      ArgumentValidator.EnsureArgumentNotNull(x, "x");
      ArgumentValidator.EnsureArgumentNotNull(y, "y");
      return new Binding(y.disposeAction + x.disposeAction);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    internal Binding(Action disposeAction)
    {
      this.disposeAction = disposeAction;
    }
  }
}