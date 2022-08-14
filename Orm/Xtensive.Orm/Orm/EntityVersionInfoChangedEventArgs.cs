// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;

using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/>.<see cref="Entity.VersionInfo"/> change-related events.
  /// </summary>
  public class EntityVersionInfoChangedEventArgs : EntityFieldEventArgs
  {
    /// <summary>
    /// Gets or sets a value indicating whether 
    /// <see cref="Entity.VersionInfo"/> was changed or not.
    /// </summary>
    public bool Changed { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="changedEntity">The entity that was changed.</param>
    /// <param name="changedField">The field that was changed.</param>
    /// <param name="changed"><see cref="Changed"/> property value.</param>
    public EntityVersionInfoChangedEventArgs(Entity changedEntity, FieldInfo changedField, bool changed)
      : base(changedEntity, changedField)
    {
      Changed = changed;
    }
  }
}