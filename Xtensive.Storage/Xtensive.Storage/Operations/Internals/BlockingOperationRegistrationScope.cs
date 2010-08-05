// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.05

using System;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage.Operations
{
  internal sealed class BlockingOperationRegistrationScope : CompletableScope
  {
    private bool isDisposed;
    private OperationRegistry owner;
    
    public override void Complete()
    {
      return;
    }

    
    // Constructors

    public BlockingOperationRegistrationScope(OperationRegistry owner)
    {
      this.owner = owner;
    }

    // Disposal

    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      owner.SetCurrentScope(this);
    }
  }
}