// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.07.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// A mapping model of storage.
  /// </summary>
  [Serializable]
  public sealed class DomainModel: Node
  {
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
    public AssociationInfoCollection Associations { get; private set; }

    /// <summary>
    /// Gets value indicating whenever this model defines mapping in multi-database mode.
    /// </summary>
    public bool IsMultidatabase
    {
      get { return (Attributes & DomainModelAttributes.Multidatabase)==DomainModelAttributes.Multidatabase; }
    }

    /// <summary>
    /// Gets value indicating whenever this model defines mapping in multi-schema mode.
    /// </summary>
    public bool IsMultischema
    {
      get { return (Attributes & DomainModelAttributes.Multischema)==DomainModelAttributes.Multischema; }
    }

    /// <summary>
    /// Gets model attributes.
    /// </summary>
    public DomainModelAttributes Attributes { get; private set; }

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
      UpdateAttributes();
    }

    private void UpdateAttributes()
    {
      var multischema = Types
        .Select(t => t.MappingSchema)
        .Where(schema => !string.IsNullOrEmpty(schema))
        .Distinct()
        .Count() > 1;

      var multidatabase = Types
        .Select(t => t.MappingDatabase)
        .Any(db => !string.IsNullOrEmpty(db));

      var attributes = DomainModelAttributes.None;
      if (multischema)
        attributes |= DomainModelAttributes.Multischema;
      if (multidatabase)
        attributes |= DomainModelAttributes.Multidatabase;
      Attributes = attributes;
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