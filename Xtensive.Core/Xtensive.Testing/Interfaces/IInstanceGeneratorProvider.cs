// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;

namespace Xtensive.Testing
{
  /// <summary>
  /// Instance generator provider.
  /// </summary>
  public interface IInstanceGeneratorProvider
  {
    /// <summary>
    /// Gets <see cref="IInstanceGenerator{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the hasher for.</typeparam>
    /// <returns><see cref="IInstanceGenerator{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    IInstanceGenerator<T> GetInstanceGenerator<T>();

    /// <summary>
    /// Gets <see cref="IInstanceGeneratorBase"/> for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to get the instance generator for.</param>
    /// <returns><see cref="IInstanceGenerator{T}"/> for the specified <paramref name="type"/>.</returns>
    IInstanceGeneratorBase GetInstanceGenerator(Type type);
  }
}