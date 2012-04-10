// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.10

using System;

using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes referential relationship between two particular entities.
  /// </summary>
  [Serializable]
  public struct ReferenceInfo
  {
    /// <summary>
    /// Gets the referencing entity.
    /// </summary>
    public Entity ReferencingEntity { get; private set; }

    /// <summary>
    /// Gets the referenced entity.
    /// </summary>
    public Entity ReferencedEntity { get; private set; }

    /// <summary>
    /// Gets the <see cref="AssociationInfo"/> object describing the relationship.
    /// </summary>
    public AssociationInfo Association { get; private set; }

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="referencingEntity">The referencing entity.</param>
    /// <param name="referencedEntity">The referenced entity.</param>
    /// <param name="association">The association.</param>
    public ReferenceInfo(Entity referencingEntity, Entity referencedEntity, AssociationInfo association)
      : this()
    {
      ReferencingEntity = referencingEntity;
      ReferencedEntity = referencedEntity;
      Association = association;
    }
  }
}