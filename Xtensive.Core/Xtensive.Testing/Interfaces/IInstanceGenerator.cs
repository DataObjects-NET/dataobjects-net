// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;
using System.Collections.Generic;

namespace Xtensive.Testing
{
  /// <summary>
  /// Generates random instances of specified type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">Type of instances to generate.</typeparam>
  public interface IInstanceGenerator<T>: IInstanceGeneratorBase
  {
    /// <summary>
    /// Gets new random instance.
    /// </summary>
    /// <returns>A new random instance.</returns>
    new T GetInstance(Random random);

    /// <summary>
    /// Gets the enumerable providing new sequence of random instances.
    /// </summary>
    /// <returns>A new enumerable providing new sequence of random instances.</returns>
    new IEnumerable<T> GetInstances(Random random, int? count);
  }
}
