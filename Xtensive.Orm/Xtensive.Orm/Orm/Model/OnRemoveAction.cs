// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.24

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Defines actions that can be taken to preserve referential integrity.
  /// </summary>
  [Serializable]
  public enum OnRemoveAction
  {
    /// <summary>
    /// The same as <see cref="Deny"/>.
    /// </summary>
    Default = Deny,

    /// <summary>
    /// Indicates that exception will be thrown in case that removing object 
    /// is referenced by other object.
    /// </summary>
    Deny = 0,

    /// <summary>
    /// Indicates that delete cascading operation will be taken on objects 
    /// that are referenced by removing object.
    /// </summary>
    Cascade = 1,

    /// <summary>
    /// Indicates that <see langword="null"/> value will be assigned 
    /// to corresponding reference fields of referencing object
    /// or the whole item that is used in n-ry relations will be removed.
    /// </summary>
    Clear = 2,

    /// <summary>
    /// Indicates that no action will be executed to corresponding reference field 
    /// of referencing object.
    /// </summary>
    /// <remarks>This option is prohibited for fields of <c>EntitySet&lt;T&gt;</c> type.</remarks>
    None = 3,
  }
}