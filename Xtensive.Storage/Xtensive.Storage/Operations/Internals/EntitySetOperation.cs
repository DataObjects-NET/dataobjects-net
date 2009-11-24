// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.20

using System;
using System.Runtime.Serialization;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Operations
{
  [Serializable]
  internal class EntitySetOperation : EntityFieldOperation
  {
    /// <inheritdoc/>
    public override void Execute(OperationExecutionContext context)
    {
      base.Execute(context);
      if (Type != OperationType.ClearEntitySet) 
        return;
      var session = context.Session;
      var target = Query.Single(session, Key);
      var entitySet = target.GetFieldValue<EntitySetBase>(Field);
      entitySet.Clear();
    }

    // Constructors

    /// <inheritdoc/>
    public EntitySetOperation(Key targetKey, OperationType type, FieldInfo fieldInfo)
      : base(targetKey, type, fieldInfo)
    {}

    // Serializable

    /// <inheritdoc/>
    protected EntitySetOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {}
  }
}