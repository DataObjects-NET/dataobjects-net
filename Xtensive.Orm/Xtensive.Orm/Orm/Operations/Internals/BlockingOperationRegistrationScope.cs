// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.05

using System;
using Xtensive.Disposing;

namespace Xtensive.Orm.Operations
{
  internal sealed class BlockingOperationRegistrationScope : CompletableScope
  {
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
      owner.RemoveCurrentScope(this);
    }
  }
}