// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Text.Json.Serialization;
using Xtensive.Core;

using Xtensive.Orm.Model;
using Xtensive.Orm.Operations.Serialization.Json;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes an operation with <see cref="EntitySet{TItem}"/> item.
  /// </summary>
  public abstract class EntitySetItemOperation : EntitySetOperation
  {
    /// <summary>
    /// Gets the key of the involved item.
    /// </summary>
    [JsonInclude]
    public Key ItemKey { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Description {
      get
      {
        return $"{base.Description}, Item Key = {ItemKey}";
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      base.PrepareSelf(context);
      context.RegisterKey(context.TryRemapKey(ItemKey), false);
    }

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    /// <param name="itemKey">The item key.</param>
    protected EntitySetItemOperation(Key key, FieldInfo field, Key itemKey)
      : base(key, field)
    {
      ItemKey = itemKey ?? throw new ArgumentNullException(nameof(itemKey));
    }
  }
}