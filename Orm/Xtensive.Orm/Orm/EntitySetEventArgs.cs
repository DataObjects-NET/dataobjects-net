// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.23

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Orm.EntitySet{TItem}"/>-related events.
  /// </summary>
  public class EntitySetEventArgs : EntityFieldEventArgs
  {
    /// <summary>
    /// Gets the <see cref="EntitySetBase"/> to which this event is related.
    /// </summary>
    public EntitySetBase EntitySet { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entitySet">The entity set.</param>
    public EntitySetEventArgs(EntitySetBase entitySet)
      : base(entitySet.Owner, entitySet.Field)
    {
      EntitySet = entitySet;
    }
  }
}