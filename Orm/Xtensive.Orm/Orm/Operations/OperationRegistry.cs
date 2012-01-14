// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Operation registry for <see cref="Session"/> type.
  /// </summary>
  public sealed class OperationRegistry
  {
    private CompletableScope blockingScope;
    private bool isOperationRegistrationEnabled = true;
    private bool isUndoOperationRegistrationEnabled = true;
    private bool isSystemOperationRegistrationEnabled = true;
    private Deque<CompletableScope> scopes = new Deque<CompletableScope>();

    /// <summary>
    /// Gets the session this instance is bound to.
    /// </summary>
    internal Session Session { get; private set; }

    #region IsXxx properties

    /// <summary>
    /// Indicates whether operation logging is enabled.
    /// <see cref="Orm.Session.OpenSystemLogicOnlyRegion"/> implicitly turns this option off;
    /// <see cref="DisableUndoOperationRegistration"/> does this explicitly.
    /// </summary>
    public bool IsRegistrationEnabled {
      get { return isOperationRegistrationEnabled || isUndoOperationRegistrationEnabled; }
    }

    /// <summary>
    /// Gets a value indicating whether system operation registration is enabled.
    /// </summary>
    public bool IsSystemOperationRegistrationEnabled {
      get { return isSystemOperationRegistrationEnabled; }
      internal set { isSystemOperationRegistrationEnabled = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance can register operation
    /// using <see cref="RegisterOperation"/> method.
    /// </summary>
    public bool CanRegisterOperation {
      get {
        var scope = scopes.TailOrDefault;
        return scope!=null && scope is OperationRegistrationScope;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is registering operation now,
    /// i.e. <see cref="BeginRegistration"/> method was invoked, but the
    /// scope isn't closed yet.
    /// </summary>
    public bool IsRegisteringOperation {
      get { return scopes.Count!=0; }
    }

    internal bool IsOutermostOperationRegistrationEnabled {
      get {
        return (
          OutermostOperationCompleted!=null || OutermostOperationStarting!=null || 
          NestedOperationCompleted!=null || NestedOperationStarting!=null
          ) && IsRegistrationEnabled;
      }
    }

    internal bool IsNestedOperationRegistrationEnabled {
      get { 
        return 
          (NestedOperationStarting!=null || NestedOperationCompleted!=null)
          && IsRegistrationEnabled; 
      }
    }

    internal bool IsOperationRegistrationEnabled {
      get {
        return (
          OutermostOperationCompleted!=null || OutermostOperationStarting!=null || 
          NestedOperationCompleted!=null || NestedOperationStarting!=null
          ) && isOperationRegistrationEnabled;
      }
    }

    internal bool IsUndoOperationRegistrationEnabled {
      get { return UndoOperation!=null && IsUndoOperationRegistrationEnabled; }
    }

    #endregion

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    public void RegisterOperation(Operation operation)
    {
      RegisterOperation(operation, false);
    }

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    /// <param name="isStarted">If set to <see langword="true"/>, <see cref="NotifyOperationStarting"/> method
    /// will be called on completion of operation registration.</param>
    public void RegisterOperation(Operation operation, bool isStarted)
    {
      var scope = GetCurrentOperationRegistrationScope();
      if (scope==null)
        return;
      if (scope.Operation!=null)
        throw new InvalidOperationException(Strings.ExOnlyOneOperationCanBeRegisteredInEachScope);
      operation.Type = scope.OperationType;
      scope.Operation = operation;
      if (isStarted) {
        scope.IsOperationStarted = true;
        if (scope.IsOutermost)
          NotifyOutermostOperationStarting(operation);
        else
          NotifyNestedOperationStarting(operation);
      }
    }

    /// <summary>
    /// Indicates that operation, that is currently registering, is started.
    /// Leads to <see cref="OutermostOperationStarting"/> or <see cref="NestedOperationStarting"/> notification.
    /// </summary>
    public void NotifyOperationStarting()
    {
      NotifyOperationStarting(true);
    }

    /// <summary>
    /// Indicates that operation, that is currently registering, is started.
    /// Leads to <see cref="OutermostOperationStarting"/> or <see cref="NestedOperationStarting"/> notification.
    /// </summary>
    /// <param name="throwIfNotRegistered">Indicates whether <see cref="InvalidOperationException"/> 
    /// must be thrown if operation isn't registered yet.</param>
    public void NotifyOperationStarting(bool throwIfNotRegistered)
    {
      if (!CanRegisterOperation)
        return;
      var scope = GetCurrentOperationRegistrationScope();
      if (scope==null)
        return;
      var operation = scope.Operation;
      if (operation==null) {
        if (throwIfNotRegistered)
          throw new InvalidOperationException(Strings.ExOperationIsNotRegisteredYet);
        else
          return;
      }
      if (scope.IsOperationStarted)
        throw new InvalidOperationException(Strings.ExOperationStartedIsAlreadyCalledForThisOperation);
      scope.IsOperationStarted = true;
      if (scope.IsOutermost)
        NotifyOutermostOperationStarting(operation);
      else
        NotifyNestedOperationStarting(operation);
    }

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    public void RegisterUndoOperation(Operation operation)
    {
      var scope = GetCurrentOperationRegistrationScope();
      if (scope==null)
        return;
      if (scope.UndoOperations==null)
        scope.UndoOperations = new List<IOperation>();
      scope.UndoOperations.Add(operation);
      // Notifying...
      NotifyUndoOperation(operation);
    }

    /// <summary>
    /// Registers the entity identifier.
    /// </summary>
    /// <param name="key">The key of the entity to log the identifier for.</param>
    /// <param name="identifier">The entity identifier.
    /// <see langword="null" /> indicates identifier must be assigned automatically 
    /// as sequential number inside the current operation context.</param>
    public void RegisterEntityIdentifier(Key key, string identifier)
    {
      var scope = scopes.HeadOrDefault as OperationRegistrationScope;
      if (scope==null)
        return;
      scope.RegisterEntityIdentifier(key, identifier);
    }

    /// <summary>
    /// Temporarily disables undo operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object enabling the logging back on its disposal.</returns>
    public IDisposable DisableUndoOperationRegistration()
    {
      if (!isUndoOperationRegistrationEnabled)
        return null;
      var result = new Disposable<OperationRegistry, bool>(this, isUndoOperationRegistrationEnabled,
        (disposing, _this, previousState) => _this.isUndoOperationRegistrationEnabled = previousState);
      isUndoOperationRegistrationEnabled = false;
      return result;
    }

    /// <summary>
    /// Temporarily disables system operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object enabling the logging back on its disposal.</returns>
    public IDisposable DisableSystemOperationRegistration()
    {
      if (!isSystemOperationRegistrationEnabled)
        return null;
      var result = new Disposable<OperationRegistry, bool>(this, isSystemOperationRegistrationEnabled,
        (disposing, _this, previousState) => _this.isSystemOperationRegistrationEnabled = previousState);
      isSystemOperationRegistrationEnabled = false;
      return result;
    }

    /// <summary>
    /// Temporarily enables system operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object disabling the logging back on its disposal.</returns>
    public IDisposable EnableSystemOperationRegistration()
    {
      if (isSystemOperationRegistrationEnabled)
        return null;
      var result = new Disposable<OperationRegistry, bool>(this, isSystemOperationRegistrationEnabled,
        (disposing, _this, previousState) => _this.isSystemOperationRegistrationEnabled = previousState);
      isSystemOperationRegistrationEnabled = true;
      return result;
    }

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operationType">Type of the operation.</param>
    /// <returns></returns>
    public CompletableScope BeginRegistration(OperationType operationType)
    {
      // Let's see if any kind of operation (incl. undo) is enabled for registration
      bool isRegistrationEnabled = IsOutermostOperationRegistrationEnabled; 
      if (!isRegistrationEnabled)
        return SetCurrentScope(blockingScope);
      
      CompletableScope currentScope;
      bool isSystemOperation = (operationType & OperationType.System)==OperationType.System;
      if (isSystemOperation) {
        if (!IsSystemOperationRegistrationEnabled)
          return SetCurrentScope(blockingScope);
        // otherwise we must create normal scope
        currentScope = GetCurrentScope();
      }
      else {
        currentScope = GetCurrentScope();
        bool currentScopeIsBlocking = currentScope!=null && (currentScope as OperationRegistrationScope)==null;
        if (currentScopeIsBlocking)
          return SetCurrentScope(blockingScope);
      }
      
      return SetCurrentScope(new OperationRegistrationScope(this, operationType, currentScope));
    }

    internal void CloseOperationRegistrationScope(OperationRegistrationScope scope)
    {
      Operation operation = null;
      try {
        operation = (Operation) scope.Operation;
        if (operation == null)
          return;
        if (!scope.IsOperationStarted) {
          if (scope.IsCompleted)
            throw new InvalidOperationException(Strings.ExOperationIsNotMarkedAsStarted);
          else
            // We can't throw an exception here, since it will suppress the thrown one.
            return;
        }

        if (scope.PrecedingOperations!=null)
          operation.PrecedingOperations = new ReadOnlyList<IOperation>(scope.PrecedingOperations);
        if (scope.FollowingOperations!=null)
          operation.FollowingOperations = new ReadOnlyList<IOperation>(scope.FollowingOperations);
        if (scope.UndoOperations!=null)
          operation.UndoOperations = new ReadOnlyList<IOperation>(scope.UndoOperations);
        if (scope.KeyByIdentifier!=null)
          operation.IdentifiedEntities = new ReadOnlyDictionary<string, Key>(scope.KeyByIdentifier);
      }
      finally {
        RemoveCurrentScope(scope);
      }
      // Adding it to parent scope's nested operations collection
      var parentScope = GetCurrentOrParentOperationRegistrationScope();
      if (parentScope != null) {
        if (!parentScope.IsOperationStarted) {
          if (parentScope.PrecedingOperations==null)
            parentScope.PrecedingOperations = new List<IOperation>();
          parentScope.PrecedingOperations.Add(operation);
        }
        else {
          if (parentScope.FollowingOperations==null)
            parentScope.FollowingOperations = new List<IOperation>();
          parentScope.FollowingOperations.Add(operation);
        }
      }
      // Notifying...
      if (scope.IsOutermost)
        NotifyOutermostOperationCompleted(operation, scope.IsCompleted);
      else
        NotifyNestedOperationCompleted(operation, scope.IsCompleted);
    }

    #region Events and notification methods

    /// <summary>
    /// Occurs when outermost <see cref="IOperation"/> is starting.
    /// </summary>
    public event EventHandler<OperationEventArgs> OutermostOperationStarting;

    /// <summary>
    /// Occurs when outermost <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationCompletedEventArgs> OutermostOperationCompleted;

    /// <summary>
    /// Occurs when nested <see cref="IOperation"/> is starting.
    /// </summary>
    public event EventHandler<OperationEventArgs> NestedOperationStarting;

    /// <summary>
    /// Occurs when nested <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationCompletedEventArgs> NestedOperationCompleted;

    /// <summary>
    /// Occurs when undo <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationEventArgs> UndoOperation;

    internal void NotifyOutermostOperationStarting(IOperation operation)
    {
      if (OutermostOperationStarting!=null && IsRegistrationEnabled)
        OutermostOperationStarting(this, new OperationEventArgs(operation));
    }

    internal void NotifyOutermostOperationCompleted(IOperation operation, bool isCompleted)
    {
      if (OutermostOperationCompleted!=null && IsRegistrationEnabled)
        OutermostOperationCompleted(this, new OperationCompletedEventArgs(operation, isCompleted));
    }

    internal void NotifyNestedOperationStarting(IOperation operation)
    {
      if (NestedOperationStarting!=null && IsRegistrationEnabled)
        NestedOperationStarting(this, new OperationEventArgs(operation));
    }

    internal void NotifyNestedOperationCompleted(IOperation operation, bool isCompleted)
    {
      if (NestedOperationCompleted!=null && IsRegistrationEnabled)
        NestedOperationCompleted(this, new OperationCompletedEventArgs(operation, isCompleted));
    }

    internal void NotifyUndoOperation(IOperation operation)
    {
      if (IsUndoOperationRegistrationEnabled)
        UndoOperation(this, new OperationEventArgs(operation));
    }

    #endregion

    #region Private \ internal methods

    internal CompletableScope GetCurrentScope()
    {
      return scopes.TailOrDefault;
    }

    private OperationRegistrationScope GetCurrentOperationRegistrationScope()
    {
      var scope = GetCurrentScope();
      if (scope==null)
        throw new InvalidOperationException(Strings.ExNoOperationRegistrationScope);
      return scope as OperationRegistrationScope;
    }

    private OperationRegistrationScope GetCurrentOrParentOperationRegistrationScope()
    {
      var scope = GetCurrentScope();
      if (scope==null)
        return null;
      var result = scope as OperationRegistrationScope;
      if (result!=null)
        return result;
      var list = new CompletableScope[scopes.Count];
      scopes.CopyTo(list, 0);
      for (int i = list.Length - 2; i>=0; i--) {
        result = list[i] as OperationRegistrationScope;
        if (result!=null)
          return result;
      }
      throw new InvalidOperationException(Strings.ExNoOperationRegistrationScope);
    }

    internal CompletableScope SetCurrentScope(CompletableScope scope)
    {
      scopes.AddTail(scope);
      return scope;
    }

    internal void RemoveCurrentScope(CompletableScope scope)
    {
      if (scopes.TailOrDefault!=scope)
        throw new InvalidOperationException(Strings.ExInvalidScopeDisposalOrder);
      scopes.ExtractTail();
    }

    internal long GetNextIdentifier()
    {
      var scope = scopes.HeadOrDefault as OperationRegistrationScope;
      return scope==null ? -1 : scope.CurrentIdentifier++;
    }

    #endregion


    // Constructors

    internal OperationRegistry(Session session)
    {
      Session = session;
      blockingScope = new BlockingOperationRegistrationScope(this);
    }
  }
}