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
      var remappedKey = context.TryRemapKey(Key);
      context.RegisterKey(remappedKey, Type == OperationType.CreateEntity);
    }

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var key = context.TryRemapKey(Key);
      if (Type == OperationType.CreateEntity) {
        var entityType = key.TypeRef.Type;
        var domain = session.Domain;
        session.CreateEntityState(key);
      }
      else {
        var entity = Query.Single(session, key);
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