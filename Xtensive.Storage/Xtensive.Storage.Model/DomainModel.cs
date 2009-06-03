// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Represents a model of <see cref="Storage"/>.
  /// </summary>
  [Serializable]
  public sealed class DomainModel: Node
  {
    internal readonly object unlockKey = new object();

    /// <summary>
    /// Gets the <see cref="TypeInfo"/> instances contained in this instance.
    /// </summary>
    public TypeInfoCollection Types { get; private set; }

    /// <summary>
    /// Gets real indexes contained in this instance.
    /// </summary>
    public IndexInfoCollection RealIndexes { get; private set; }

    /// <summary>
    /// Gets the hierarchies.
    /// </summary>
    public HierarchyInfoCollection Hierarchies { get; private set; }

    /// <summary>
    /// Gets or sets the associations.
    /// </summary>
    public AssociationInfoCollection Associations { get; private set;}

    /// <summary>
    /// Gets or sets the generators.
    /// </summary>
    public GeneratorInfoCollection Generators { get; private set;}

    public object GetUnlockKey()
    {
      this.EnsureNotLocked();
      return unlockKey;
    }
 
    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      Hierarchies.Lock(true);
      Generators.Lock(true);
      Types.Lock(true);
      RealIndexes.Lock(true);
      Associations.Lock(true);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainModel"/> class.
    /// </summary>
    public DomainModel()
    {
      Types = new TypeInfoCollection();
      RealIndexes = new IndexInfoCollection();
      Hierarchies = new HierarchyInfoCollection();
      Associations = new AssociationInfoCollection();
      Generators = new GeneratorInfoCollection();
    }
  }
}