// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Definitions
{
  [Serializable]
  public sealed class HierarchyDef : Node
  {
    private readonly DirectionCollection<KeyField> keyFields = new DirectionCollection<KeyField>();
    private Type generator;

    /// <summary>
    /// Gets the fields that are included in the key for this instance.
    /// </summary>
    public DirectionCollection<KeyField> KeyFields
    {
      get { return keyFields; }
    }

    /// <summary>
    /// Gets or sets the type instance of which is responsible for key generation.
    /// </summary>
    public Type Generator
    {
      get { return generator; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        generator = value;
      }
    }

    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    public TypeDef Root { get; internal set; }

    /// <summary>
    /// Gets the <see cref="InheritanceSchema"/> for this hierarchy.
    /// </summary>
    public InheritanceSchema Schema { get; set; }

    internal HierarchyDef(TypeDef root)
    {
      Root = root;
    }
  }
}