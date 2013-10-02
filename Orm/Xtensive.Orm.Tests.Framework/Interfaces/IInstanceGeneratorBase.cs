// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.03.07

using System;
using System.Collections;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Base interface for any instance generator supported by
  /// <see cref="InstanceGeneratorProvider"/>.
  /// </summary>
  public interface IInstanceGeneratorBase
  {
    /// <summary>
    /// Gets the provider this instance generator is associated with.
    /// </summary>
    IInstanceGeneratorProvider Provider { get; }

    /// <summary>
    /// Gets new random instance.
    /// </summary>
    /// <returns>A new random instance.</returns>
    object GetInstance(Random random);

    /// <summary>
    /// Gets the enumerable providing new sequence of random instances.
    /// </summary>
    /// <returns>A new enumerable providing new sequence of random instances.</returns>
    IEnumerable GetInstances(Random random, int? count);
  }
}