// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Collections.Generic;
using System.Configuration;

namespace Xtensive.Configuration
{
  /// <summary>
  /// A typed version of <see cref="ConfigurationElementCollection"/>.
  /// </summary>
  /// <typeparam name="T">The type of the collection item.</typeparam>
  public class ConfigurationCollection<T> : ConfigurationElementCollection,
    IEnumerable<T>
    where T : ConfigurationCollectionElementBase, new()
  {
    /// <inheritdoc/>
    protected override ConfigurationElement CreateNewElement()
    { 
      return new T();
    }

    /// <inheritdoc/>
    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((ConfigurationCollectionElementBase) element).Identifier;
    }

    /// <summary>
    /// Gets the element by specified identifier.
    /// </summary>
    public new T this[string identifier] {
      get {
        foreach (var element in this)
          if (element.Identifier.Equals(identifier))
            return element;
        return null;
      }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public new IEnumerator<T> GetEnumerator()
    {
      foreach (object element in (ConfigurationElementCollection) this)
        yield return (T) element;
    }
  }
}