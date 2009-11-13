// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  public class EntitySetOperation : IEntitySetOperation,
    ISerializable
  {
    public Key Key { get; private set; }
    public Key TargetKey { get; private set; }
    public EntityOperationType Type { get; private set; }
    public FieldInfo Field { get; private set; }

    public void Prepare(PrefetchContext prefetchContext)
    {
      prefetchContext.Register(Key);
      prefetchContext.Register(TargetKey);
    }

    public void Execute(Session session)
    {
      var target = Query.Single(session, TargetKey);
      var entity = Query.Single(session, Key);
      var entitySet = target.GetFieldValue<EntitySetBase>(Field);
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
      Field = fieldInfo;
    }


    // Serialization

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("key", Key.Format());
      info.AddValue("targetKey", TargetKey.Format());
      info.AddValue("type", Type, typeof(EntityOperationType));
      info.AddValue("field", new FieldInfoRef(Field), typeof(FieldInfoRef));
    }

    protected EntitySetOperation(SerializationInfo info, StreamingContext context)
    {
      Key = Key.Parse(info.GetString("key"));
      Key.TypeRef = new TypeReference(Key.TypeRef.Type, TypeReferenceAccuracy.ExactType);
      TargetKey = Key.Parse(info.GetString("targetKey"));
      TargetKey.TypeRef = new TypeReference(TargetKey.TypeRef.Type, TypeReferenceAccuracy.ExactType);
      Type = (EntityOperationType) info.GetInt32("type");
      var fieldRef = (FieldInfoRef)info.GetValue("field", typeof (FieldInfoRef));
      Field = fieldRef.Resolve(Session.Demand().Domain.Model);
    }
  }
}