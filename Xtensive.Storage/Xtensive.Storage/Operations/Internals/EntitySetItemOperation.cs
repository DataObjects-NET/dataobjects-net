// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Operations.Internals
{
  /// <summary>
  /// Describes an operation with <see cref="EntitySet{TItem}"/> item.
  /// </summary>
  [Serializable]
  public abstract class EntitySetItemOperation : EntitySetOperation
  {
    /// <summary>
    /// Gets the key of the involved item.
    /// </summary>
    public Key ItemKey { get; set; }

    /// <inheritdoc/>
    public override string Description {
      get {
        return "{0}, Item Key = {1}".FormatWith(base.Description, ItemKey);
      }
    }

    /// <inheritdoc/>
    public override void Prepare(OperationExecutionContext context)
    {
      base.Prepare(context);
      context.RegisterKey(context.TryRemapKey(ItemKey), false);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    /// <param name="itemKey">The item key.</param>
    protected EntitySetItemOperation(Key key, FieldInfo field, Key itemKey)
      : base(key, field)
    {
      ArgumentValidator.EnsureArgumentNotNull(itemKey, "itemKey");
      ItemKey = itemKey;
    }

    // Serialization

    /// <inheritdoc/>
    protected override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("ItemKey", ItemKey.Format());
    }

    /// <inheritdoc/>
    protected EntitySetItemOperation(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      ItemKey = Key.Parse(info.GetString("ItemKey"));
      ItemKey.TypeRef = new TypeReference(ItemKey.TypeRef.Type, TypeReferenceAccuracy.ExactType);
    }
  }
}