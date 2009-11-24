// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  [DebuggerDisplay("Key = {Key}, Type = {Type}")]
  internal class EntityOperation : Operation
  {
    protected Key Key { get; private set;}

    /// <inheritdoc/>
    public override void Prepare(OperationExecutionContext context)
    {
      if (context.KeysForRemap.Contains(Key)) {
        var oldKey = Key;
        Key newKey;
        if (!context.KeyMapping.TryGetValue(oldKey, out newKey)) {
          newKey = KeyFactory.CreateNext(context.Session.Domain, oldKey.Type);
          context.KeyMapping.Add(oldKey, newKey);
        }
        Key = newKey;
      }
      if (Type == OperationType.CreateEntity) 
        context.RegisterNew(Key);
      else
        context.Register(Key);
    }

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      if (Type == OperationType.CreateEntity) {
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

    /// <inheritdoc/>
    public EntityOperation(Key key, OperationType type)
      : base(type)
    {
      Key = key;
    }

    // Serialization

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("key", Key.Format());
    }

    /// <inheritdoc/>
    protected EntityOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Key = Key.Parse(info.GetString("key"));
      Key.TypeRef = new TypeReference(Key.TypeRef.Type, TypeReferenceAccuracy.ExactType);
    }
  }
}