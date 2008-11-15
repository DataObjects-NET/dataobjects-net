// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Represents a model of <see cref="Storage"/>.
  /// </summary>
  [Serializable]
  public sealed class DomainModel: Node
  {
    /// <summary>
    /// Gets the services contained in this instance.
    /// </summary>
    public ServiceInfoCollection Services { get; private set; }

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

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      Services.Lock(true);
      Hierarchies.Lock(true);
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
      Services = new ServiceInfoCollection();
      Types = new TypeInfoCollection();
      RealIndexes = new IndexInfoCollection();
      Hierarchies = new HierarchyInfoCollection();
      Associations = new AssociationInfoCollection();
    }
  }
}