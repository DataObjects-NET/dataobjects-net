// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.25

using System;
using System.Runtime.Serialization;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Operations
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
    public override void Execute(OperationExecutionContext context)
    {
      var session = context.Session;
      var item = Query.Single(session, context.TryRemapKey(ItemKey));
      GetEntitySet(context).Add(item);
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