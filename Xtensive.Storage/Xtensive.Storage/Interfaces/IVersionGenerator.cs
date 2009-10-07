// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.02

namespace Xtensive.Storage
{
  /// <summary>
  /// Generates version values.
  /// </summary>
  /// <typeparam name="T">Type of version value.</typeparam>
  public interface IVersionGenerator
  {
    /// <summary>
    /// Generates the next version value.
    /// </summary>
    /// <param name="currentValue">The current version value.</param>
    /// <returns>New version value.</returns>
    object GetNextVersion(object currentValue);
  }
}