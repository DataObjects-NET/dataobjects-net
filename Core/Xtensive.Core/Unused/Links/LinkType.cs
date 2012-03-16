// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;

namespace Xtensive.Core.Links
{
  /// <summary>
  /// Possible link (relation) types.
  /// </summary>
  [Serializable]
  public enum LinkType
  {
    /// <summary>
    /// Unknown relation type.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// One-to-one relation type.
    /// </summary>
    OneToOne = 1,
    /// <summary>
    /// One-to-many relation type.
    /// </summary>
    OneToMany = 2,
    /// <summary>
    /// Many-to-many relation type.
    /// </summary>
    ManyToMany = 4,
  }
}