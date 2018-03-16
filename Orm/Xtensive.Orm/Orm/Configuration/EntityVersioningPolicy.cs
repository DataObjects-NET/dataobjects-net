// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.03.03

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Enumerates all possible ways of Entity version changing.
  /// </summary>
  public enum EntityVersioningPolicy
  {
    /// <summary>
    /// See <see cref="Pessimistic"/>.
    /// </summary>
    Default = Pessimistic,

    /// <summary>
    /// An Entity version changes on every attempt to change the Entity.
    /// <remarks>
    /// The version will be changes even if the current value of changing field and the value which is about to set are actually the same.
    /// </remarks>
    /// </summary>
    Pessimistic = 0,

    /// <summary>
    /// An Entity version changes only after the Entity field value is actually about to be changed.
    /// <remarks>
    /// If the current value of the field and the value which is setting are the same then the version of the entity doesn't change.
    /// </remarks>
    /// </summary>
    Optimistic = 1,

    
  }
}
