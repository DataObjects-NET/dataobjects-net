// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.27

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Interface for representing reference to object.
  /// </summary>
  public interface IReference : 
    IEquatable<IReference>
  {
    /// <summary>
    /// Gets the identifying string of this reference.
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Tries to resolve the reference.
    /// </summary>
    /// <param name="target">The object reference is pointing to.</param>
    /// <returns><see langword="True" /> if reference was resolved successfully;
    /// otherwise, <see langword="false" />.</returns>
    bool TryResolve(out object target);

    /// <summary>
    /// Indicates whether reference is the only possible reference to its target,
    /// so it can be cached.
    /// </summary>
    bool IsUnique { get; }

    /// <summary>
    /// Specify if it is necessary to put referencing object to <see cref="SerializationContext.SerializationQueue"/>.
    /// </summary>
    bool IsQueueable { get; }
  }
}