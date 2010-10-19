// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.03

using System;
using System.Collections.Generic;

namespace Xtensive.Collections
{
  /// <summary>
  /// A collection of type-based extensions.
  /// </summary>
  public interface IExtensionCollection: ICountable<Type>
  {
    /// <summary>
    /// Gets the extension of type <typeparamref name="T"/> from the collection.
    /// </summary>
    /// <typeparam name="T">The type of extension to get.</typeparam>
    /// <returns>The extension of type <typeparamref name="T"/>, if exists;
    /// otherwise, <see langword="null"/>.</returns>
    T Get<T>()
      where T : class;

    /// <summary>
    /// Gets the extension of type <paramref name="extensionType"/> from the collection.
    /// </summary>
    /// <param name="extensionType">The type of extension to get.</param>
    /// <returns>The extension of type <paramref name="extensionType"/>, if exists;
    /// otherwise, <see langword="null"/>.</returns>
    object Get(Type extensionType);

    /// <summary>
    /// Adds \ replaces the extension of type <typeparamref name="T"/> in the collection.
    /// </summary>
    /// <typeparam name="T">The type of extension to add \ replace.</typeparam>
    /// <param name="value">The extension to add \ replace.</param>
    void Set<T>(T value)
      where T : class;

    /// <summary>
    /// Adds \ replaces the extension of type <paramref name="extensionType"/> in the collection.
    /// </summary>
    /// <param name="extensionType">The type of extension to add \ replace.</param>
    /// <param name="value">The extension to add \ replace.</param>
    void Set(Type extensionType, object value);

    /// <summary>
    /// Clears the collection.
    /// </summary>
    void Clear();
  }
}