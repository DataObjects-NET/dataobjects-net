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

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal sealed class EntitySetItemOperation : EntitySetOperation,
    ISerializable
  {
    private Key ItemKey { get; set; }

    /// <inheritdoc/>
    public override void Prepare(OperationExecutionContext context)
    {
      base.Prepare(context);
      if (context.KeysForRemap.Contains(ItemKey))
        ItemKey = context.KeyMapping[ItemKey];
      context.Register(ItemKey);
    }

    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var target = Query.Single(session, Key);
      var item = Query.Single(session, ItemKey);
      var entitySet = target.GetFieldValue<EntitySetBase>(Field);
      if (Type == OperationType.AddEntitySetItem)
        entitySet.Add(item);
      else
        entitySet.Remove(item);
    }

    
    // Constructors

    public EntitySetItemOperation(Key targetKey, FieldInfo fieldInfo, OperationType type, Key itemKey)
      : base(targetKey, type, fieldInfo)
    {
      if (!type.In(OperationType.AddEntitySetItem, OperationType.RemoveEntitySetItem))
        throw new InvalidOperationException();
      ItemKey = itemKey;
    }


    // Serialization

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("itemKey", ItemKey.Format());
    }

    /// <inheritdoc/>
    protected EntitySetItemOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      ItemKey = Key.Parse(info.GetString("itemKey"));
      ItemKey.TypeRef = new TypeReference(ItemKey.TypeRef.Type, TypeReferenceAccuracy.ExactType);
    }
  }
}