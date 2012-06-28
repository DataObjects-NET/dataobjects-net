// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.16

using System;
using System.Diagnostics;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Provides access to some <see cref="Leased"/> object of type <typeparamref name="T"/>
  /// Much like <see cref="Disposable{T}"/>, but provides 
  /// access to its parameter via <see cref="Leased"/> property.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="Leased"/> object.</typeparam>
  public class LeasedAccessor<T>: IDisposable
  {
    private T leased;
    private readonly Action<bool, T> onDispose;

    /// <summary>
    /// Provides access to actually leased object.
    /// </summary>
    public T Leased
    {
      [DebuggerStepThrough]
      get { return leased; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="leased">Initial <see cref="Leased"/> property value.</param>
    /// <param name="onDispose">Action to execute on disposal.</param>
    public LeasedAccessor(T leased, Action<bool, T> onDispose)
    {
      this.leased = leased;
      this.onDispose = onDispose;
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing) 
    {
      onDispose(disposing, leased);
      leased = default(T);
      if (!disposing)
        GC.SuppressFinalize(this); 
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose() 
    {
      Dispose(true);
    }
  }
}