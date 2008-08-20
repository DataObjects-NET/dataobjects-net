// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// A disposable type allowing to execute custom
  /// <see cref="Action"/> on its disposal.
  /// Allows to store and pass two parameters to this action.
  /// </summary>
  /// <typeparam name="T1">First parameter type.</typeparam>
  /// <typeparam name="T2">Second parameter type.</typeparam>
  public class Disposable<T1, T2>: IDisposable
  {
    private readonly Action<bool, T1, T2> onDispose;
    private readonly T1 parameter1;
    private readonly T2 parameter2;


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="parameter1">First parameter to pass to the <paramref name="onDispose"/> action.</param>
    /// <param name="parameter2">Second parameter to pass to the <paramref name="onDispose"/> action.</param>
    /// <param name="onDispose">Action to execute on disposal.</param>
    public Disposable(T1 parameter1, T2 parameter2, Action<bool, T1, T2> onDispose)
    {
      this.onDispose  = onDispose;
      this.parameter1 = parameter1;
      this.parameter2 = parameter2;
    }


    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing) 
    {
      onDispose(disposing, parameter1, parameter2);
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
