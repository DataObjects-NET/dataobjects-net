// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.04

using System;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Enumerates all possible namespace treatment options in naming policy.
  /// </summary>
  [Serializable]
  public enum NamespacePolicy
  {
    /// <summary>
    /// Default value is <see cref="Omit"/>.
    /// </summary>
    Default = Omit,

    /// <summary>
    /// Only name of the type will be used to derive the name of
    /// the table or view.
    /// </summary>
    Omit = 0,

    /// <summary>
    /// Name of the type and namespace synonym will be used to derive 
    /// the name of the table or view.
    /// </summary>
    Synonymize = 1,

    /// <summary>
    /// Name of the type and namespace name will be used to derive 
    /// the name of the table or view.
    /// </summary>
    AsIs = 2,

    /// <summary>
    /// Name of the type and namespace hashes will be used to derive 
    /// the name of the table or view. Driver decides on the type of hashing
    /// algorythm to use.
    /// </summary>
    Hash = 3,
  }
}