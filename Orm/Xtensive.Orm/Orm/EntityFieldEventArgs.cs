// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.10.08


using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/> field-related events.
  /// </summary>
  public class EntityFieldEventArgs : EntityEventArgs
  {
    /// <summary>
    /// Gets the field to which this event is related.
    /// </summary>
    public FieldInfo Field { get; }


    // Constructors


    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    public EntityFieldEventArgs(Entity entity, FieldInfo field)
      : base(entity)
    {
      Field = field;
    }
  }
}