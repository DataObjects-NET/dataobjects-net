// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.14

using System;
using Xtensive.Core;

namespace Xtensive.Internals.DocTemplates
{
  /// <summary>
  /// Disposable class documentation template.
  /// </summary>
  public class DisposableDocTemplate: IDisposable
  {
    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="DisposableDocTemplate"/> is reclaimed by garbage collection.
    /// </summary>
    /// <remarks>
    /// <para id="About">
    /// This method always forwards it job to <see cref="Dispose(bool)"/> method;
    /// its "disposing" parameter gets <see langword="false"/> value.
    /// </para>
    /// </remarks>
    public void Dtor()
    {
    }

    /// <see cref="Dtor" copy="true"/>
    ~DisposableDocTemplate()
    {
    }

    /// <summary>
    /// Performs the tasks associated with freeing, releasing, or resetting unmanaged resources
    /// or associated <see cref="IDisposable"/> objects.
    /// </summary>
    /// <remarks>
    /// <para id="About">
    /// This method always forwards it job to <see cref="Dispose(bool)"/> method;
    /// its "disposing" parameter gets <see langword="true"/> value.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
    }

    /// <summary>
    /// Performs the tasks associated with freeing, releasing, or resetting unmanaged resources
    /// or associated <see cref="IDisposable"/> objects.
    /// </summary>
    /// <param name="disposing">Indicates whether this method was invoked by <see cref="Dispose()"/>,
    /// or by the finalizer.</param>
    public void Dispose(bool disposing)
    {
    }
  }
}