// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;


namespace Xtensive.Core
{
  /// <summary>
  /// A disposable type allowing to execute custom
  /// <see cref="Action"/> on its disposal.
  /// </summary>
  public class Disposable: 
    IDisposable
  {
    private readonly Action<bool> onDispose;

    /// <summary>
    /// Joins the <see cref="Disposable"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="first">The first disposable to join.</param>
    /// <param name="second">The second disposable to join.</param>
    /// <returns>New <see cref="JoiningDisposable"/> that will
    /// dispose both of them on its disposal</returns>
    public static JoiningDisposable operator &(Disposable first, IDisposable second)
    {
      return new JoiningDisposable(first, second);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="onDispose">Action to execute on disposal.</param>
    public Disposable(Action<bool> onDispose)
    {
      this.onDispose = onDispose;
    }


    // Destructor

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing) 
    {
      onDispose(disposing);
      if (!disposing)
        GC.SuppressFinalize(this); 
    }

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
    }
  }
}