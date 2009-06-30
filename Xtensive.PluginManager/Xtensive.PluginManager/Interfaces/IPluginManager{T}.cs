// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.07

using System;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Defines an interface for object that is capable for finding and loading plugins by their <see cref="Type"/> and <see cref="Attribute"/>s.
  /// </summary>
  /// <typeparam name="T">The type of attribute.</typeparam>
  public interface IPluginManager<T>
    where T: Attribute
  {
    /// <summary>
    /// Gets the base plugin type.
    /// </summary>
    /// <value>The base plugin type.</value>
    Type PluginType { get; }

    /// <summary>
    /// Gets the type of the plugin attribute.
    /// </summary>
    /// <value>The type of the attribute.</value>
    Type AttributeType { get; }

    /// <summary>
    /// Gets the path to search for plugins.
    /// </summary>
    /// <value>The path.</value>
    string SearchPath { get; }

    /// <summary>
    /// Determines whether plugin with specified attribute exists.
    /// </summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns><see langword="true"/> if a plugin <see cref="Type"/> with specified attribute exists, otherwise <see langword="false"/>.</returns>
    bool Exists(T attribute);

    /// <summary>
    /// Gets the plugin <see cref="Type"/> by specified attribute.
    /// </summary>
    /// <param name="attribute">The plugin attribute.</param>
    /// <returns>The plugin <see cref="Type"/> if exists, otherwise <see langword="null"/>.</returns>
    Type this[T attribute] { get; }

    // /// <summary>
    // /// Gets the array of plugin types.
    // /// </summary>
    // /// <returns>The array of plugin types.</returns>
    // Type[] GetPlugins();
  }
}