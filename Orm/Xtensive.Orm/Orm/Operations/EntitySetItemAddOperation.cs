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
  public class EntitySetItemAddOperation : EntitySetItemOperation
  {
    /// <inheritdoc/>
    public override string Title {
      get { return "Add item to entity set"; }
    }

    /// <inheritdoc/>
    protected override void ExecuteSelf(OperationExecutionContext context)
    {
      var session = context.Session;
      var item = session.Query.Single(context.TryRemapKey(ItemKey));
      GetEntitySet(context).Add(item);
    }

    /// <inheritdoc/>
    protected override Operation CloneSelf(Operation clone)
    {
      if (clone==null)
        clone = new EntitySetItemAddOperation(Key, Field, ItemKey);
      return clone;
    }
    
    // Constructors

    /// <inheritdoc/>
    public EntitySetItemAddOperation(Key key, FieldInfo field, Key itemKey)
      : base(key, field, itemKey)
    {
    }

    /// <inheritdoc/>
    protected EntitySetItemAddOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}