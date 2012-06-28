// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.13

using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// A configuration described by a set of named configurations 
  /// (<typeparamref name="TItem"/>s) contract.
  /// </summary>
  /// <remarks>
  /// <para id="Ctor"><see cref="ParameterlessCtorClassDocTemplate" copy="true" /></para>
  /// </remarks>
  /// <typeparam name="TItem">Type of particular named configuration.</typeparam>
  public interface IConfigurationSet<TItem> : IConfiguration,
    ICountable<TItem>, 
    IEnumerable<Pair<string, TItem>>
  {
    /// <summary>
    /// Gets the item by the specified name.
    /// </summary>
    /// <param name="name">Name of the item to get.</param>
    /// <value><typeparamref name="TItem"/> instance or null.</value>
    TItem this[string name] { get; }

    /// <summary>
    /// Gets the item by the specified index.
    /// </summary>
    /// <param name="index">Index of the item to get.</param>
    /// <value><typeparamref name="TItem"/> instance or null.</value>
    TItem this[int index] { get; }

    /// <summary>
    /// Determines the index of a specified item in this instance.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The index of value if found in this instance; otherwise, <see langword="-1"/>.</returns>
    int IndexOf(TItem item);

    /// <summary>
    /// Adds the specified item into this instance.
    /// </summary>
    /// <param name="item">The item.</param>
    void Add(TItem item);

    /// <summary>
    /// Removes the specified item from this instance.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    void Remove(string name);

    /// <summary>
    /// Removes all the items from this instance.
    /// </summary>
    void Clear();
  }
}