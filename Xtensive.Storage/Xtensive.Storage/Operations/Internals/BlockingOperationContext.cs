// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.18

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Storage.Operations
{
  internal sealed class BlockingOperationContext : IOperationContext
  {
    private readonly Session session;

    public bool IsLoggingEnabled { get { return false; } }

    public bool IsIntermediate { get { return false; } }

    public void LogOperation(IOperation operation)
    {
      var currentOperationContext = session.CurrentOperationContext;
      if (currentOperationContext!=null && (operation is IPrecondition))
        // Here we log only preconditions
        currentOperationContext.LogOperation(operation);
    }

    public void Complete()
    {}

    public IEnumerator<IOperation> GetEnumerator()
    {
      yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    public BlockingOperationContext(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
    }

    // Disposing

    public void Dispose()
    {}
  }
}