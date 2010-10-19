// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Disposing
{
  /// <summary>
  /// A disposable type allowing to execute custom
  /// <see cref="Action"/> on its disposal.
  /// Allows to store and pass one parameter to this action.
  /// </summary>
  /// <typeparam name="T">Parameter type.</typeparam>
  public class Disposable<T>: IDisposable
  {
    private readonly Action<bool, T> onDispose;
    private readonly T parameter;

    /// <summary>
    /// Joins the <see cref="Disposable{T}"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="first">The first disposable to join.</param>
    /// <param name="second">The second disposable to join.</param>
    /// <returns>New <see cref="JoiningDisposable"/> that will
    /// dispose both of them on its disposal</returns>
    public static JoiningDisposable operator &(Disposable<T> first, IDisposable second)
    {
      return new JoiningDisposable(first, second);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="parameter">Parameter to pass to the <paramref name="onDispose"/> action.</param>
    /// <param name="onDispose">Action to execute on disposal.</param>
    public Disposable(T parameter, Action<bool, T> onDispose)
    {
      this.onDispose = onDispose;
      this.parameter = parameter;
    }


    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing) 
    {
      onDispose(disposing, parameter);
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