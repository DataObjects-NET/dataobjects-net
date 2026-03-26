// Copyright (C) 2018-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      Reason = reason;
    }
  }
}