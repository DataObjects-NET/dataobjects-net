// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// Defines a signle persistent type hierarchy.
  /// </summary>
  [Serializable]
  public sealed class HierarchyDef : Node
  {
    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    public TypeDef Root { get; private set; }

    /// <summary>
    /// Gets the fields that are included in the key for this hierarchy.
    /// </summary>
    public List<KeyField> KeyFields { get; private set; }

    /// <summary>
    /// Gets the <see cref="InheritanceSchema"/> for this hierarchy.
    /// </summary>
    public InheritanceSchema Schema { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether key includes TypeId field.
    /// </summary>
    public bool IncludeTypeId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether primary key for this hierarchy is clustered.
    /// </summary>
    public bool IsClustered { get; set; }

    /// <summary>
    /// Gets or sets the key generator kind to use in this hierarchy.
    /// </summary>
    public KeyGeneratorKind KeyGeneratorKind { get; set; }

    /// <summary>
    /// Gets or sets the key generator name to use in this hierarchy.
    /// </summary>
    public string KeyGeneratorName { get; set; }


    // Constructors

    internal HierarchyDef(TypeDef root)
    {
      Root = root;
      KeyFields = new List<KeyField>(WellKnown.MaxKeyFieldNumber);
      IsClustered = true;
      KeyGeneratorKind = KeyGeneratorKind.Default;
    }
  }
}