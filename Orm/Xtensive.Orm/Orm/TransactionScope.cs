// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: 
// Created:    2008.08.30

using System;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Transaction scope suitable for storage.
  /// </summary>
  public sealed class TransactionScope : ICompletableScope, IAsyncDisposable
  {
    private static readonly TransactionScope VoidScope = new TransactionScope();

    private IDisposable disposable;
    private bool isDisposed;

    /// <summary>
    /// Gets a value indicating whether this instance is <see cref="Complete"/>d.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// <see cref="TransactionScope"/> instance that is used for all <see cref="IsVoid">nested</see> scopes.
    /// </summary>
    public static TransactionScope VoidScopeInstance => VoidScope;

    /// <summary>
    /// Gets the transaction this scope controls.
    /// </summary>
    public Transaction Transaction { get; }

    /// <summary>
    /// Gets a value indicating whether this scope is void,
    /// i.e. is included into another <see cref="TransactionScope"/> 
    /// and therefore does nothing on opening and disposing.
    /// </summary>
    public bool IsVoid => this == VoidScopeInstance;

    /// <summary>
    /// Completes this scope. 
    /// This method can be called multiple times; if so, only the first call makes sense.
    /// </summary>
    public void Complete() => IsCompleted = true;

    /// <inheritdoc/>
    public void Dispose() => DisposeImpl(false).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => DisposeImpl(true);

    private async ValueTask DisposeImpl(bool isAsync)
    {
      if (isDisposed) {
        return;
      }

      isDisposed = true;
      try {
        if (Transaction == null || !Transaction.State.IsActive()) {
          return;
        }

        if (IsCompleted) {
          await Transaction.Commit(isAsync).ConfigureAwaitFalse();
        }
        else {
          await Transaction.Rollback(isAsync).ConfigureAwaitFalse();
        }
      }
      finally {
        try {
          disposable.DisposeSafely(true);
        }
        finally {
          disposable = null;
        }
      }
    }


    // Constructors

    private TransactionScope()
    {
    }

    internal TransactionScope(Transaction transaction, IDisposable disposable)
    {
      Transaction = transaction;
      this.disposable = disposable;
    }
  }
}