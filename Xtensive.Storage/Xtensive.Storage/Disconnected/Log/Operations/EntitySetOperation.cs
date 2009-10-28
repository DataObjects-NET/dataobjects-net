// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Storage.Linq;
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
      prefetchContext.Register(Key);
      prefetchContext.Register(TargetKey);
    }

    public void Execute(Session session)
    {
      var target = Query.Single(session, TargetKey);
      var entity = Query.Single(session, Key);
      var entitySet = target.GetFieldValue<EntitySetBase>(FieldInfo);
      if (Type == EntityOperationType.AddItem)
        entitySet.Add(entity);
      else
        entitySet.Remove(entity);
    }

    
    // Constructors

    public EntitySetOperation(Key key, Key targetKey, FieldInfo fieldInfo, EntityOperationType type)
    {
      if (!type.In(EntityOperationType.AddItem, EntityOperationType.RemoveItem))
        throw new InvalidOperationException();
      Key = key;
      TargetKey = targetKey;
      Type = type;
      FieldInfo = fieldInfo;
    }
  }
}