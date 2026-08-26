// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Text.Json.Serialization;
using Xtensive.Orm.Operations.Serialization.Json;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes an operation involving the <see cref="Key"/>.
  /// </summary>
  [Serializable]
  public abstract class KeyOperation : Operation
  {
    /// <summary>
    /// Gets the key of the entity.
    /// </summary>
    [JsonInclude]
    public Key Key { get; private set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Description {
      get
      {
        return $"{Title}, Key = {Key}";
      }
    }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      context.RegisterKey(context.TryRemapKey(Key), false);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class..
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    protected KeyOperation(Key key)
    {
      Key = key;
    }
  }
}