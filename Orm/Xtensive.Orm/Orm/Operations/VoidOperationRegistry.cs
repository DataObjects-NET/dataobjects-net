// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// An <see cref="IOperationRegistry"/> implementor that does nothing
  /// so wastes no time, basically a null object.
  /// </summary>
  internal sealed class VoidOperationRegistry(Session session) : IOperationRegistry
  {
    private sealed class VoidRegistrationScope : ICompletableScope
    {
      private readonly VoidOperationRegistry owner;
      public bool IsCompleted { get; private set; }
      public void Complete() { }
      public void Dispose()
      {
        if (owner.scopes.Peek() != this)
          throw new InvalidOperationException("Invalid scope disposal order.");
        _ = owner.scopes.Pop();
      }

      public VoidRegistrationScope(VoidOperationRegistry owner)
      {

      }
    }

    private readonly Stack<ICompletableScope> scopes = new();

    /// <inheritdoc />
    public Session Session { get; private set; } = session;

    /// <inheritdoc />
    public bool IsRegistrationEnabled => false;

    /// <inheritdoc />
    public bool CanRegisterOperation => false;

    /// <inheritdoc />
    public bool IsRegisteringOperation => false;

    /// <inheritdoc />
    public bool IsSystemOperationRegistrationEnabled => false;

    /// <inheritdoc />
    public ICompletableScope BeginRegistration(OperationType operationType)
    {
      var scope = new VoidRegistrationScope(this);
      scopes.Push(scope);
      return scope;
    }

    /// <inheritdoc />
    public IDisposable DisableUndoOperationRegistration() => throw new NotSupportedException();

    /// <inheritdoc />
    public SystemOperationRegistrationScope DisableSystemOperationRegistration() => default;
    /// <inheritdoc />
    public SystemOperationRegistrationScope EnableSystemOperationRegistration() => default;

    public void NotifyOperationStarting() => throw new NotSupportedException();
    /// <inheritdoc />
    public void NotifyOperationStarting(bool throwIfNotRegistered) => throw new NotSupportedException();
    /// <inheritdoc />
    public void RegisterEntityIdentifier(Key key, string identifier) => throw new NotSupportedException();
    /// <inheritdoc />
    public void RegisterOperation(IOperation operation) => throw new NotSupportedException();
    /// <inheritdoc />
    public void RegisterOperation(IOperation operation, bool isStarted) => throw new NotSupportedException();
    /// <inheritdoc />
    public void RegisterUndoOperation(IOperation operation) => throw new NotSupportedException();

    /// <inheritdoc />
    public event EventHandler<OperationEventArgs> OutermostOperationStarting;
    /// <inheritdoc />
    public event EventHandler<OperationCompletedEventArgs> OutermostOperationCompleted;
    /// <inheritdoc />
    public event EventHandler<OperationEventArgs> NestedOperationStarting;
    /// <inheritdoc />
    public event EventHandler<OperationCompletedEventArgs> NestedOperationCompleted;
    /// <inheritdoc />
    public event EventHandler<OperationEventArgs> UndoOperation;
  }
}