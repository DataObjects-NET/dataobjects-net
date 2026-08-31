// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Text.Json.Serialization;
using Xtensive.Core;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes an operation with <see cref="Entity"/> field.
  /// </summary>
  public abstract class EntityFieldOperation : EntityOperation
  {
    /// <summary>
    /// Gets the field involved into the operation.
    /// </summary>
    [JsonInclude]
    public FieldInfo Field { get; private set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Description {
      get
      {
        return $"{base.Description}, Field = {Field.Name}";
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <param name="field">The field involved into the operation.</param>
    protected EntityFieldOperation(Key key, FieldInfo field)
      : base(key)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      Field = field;
    }
  }
}