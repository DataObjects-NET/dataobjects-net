// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Operations
{
  internal sealed class OperationRegistrationScope : CompletableScope
  {
    private bool isDisposed;

    public OperationRegistry Owner;
    public OperationRegistrationScope Parent;
    public OperationType OperationType;
    public IOperation Operation;
    public List<IPrecondition> Preconditions;
    public List<IOperation> NestedOperations;
    public List<IOperation> UndoOperations;
    public Dictionary<string, Key> KeyByIdentifier;
    public Dictionary<Key, string> IdentifierByKey;
    public long CurrentIdentifier;
    private bool oldIsSystemOperationRegistrationEnabled;

    /// <inheritdoc/>
    public void RegisterEntityIdentifier(Key key, string identifier)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");

      // Initializing dictionaries, if necessary
      if (IdentifierByKey == null) {
        IdentifierByKey = new Dictionary<Key, string>();
        KeyByIdentifier = new Dictionary<string, Key>();
      }

      // Removing existing records about the same entity or identifier
      string existingIdentifier = IdentifierByKey.GetValueOrDefault(key);
      if (existingIdentifier != null) {
        KeyByIdentifier.Remove(existingIdentifier);
        IdentifierByKey.Remove(key);
      }
      if (identifier==null)
        return;

      var existingKey = KeyByIdentifier.GetValueOrDefault(identifier);
      if (existingKey != null) {
        KeyByIdentifier.Remove(identifier);
        IdentifierByKey.Remove(existingKey);
      }

      KeyByIdentifier.Add(identifier, key);
      IdentifierByKey.Add(key, identifier);

      if (Owner.Session.IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXEntityWithKeyYIdentifiedAsZ,
          Owner.Session, key, identifier);
    }

    
    // Constructors

    public OperationRegistrationScope(OperationRegistry owner, OperationType operationType)
    {
      Owner = owner;
      OperationType = operationType;
      var currentScope = owner.GetCurrentScope();
      Parent = (OperationRegistrationScope) currentScope;

      oldIsSystemOperationRegistrationEnabled = owner.IsSystemOperationRegistrationEnabled;
      if (currentScope==null && ((operationType & OperationType.System)==OperationType.System))
        // We automatically disable system operation registration inside each of them;
        // they must be explicitly enabled via Session.Operations.EnableSystemOperationsRegistration() call,
        // that normally should wrap each invocation of event or overridable method.
        owner.IsSystemOperationRegistrationEnabled = false;
    }

    // Disposal

    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      Owner.CloseOperationRegistrationScope(this);
      Owner.IsSystemOperationRegistrationEnabled = oldIsSystemOperationRegistrationEnabled;
    }
  }
}