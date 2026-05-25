// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Orm.Operations;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contract for an operation registry as a session service.
  /// Its implementors can be registered and be retrieved in <see cref="Session.Services"/>
  /// </summary>
  public interface IOperationRegistry : ISessionService
  {
    /// <summary>
    /// Gets the session this instance is bound to.
    /// </summary>
    Session Session { get; }

    /// <summary>
    /// Indicates whether operation logging is enabled.
    /// <see cref="Orm.Session.OpenSystemLogicOnlyRegion"/> implicitly turns this option off;
    /// <see cref="DisableUndoOperationRegistration"/> does this explicitly.
    /// </summary>
    bool IsRegistrationEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether system operation registration is enabled.
    /// </summary>
    bool IsSystemOperationRegistrationEnabled { get; }

    /// <summary>
    /// Gets a value indicating whether this instance can register operation
    /// using <see cref="RegisterOperation(IOperation)"/> method.
    /// </summary>
    bool CanRegisterOperation { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is registering operation now,
    /// i.e. <see cref="BeginRegistration"/> method was invoked, but the
    /// scope isn't closed yet.
    /// </summary>
    bool IsRegisteringOperation { get; }

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    void RegisterOperation(IOperation operation);

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    /// <param name="isStarted">If set to <see langword="true"/>,
    /// <see cref="NotifyOperationStarting()"/> method
    /// will be called on completion of operation registration.</param>
    void RegisterOperation(IOperation operation, bool isStarted);

    /// <summary>
    /// Indicates that operation, that is currently registering, is started.
    /// Leads to <see cref="OutermostOperationStarting"/> or <see cref="NestedOperationStarting"/> notification.
    /// </summary>
    void NotifyOperationStarting();

    /// <summary>
    /// Indicates that operation, that is currently registering, is started.
    /// Leads to <see cref="OutermostOperationStarting"/> or <see cref="NestedOperationStarting"/> notification.
    /// </summary>
    /// <param name="throwIfNotRegistered">Indicates whether <see cref="InvalidOperationException"/> 
    /// must be thrown if operation isn't registered yet.</param>
    void NotifyOperationStarting(bool throwIfNotRegistered);

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    void RegisterUndoOperation(IOperation operation);

    /// <summary>
    /// Registers the entity identifier.
    /// </summary>
    /// <param name="key">The key of the entity to log the identifier for.</param>
    /// <param name="identifier">The entity identifier.
    /// <see langword="null" /> indicates identifier must be assigned automatically 
    /// as sequential number inside the current operation context.</param>
    void RegisterEntityIdentifier(Key key, string identifier);

    /// <summary>
    /// Temporarily disables undo operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object enabling the logging back on its disposal.</returns>
    IDisposable DisableUndoOperationRegistration();

    /// <summary>
    /// Temporarily disables system operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object enabling the logging back on its disposal.</returns>
    SystemOperationRegistrationScope DisableSystemOperationRegistration();

    /// <summary>
    /// Temporarily enables system operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object disabling the logging back on its disposal.</returns>
    SystemOperationRegistrationScope EnableSystemOperationRegistration();

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operationType">Type of the operation.</param>
    /// <returns></returns>
    ICompletableScope BeginRegistration(OperationType operationType);

    long GetNextIdentifier() => -1;

    /// <summary>
    /// Occurs when outermost <see cref="IOperation"/> is starting.
    /// </summary>
    event EventHandler<OperationEventArgs> OutermostOperationStarting;

    /// <summary>
    /// Occurs when outermost <see cref="IOperation"/> is being registered.
    /// </summary>
    event EventHandler<OperationCompletedEventArgs> OutermostOperationCompleted;

    /// <summary>
    /// Occurs when nested <see cref="IOperation"/> is starting.
    /// </summary>
    event EventHandler<OperationEventArgs> NestedOperationStarting;

    /// <summary>
    /// Occurs when nested <see cref="IOperation"/> is being registered.
    /// </summary>
    event EventHandler<OperationCompletedEventArgs> NestedOperationCompleted;

    /// <summary>
    /// Occurs when undo <see cref="IOperation"/> is being registered.
    /// </summary>
    event EventHandler<OperationEventArgs> UndoOperation;
  }
}