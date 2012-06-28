// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.05

namespace Xtensive.Orm.Operations
{
  internal sealed class BlockingOperationRegistrationScope : ICompletableScope
  {
    private OperationRegistry owner;

    public bool IsCompleted { get; private set; }

    public void Complete()
    {
    }

    
    // Constructors

    public BlockingOperationRegistrationScope(OperationRegistry owner)
    {
      this.owner = owner;
    }

    // Disposal

    public void Dispose()
    {
      owner.RemoveCurrentScope(this);
    }
  }
}