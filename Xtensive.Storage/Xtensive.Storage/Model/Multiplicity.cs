// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Multiplicity of relationship.
  /// </summary>
  public enum Multiplicity
  {
    /// <summary>
    /// Zero to one. 
    /// </summary>
    ZeroToOne = 1,

    /// <summary>
    /// Zero to many.
    /// </summary>
    ZeroToMany = 2,

    /// <summary>
    /// One to one.
    /// </summary>
    OneToOne = 3,

    /// <summary>
    /// One to many.
    /// </summary>
    OneToMany = 4,

    /// <summary>
    /// Many to one.
    /// </summary>
    ManyToOne = 5,

    /// <summary>
    /// Many to many.
    /// </summary>
    ManyToMany = 6,
  }
}