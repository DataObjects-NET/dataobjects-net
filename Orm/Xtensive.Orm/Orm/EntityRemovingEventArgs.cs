// Copyright (C) 2003-2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.10.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/>-removing related events.
  /// </summary>
  public class EntityRemovingEventArgs : EntityEventArgs
  {
    /// <summary>
    /// Gets the entity remove reason.
    /// </summary>
    public EntityRemoveReason Reason { get; }

    // Constructors
    public EntityRemovingEventArgs(Entity entity, EntityRemoveReason reason)
      : base(entity)
    {
      this.Reason = reason;
    }
  }
}