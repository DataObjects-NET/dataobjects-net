// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Referential integrity maintenance actions.
  /// </summary>
  [Serializable]
  public enum ReferentialAction
  {
    /// <summary>
    /// Do nothing to maintain referential integrity.
    /// </summary>
    None = 0,
    /// <summary>
    /// The same as <see cref="Restrict"/>.
    /// </summary>
    Default = Restrict,
    /// <summary>
    /// Restricts primary key update \ removal when it is referenced by some foreign key.
    /// </summary>
    Restrict = 1,
    /// <summary>
    /// Cascades primary key update \ removal to its foreign key.
    /// </summary>
    Cascade = 2,
    /// <summary>
    /// Clears the foreign key on its primary key update \ removal.
    /// </summary>
    Clear = 3,
  }
}