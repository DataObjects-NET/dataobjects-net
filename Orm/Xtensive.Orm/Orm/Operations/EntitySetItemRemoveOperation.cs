// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Runtime.Serialization;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes <see cref="EntitySet{TItem}"/> item add operation.
  /// </summary>
  [Serializable]
  public class EntitySetItemRemoveOperation : EntitySetItemOperation
  {
    private readonly EntityRemoveReason reason;

    /// <inheritdoc/>
    public override string Title {
      get { return "Remove item from entity set"; }
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var item = session.Query.Single(context.TryRemapKey(ItemKey));
      GetEntitySet(context).Remove(item, this.reason);
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntitySetItemRemoveOperation(Key, Field, ItemKey, reason);
      return clone;
    }
    
    // Constructors

    /// <inheritdoc/>
    public EntitySetItemRemoveOperation(Key key, FieldInfo field, Key itemKey, EntityRemoveReason reason)
      : base(key, field, itemKey)
    {
      this.reason = reason;
    }

    /// <inheritdoc/>
    protected EntitySetItemRemoveOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}