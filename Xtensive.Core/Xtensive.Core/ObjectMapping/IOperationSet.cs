// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Container for operations which may be used to apply found modifications to source objects.
  /// </summary>
  public interface IOperationSet
  {
    /// <summary>
    /// Gets a value indicating whether this instance contains any operations.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Applies operations being contained in this instance.
    /// </summary>
    void Apply();
  }
}