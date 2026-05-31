// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Xtensive.Core;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Describes operation over key set.
  /// </summary>
  [Serializable]
  public abstract class KeySetOperation : Operation
  {
    private IReadOnlyList<Key> keys = Array.Empty<Key>();

    /// <inheritdoc/>
    [JsonIgnore]
    public override string Description {
      get
      {
        return $"{Title}:{Environment.NewLine}{Keys.ToDelimitedString(Environment.NewLine).Indent(2)}";
      }
    }

    /// <summary>
    /// Gets the key set.
    /// </summary>
    [JsonInclude]
    public IReadOnlyList<Key> Keys {
      get;
      private set;
     }

    /// <inheritdoc/>
    protected override void PrepareSelf(OperationExecutionContext context)
    {
      foreach (var key in Keys)
        context.RegisterKey(context.TryRemapKey(key), false);
    }


    // Constructors

    /// <inheritdoc/>
    public KeySetOperation(Key key)
    {
      Keys = new[] { key }.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="keys">The sequence of keys.</param>
    public KeySetOperation(IReadOnlyList<Key> keys)
    {
      ArgumentNullException.ThrowIfNull(keys);
      if (keys.Count < 1)
        throw new ArgumentException("Keys collection must have at least 1 item");

      Keys = keys;
    }
  }
}