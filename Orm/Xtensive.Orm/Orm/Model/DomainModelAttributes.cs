// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.10

using System;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// <see cref="DomainModel"/> attributes.
  /// </summary>
  [Flags]
  public enum DomainModelAttributes
  {
    /// <summary>
    /// Empty attribute set.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Multiple schemas are involved in mapping.
    /// </summary>
    Multischema = 0x1,

    /// <summary>
    /// Multiple databases are involved in mapping.
    /// This implies <see cref="Multischema"/>.
    /// </summary>
    Multidatabase = 0x3,
  }
}