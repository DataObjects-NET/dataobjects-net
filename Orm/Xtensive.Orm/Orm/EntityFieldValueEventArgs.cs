// Copyright (C) 2009-2010 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.10.08


using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/> field related events containing field value.
  /// </summary>
  public class EntityFieldValueEventArgs : EntityFieldEventArgs
  {
    /// <summary>
    /// Gets the field value.
    /// </summary>
    public object Value { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="value">The <see cref="Value"/> property value.</param>
    public EntityFieldValueEventArgs(Entity entity, FieldInfo field, object value)
      : base(entity, field)
    {
      Value = value;
    }
  }
}