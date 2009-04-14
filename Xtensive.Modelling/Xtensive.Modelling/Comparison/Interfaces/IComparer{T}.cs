// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Compares two objects of <typeparamref name="T"/> type.
  /// </summary>
  /// <typeparam name="T">The type of objects to compare.</typeparam>
  public interface IComparer<T> : IComparer
    where T: IModel
  {
    /// <summary>
    /// Gets the source object to compare.
    /// </summary>
    new T Source { get; }

    /// <summary>
    /// Gets the target object to compare.
    /// </summary>
    new T Target { get; }
  }
}