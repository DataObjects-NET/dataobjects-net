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

    public bool AreNormalOperationAccepted {get { return false; }}

    public bool DisableNested { get { return true; } }

    public void Add(IOperation operation)
    {}

    public void Add(IOperation operation, bool highPriority)
    {
      if (highPriority && session.CurrentOperationContext!=null)
        session.CurrentOperationContext.Add(operation);
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