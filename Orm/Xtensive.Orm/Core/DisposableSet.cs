// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2007.12.29

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xtensive.Core;


namespace Xtensive.Core
{
  /// <summary>
  /// Ensures all the <see cref="IDisposable"/> objects added to it are disposed
  /// on disposal of <see cref="DisposableSet"/> instance.
  /// </summary>
  /// <remarks>
  /// <note>
  /// <see cref="IDisposable.Dispose"/> methods are invoked in backward order.
  /// </note>
  /// </remarks>
  public sealed class DisposableSet : IDisposable, IAsyncDisposable
  {
    private HashSet<IDisposable> set;
    private List<IDisposable> list;

    /// <summary>
    /// Adds an <see cref="IDisposable"/> object to the set.
    /// </summary>
    /// <param name="disposable">The object to add.</param>
    /// <returns><see langword="True"/>, if object is successfully added;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Add(IDisposable disposable)
    {
      if (disposable==null)
        return false;
      EnsureInitialized();
      if (set.Add(disposable)) {
        list.Add(disposable);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Clears this instance by discarding all registered objects.
    /// <see cref="IDisposable.Dispose"/> methods are not called.
    /// </summary>
    public void Clear()
    {
      set = null;
      list = null;
    }

    /// <summary>
    /// Joins the <see cref="DisposableSet"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="first">The first disposable to join.</param>
    /// <param name="second">The second disposable to join.</param>
    /// <returns>New <see cref="JoiningDisposable"/> that will
    /// dispose both of them on its disposal</returns>
    public static JoiningDisposable operator &(DisposableSet first, IDisposable second)
    {
      return new JoiningDisposable(first, second);
    }

    private void EnsureInitialized()
    {
      if (set==null) {
        set = new HashSet<IDisposable>();
        list = new List<IDisposable>();
      }
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="initialContent">The initial content.</param>
    public DisposableSet(IEnumerable initialContent)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(initialContent, "initialContent");
      foreach (object o in initialContent)
        Add(o as IDisposable);
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public DisposableSet()
    {
    }

    /// <summary>
    /// Releases resources associated with this instance.
    /// </summary>
    void IDisposable.Dispose()
    {
      try {
        if (list==null) {
          return;
        }

        using (var aggregator = new ExceptionAggregator()) {
          for (var i = list.Count - 1; i >= 0; i--) {
            aggregator.Execute(d => d.Dispose(), list[i]);
          }

          aggregator.Complete();
        }
      }
      finally {
        set = null;
        list = null;
      }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
      try {
        if (list==null) {
          return;
        }

        using (var aggregator = new ExceptionAggregator()) {
          for (var i = list.Count - 1; i >= 0; i--) {
            var disposable = list[i];
            if (disposable is IAsyncDisposable asyncDisposable) {
              await aggregator.ExecuteAsync(d => d.DisposeAsync(), asyncDisposable).ConfigureAwaitFalse();
            }
            else {
              aggregator.Execute(d => d.Dispose(), disposable);
            }
          }

          aggregator.Complete();
        }
      }
      finally {
        set = null;
        list = null;
      }
    }
  }
}
