// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  public class EntityOperation : IEntityOperation
  {
    public Key Key { get; private set;}

    public EntityOperationType Type { get; private set; }

    public void Prepare(PrefetchContext prefetchContext)
    {
      throw new NotImplementedException();
    }

    public void Execute(IOperationExecutionContext executionContext)
    {
      throw new NotImplementedException();
    }


    // Constructors

    public EntityOperation(Key key, EntityOperationType type)
    {
      if (type == EntityOperationType.Update)
        throw new InvalidOperationException();
      Key = key;
      Type = type;
    }
  }
}