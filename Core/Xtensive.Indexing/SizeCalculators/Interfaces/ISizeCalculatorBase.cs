// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

namespace Xtensive.Indexing.SizeCalculators
{
  /// <summary>
  /// Base interface for any size calculator supported by
  /// <see cref="SizeCalculatorProvider"/>.
  /// </summary>
  public interface ISizeCalculatorBase
  {
    /// <summary>
    /// Gets the provider this size calculator is associated with.
    /// </summary>
    ISizeCalculatorProvider Provider { get; }

    /// <summary>
    /// Gets the default (minimal) size (the size of <see langword="default(T)"/>) 
    /// for the type handled by this size calculator.
    /// </summary>
    /// <returns>Default (minimal) size (the size of <see langword="default(T)"/> in bytes) 
    /// for the type handled by this size calculator.</returns>
    int GetDefaultSize();

    /// <summary>
    /// Gets the size of the specified <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance to get the size for.</param>
    /// <returns>Size (in bytes) of the specified <paramref name="instance"/>.</returns>
    int GetInstanceSize(object instance);
  }
}