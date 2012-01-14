// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Collection of <see cref="Column"/> items.
  /// </summary>
  [Serializable]
  public sealed class ColumnCollection : ReadOnlyList<Column>
  {
    private readonly Dictionary<string, int> nameIndex = new Dictionary<string, int>();

    /// <summary>
    /// Gets <see cref="Column"/> by provided <paramref name="fullName"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Column"/> if it was found; otherwise <see langword="null"/>.
    /// </remarks>
    /// <param name="fullName">Full name of the <see cref="Column"/> to find.</param>
    public Column this[string fullName] {
      get {
        int index;
        if (nameIndex.TryGetValue(fullName, out index))
          return this[index];
        return null;
      }
    }

    private void Initialize()
    {
      for (int index = 0; index < Count; index++) 
        nameIndex.Add(this[index].Name, index);
    }

    /// <summary>
    /// Joins this collection with specified the column collection.
    /// </summary>
    /// <param name="joined">The joined.</param>
    /// <returns>The joined collection.</returns>
    public ColumnCollection Join(IEnumerable<Column> joined)
    {
      return new ColumnCollection(this.Concat(joined));
    }

    /// <summary>
    /// Aliases the specified <see cref="Column"/> collection.
    /// </summary>
    /// <param name="alias">The alias to add.</param>
    /// <returns>Aliased collection of columns.</returns>
    public ColumnCollection Alias(string alias)
    {      
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new ColumnCollection(this.Select(column => column.Clone(alias + "." + column.Name)));
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="collection">Collection of items to add.</param>
    public ColumnCollection(IEnumerable<Column> collection)
      : base (collection.ToList())
    {
      Initialize();
    }
  }
}