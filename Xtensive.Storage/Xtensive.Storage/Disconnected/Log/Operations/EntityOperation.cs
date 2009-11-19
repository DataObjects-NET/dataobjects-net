// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  public sealed class EntityOperation : IEntityOperation,
    ISerializable
  {
    public Key Key { get; private set;}

    public EntityOperationType Type { get; private set; }

    public void Prepare(OperationContext operationContext)
    {
      if (operationContext.KeysForRemap.Contains(Key)) {
        var oldKey = Key;
        Key newKey;
        if (!operationContext.KeyMapping.TryGetValue(oldKey, out newKey)) {
          newKey = KeyFactory.CreateNext(operationContext.Session.Domain, oldKey.Type);
          operationContext.KeyMapping.Add(oldKey, newKey);
        }
        Key = newKey;
      }
      if (Type == EntityOperationType.Create) 
        operationContext.RegisterNew(Key);
      else
        operationContext.Register(Key);
    }

    public void Execute(OperationContext operationContext)
    {
      var session = operationContext.Session;
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

    // Serialization

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("key", Key.Format());
      info.AddValue("type", Type, typeof(EntityOperationType));
    }

    protected EntityOperation(SerializationInfo info, StreamingContext context)
    {
      Key = Key.Parse(info.GetString("key"));
      Key.TypeRef = new TypeReference(Key.TypeRef.Type, TypeReferenceAccuracy.ExactType);
      Type = (EntityOperationType)info.GetValue("type", typeof(EntityOperationType));
    }
  }
}