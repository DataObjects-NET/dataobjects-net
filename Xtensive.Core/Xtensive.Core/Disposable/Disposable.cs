// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Disposable
{
  /// <summary>
  /// A disposable type allowing to execute custom
  /// <see cref="Action"/> on its disposal.
  /// </summary>
  public class Disposable: IDisposable
  {
    private readonly Action<bool> onDispose;


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="onDispose">Action to execute on disposal.</param>
    public Disposable(Action<bool> onDispose)
    {
      this.onDispose = onDispose;
    }


    // Destructor

    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true" />
    protected virtual void Dispose(bool disposing) 
    {
      onDispose(disposing);
      if (!disposing)
        GC.SuppressFinalize(this); 
    }

    /// <see cref="DisposableDocTemplate.Dispose()" copy="true" />
    public void Dispose() 
    {
      Dispose(true);
    }
  }
}