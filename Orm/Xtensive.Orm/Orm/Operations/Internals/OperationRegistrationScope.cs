// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Disposing;


namespace Xtensive.Orm.Operations
{
  internal sealed class OperationRegistrationScope : ICompletableScope
  {
    private bool isDisposed;

    public OperationRegistry Owner;
    public bool IsOutermost;
    public OperationType OperationType;
    public IOperation Operation;
    public bool IsOperationStarted;
    public List<IOperation> PrecedingOperations;
    public List<IOperation> FollowingOperations;
    public List<IOperation> UndoOperations;
    public Dictionary<string, Key> KeyByIdentifier;
    public Dictionary<Key, string> IdentifierByKey;
    public long CurrentIdentifier;
    private bool oldIsSystemOperationRegistrationEnabled;

    public bool IsCompleted { get; private set; }

    public void Complete()
    {
      IsCompleted = true;
    }

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

      OrmLog.Debug(Strings.LogSessionXEntityWithKeyYIdentifiedAsZ, Owner.Session, key, identifier);
    }

    
    // Constructors

    public OperationRegistrationScope(OperationRegistry owner, OperationType operationType, ICompletableScope currentScope)
    {
      Owner = owner;
      OperationType = operationType;
      IsOutermost = currentScope==null;

      oldIsSystemOperationRegistrationEnabled = owner.IsSystemOperationRegistrationEnabled;
      if ((operationType & OperationType.System)==OperationType.System)
        owner.IsSystemOperationRegistrationEnabled = false;
    }

    // Disposal

    public void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      try {
        Owner.CloseOperationRegistrationScope(this);
      }
      finally {
        Owner.IsSystemOperationRegistrationEnabled = oldIsSystemOperationRegistrationEnabled;
      }
    }
  }
}