// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.14

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.Resources;

namespace System
{
  /// <summary>
  /// <see cref="IDisposable"/> related extension methods.
  /// </summary>
  public static class DisposableExtensions
  {
    /// <summary>
    /// Joins the specified disposable objects by returning
    /// a single <see cref="JoiningDisposable"/> that will
    /// dispose both of them on its disposal.
    /// </summary>
    /// <param name="disposable">The first disposable.</param>
    /// <param name="joinWith">The second disposable.</param>
    /// <returns>New <see cref="JoiningDisposable"/> that will
    /// dispose both of them on its disposal</returns>
    public static JoiningDisposable Join(this IDisposable disposable, IDisposable joinWith)
    {
      return new JoiningDisposable(disposable, joinWith);
    }

    /// <summary>
    /// Safely disposes an <see cref="IDisposable"/> object.
    /// </summary>
    /// <param name="disposable">Object to dispose (can be <see langword="null"/>).</param>
    public static void DisposeSafely(this IDisposable disposable)
    {
      if (disposable!=null)
        disposable.Dispose();
    }

    /// <summary>
    /// Safely disposes an <see cref="IDisposable"/> object.
    /// </summary>
    /// <param name="disposable">Object to dispose (can be <see langword="null"/>).</param>
    /// <param name="log">Log to write errors to.</param>
    /// <param name="disposerName">Name (or type name) of the object that invoked this method.</param>
    public static Exception DisposeSafely(this IDisposable disposable, ILog log, string disposerName)
    {
      try {
        disposable.DisposeSafely();
      }
      catch (Exception e) {
        log.Error(e, Strings.LogSafeDisposeFailing, disposerName);
        return e;
      }
      return null;
    }

    /// <summary>
    /// Safely disposes a set of <see cref="IDisposable"/> items.
    /// </summary>
    /// <param name="items">Items to dispose (can be <see langword="null"/>).</param>
    /// <param name="log">Log to write errors to.</param>
    /// <param name="disposerName">Name (or type name) of the object that invoked this method.</param>
    public static void DisposeSafely(this IEnumerable<IDisposable> items, ILog log, string disposerName)
    {
      if (items==null)
        return;
      Exception firstError = null;
      foreach (IDisposable item in items) {
        try {
          item.DisposeSafely();
        }
        catch (Exception e) {
          log.Error(e, Strings.LogSafeDisposeFailing, disposerName);
          if (firstError==null)
            firstError = e;
        }
      }
      if (firstError!=null)
        throw firstError;
    }

    /// <summary>
    /// Checks ability of the item to be disposed.
    /// </summary>
    /// <param name="container">The disposable container that contains the item.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="container"/> has <see cref="IDisposableContainer.DisposingState"/> is <see cref="DisposingState.Disposing"/>;
    /// <see langword="false"/> when <paramref name="container"/> has <see cref="IDisposableContainer.DisposingState"/> is <see cref="DisposingState.Disposed"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="container"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Unable to dispose the item when <paramref name="container"/>'s <see cref="IDisposableContainer.DisposingState"/> is <see cref="DisposingState.None"/>.</exception>
    public static bool CheckItemDisposing(this IDisposableContainer container)
    {
      ArgumentValidator.EnsureArgumentNotNull(container, "container");
      switch (container.DisposingState) {
      case DisposingState.None:
        throw new InvalidOperationException(Strings.UnableToDisposeItemWhenContainerIsNotDisposed);
      case DisposingState.Disposing:
        return true;
      case DisposingState.Disposed:
        return false;
      default:
        throw new ArgumentOutOfRangeException();
      }
    }
  }
}