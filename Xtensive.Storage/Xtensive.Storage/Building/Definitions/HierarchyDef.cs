// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Definitions
{
  [Serializable]
  public sealed class HierarchyDef : Node
  {
    private Type keyGenerator;
    private int? keyGeneratorCacheSize;

    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    public TypeDef Root { get; internal set; }

    /// <summary>
    /// Gets the fields that are included in the key for this hierarchy.
    /// </summary>
    public List<KeyField> KeyFields { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether key should include TypeId field.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if TypeId field should be included into key; otherwise, <see langword="false"/>.
    /// </value>
    public bool IncludeTypeId { get; set; }

    /// <summary>
    /// Gets the <see cref="InheritanceSchema"/> for this hierarchy.
    /// </summary>
    public InheritanceSchema Schema { get; set; }

    /// <summary>
    /// Gets or sets the size of the key generator cache.
    /// </summary>
    public int? KeyGeneratorCacheSize
    {
      get { return keyGeneratorCacheSize; }
      set
      {
        if (value.HasValue)
          ArgumentValidator.EnsureArgumentIsGreaterThan(value.Value, -1, "KeyGeneratorCacheSize");
        keyGeneratorCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the type instance of which is responsible for key generation.
    /// </summary>
    public Type KeyGenerator
    {
      get { return keyGenerator; }
      set
      {
        if (value != null)
          Validator.ValidateKeyGeneratorType(value);

        keyGenerator = value;

        // Syncs keyGeneratorCacheSize with the presence of generator type.
        if (keyGenerator == null)
          keyGeneratorCacheSize = null;
      }
    }

    internal HierarchyDef(TypeDef root)
    {
      Root = root;
      KeyFields = new List<KeyField>(WellKnown.MaxKeyFieldNumber);
      KeyGenerator = typeof (KeyGenerator);
    }
  }
}