// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System.Collections.Generic;
using Xtensive.Modelling.Actions;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Difference contract.
  /// </summary>
  public interface IDifference
  {
    /// <summary>
    /// Gets the source object.
    /// </summary>
    object Source { get; }

    /// <summary>
    /// Gets the target object.
    /// </summary>
    object Target { get; }

    /// <summary>
    /// Gets the parent difference.
    /// <see langword="null" />, if none.
    /// </summary>
    Difference Parent { get; }

    /// <summary>
    /// Gets a value indicating whether this difference has changes.
    /// </summary>
    bool HasChanges { get; }
  }
}