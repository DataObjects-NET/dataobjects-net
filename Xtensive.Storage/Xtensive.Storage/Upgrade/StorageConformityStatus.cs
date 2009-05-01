// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Defines conformity of storage model with domain model.
  /// </summary>
  public enum StorageConformityStatus
  {
    /// <summary>
    /// Storage model equal to domain model.
    /// </summary>
    Match,
    /// <summary>
    /// Stoarge model contains additional elements.
    /// </summary>
    Greater,
    /// <summary>
    /// Stoarge model has not some elements of domain model.
    /// </summary>
    Less,
    /// <summary>
    /// Stoarge model contains additional elements and 
    /// has not some elements of domain model.
    /// </summary>
    Mismatch
  }
}