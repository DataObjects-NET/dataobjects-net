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
  public readonly struct ReferenceInfo
  {
    /// <summary>
    /// Gets the referencing entity.
    /// </summary>
    public readonly Entity ReferencingEntity;

    /// <summary>
    /// Gets the referenced entity.
    /// </summary>
    public readonly Entity ReferencedEntity;

    /// <summary>
    /// Gets the <see cref="AssociationInfo"/> object describing the relationship.
    /// </summary>
    public readonly AssociationInfo Association;

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="referencingEntity">The referencing entity.</param>
    /// <param name="referencedEntity">The referenced entity.</param>
    /// <param name="association">The association.</param>
    public ReferenceInfo(Entity referencingEntity, Entity referencedEntity, AssociationInfo association)
    {
      ReferencingEntity = referencingEntity;
      ReferencedEntity = referencedEntity;
      Association = association;
    }
  }
}