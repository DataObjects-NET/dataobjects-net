// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// An <see cref="IOperationRegistry"/> implementor that does nothing
  /// so wastes no time, basically a null object.
  /// </summary>
  internal sealed class VoidOperationRegistry(Session session) : IOperationRegistry
  {
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
    public ICompletableScope BeginRegistration(OperationType operationType) => null;

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

#pragma warning disable CS0067
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
#pragma warning restore CS0067
  }
}