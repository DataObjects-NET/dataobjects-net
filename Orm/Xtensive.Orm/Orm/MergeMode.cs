// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.26

namespace Xtensive.Orm
{
  /// <summary>
  /// Enumerate possible ways of merging new entity states 
  /// into the <see cref="DisconnectedState"/>.
  /// </summary>
  public enum MergeMode
  {
    /// <summary>
    /// Default value. 
    /// The same as <see cref="Strict"/>.
    /// </summary>
    Default = Strict,
    /// <summary>
    /// An exception must be thrown on any version conflict.
    /// </summary>
    Strict = 0,
    /// <summary>
    /// New (source) field values are preferred;
    /// new value will overwrite existing one, 
    /// if both values are available for a particular field.
    /// </summary>
    PreferNew = 1,
    /// <summary>
    /// Original (existing) field values are preferred;
    /// new value will not overwrite existing one, 
    /// if both values are available for a particular field.
    /// </summary>
    PreferOriginal = 2,
  }
}