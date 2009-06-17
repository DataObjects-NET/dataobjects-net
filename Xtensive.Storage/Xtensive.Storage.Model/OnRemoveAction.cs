// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.24

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Defines actions that can be taken to preserve referential integrity.
  /// </summary>
  [Serializable]
  public enum OnRemoveAction
  {
    /// <summary>
    /// Indicates that no action will be taken.
    /// </summary>
    None = 0,

    /// <summary>
    /// The same as <see cref="Deny"/>.
    /// </summary>
    Default = Deny,

    /// <summary>
    /// Indicates that exception will be thrown in case that removing entity 
    /// is referenced by other entities.
    /// </summary>
    Deny = 1,

    /// <summary>
    /// Indicates that delete cascading operation will be taken on entities 
    /// that are referenced by removing entity.
    /// </summary>
    Cascade = 2,

    /// <summary>
    /// Indicates that <see langword="null"/> value will be assigned 
    /// to corresponding reference fields of referencing entities
    /// or the whole item that is used in n-ry relations will be removed.
    /// </summary>
    Clear = 3,
  }
}