// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Storage.Linq;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  public class EntityOperation : IEntityOperation
  {
    public Key Key { get; private set;}

    public EntityOperationType Type { get; private set; }

    public void Prepare(PrefetchContext prefetchContext)
    {
      if (Type == EntityOperationType.Create)
        prefetchContext.RegisterNew(Key);
      else
        prefetchContext.Register(Key);
    }

    public void Execute(Session session)
    {
      if (Type == EntityOperationType.Create) {
        var entityType = Key.TypeRef.Type;
        var domain = session.Domain;
        session.CreateEntityState(Key);
      }
      else {
        var entity = Query.Single(session, Key);
        entity.Remove();
      }
    }


    // Constructors

    public EntityOperation(Key key, EntityOperationType type)
    {
      if (type.In(EntityOperationType.AddItem, EntityOperationType.RemoveItem, EntityOperationType.Update))
        throw new InvalidOperationException();
      Key = key;
      Type = type;
    }
  }
}