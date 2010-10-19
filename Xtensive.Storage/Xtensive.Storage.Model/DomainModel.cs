// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Helpers;

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
    /// Gets full-text indexes contained in this instance.
    /// </summary>
    public FullTextIndexInfoCollection FullTextIndexes { get; private set; }

    /// <summary>
    /// Gets the hierarchies.
    /// </summary>
    public HierarchyInfoCollection Hierarchies { get; private set; }

    /// <summary>
    /// Gets the collection providing information about associations.
    /// </summary>
    public AssociationInfoCollection Associations { get; private set;}
 
    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      if (!recursive)
        return;
      Hierarchies.UpdateState(true);
      Types.UpdateState(true);
      RealIndexes.UpdateState(true);
      Associations.UpdateState(true);
    }
 
    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      Hierarchies.Lock(true);
      Types.Lock(true);
      RealIndexes.Lock(true);
      Associations.Lock(true);
      FullTextIndexes.Lock(true);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainModel"/> class.
    /// </summary>
    public DomainModel()
    {
      Types = new TypeInfoCollection(this, "Types");
      RealIndexes = new IndexInfoCollection(this, "RealIndexes");
      Hierarchies = new HierarchyInfoCollection(this, "Hierarchies");
      Associations = new AssociationInfoCollection(this, "Associations");
      FullTextIndexes = new FullTextIndexInfoCollection();
    }
  }
}