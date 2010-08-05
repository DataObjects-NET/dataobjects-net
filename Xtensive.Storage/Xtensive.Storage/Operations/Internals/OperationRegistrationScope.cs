// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Disposing;

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
    public Dictionary<string, Key> IdentifiedEntities;
    public long CurrentIdentifier;

    
    // Constructors

    public OperationRegistrationScope(OperationRegistry owner, OperationType operationType)
    {
      Owner = owner;
      OperationType = operationType;
      Parent = (OperationRegistrationScope) owner.GetCurrentScope();
    }

    // Disposal

    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      Owner.CloseOperationRegistrationScope(this);
    }
  }
}