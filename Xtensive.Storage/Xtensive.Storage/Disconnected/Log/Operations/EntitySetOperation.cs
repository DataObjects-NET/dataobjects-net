// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  public class EntitySetOperation : IEntitySetOperation
  {
    public Key Key { get; private set; }
    public Key TargetKey { get; private set; }
    public EntityOperationType Type { get; private set; }
    public FieldInfo FieldInfo { get; private set; }

    public void Prepare(PrefetchContext prefetchContext)
    {
      throw new NotImplementedException();
    }

    public void Execute(IOperationExecutionContext executionContext)
    {
      throw new NotImplementedException();
    }

    
    // Constructors

    public EntitySetOperation(Key key, Key targetKey,  EntityOperationType type, FieldInfo fieldInfo)
    {
      if (type == EntityOperationType.Update)
        throw new InvalidOperationException();
      Key = key;
      TargetKey = targetKey;
      Type = type;
      FieldInfo = fieldInfo;
    }
  }
}