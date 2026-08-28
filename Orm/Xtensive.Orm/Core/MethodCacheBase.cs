// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;


namespace Xtensive.Core
{
  /// <summary>
  /// Base class for any method caching class.
  /// </summary>
  /// <typeparam name="TImplementation">The type of <see cref="Implementation"/>.</typeparam>
  public abstract class MethodCacheBase<TImplementation>
    where TImplementation: class 
  {
    /// <summary>
    /// Gets underlying implementation object or interface.
    /// </summary>
    public readonly TImplementation Implementation;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="implementation"><see cref="Implementation"/> property value.</param>
    public MethodCacheBase(TImplementation implementation)
    {
      Implementation = implementation;
    }
  }
}