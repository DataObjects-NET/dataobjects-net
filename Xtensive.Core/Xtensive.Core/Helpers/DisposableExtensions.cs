// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.14

using System;
using System.Collections.Generic;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// <see cref="IDisposable"/> related extension methods.
  /// </summary>
  public static class DisposableExtensions
  {
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
  }
}