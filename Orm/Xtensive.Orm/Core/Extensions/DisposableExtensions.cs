// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.05.14

using System;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Core
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
    /// Safely disposes an <see cref="IAsyncDisposable"/> object.
    /// </summary>
    /// <param name="disposable">Object to dispose (can be <see langword="null"/>).</param>
    public static ValueTask DisposeSafelyAsync(this IAsyncDisposable disposable) =>
      disposable?.DisposeAsync() ?? default;

    /// <summary>
    /// Safely disposes an <see cref="IDisposable"/> object.
    /// </summary>
    /// <param name="disposable">Object to dispose (can be <see langword="null"/>).</param>
    /// <param name="silently">If set to <see langword="true"/>, it won't throw an exception in any case.</param>
    public static void DisposeSafely(this IDisposable disposable, bool silently)
    {
      try {
        if (disposable!=null)
          disposable.Dispose();
      }
      catch {
        if (!silently)
          throw;
      }
    }

    /// <summary>
    /// Safely disposes an <see cref="IAsyncDisposable"/> object.
    /// </summary>
    /// <param name="disposable">Object to dispose (can be <see langword="null"/>).</param>
    /// <param name="silently">If set to <see langword="true"/>, it won't throw an exception in any case.</param>
    public static async ValueTask DisposeSafelyAsync(this IAsyncDisposable disposable, bool silently)
    {
      try {
        if (disposable!=null) {
          await disposable.DisposeAsync().ConfigureAwaitFalse();
        }
      }
      catch {
        if (!silently) {
          throw;
        }
      }
    }
  }
}