// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2007.09.24

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;


namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Collection of <see cref="Column"/> items.
  /// </summary>
  [Serializable]
  public sealed class ColumnCollection : IReadOnlyList<Column>
  {
    private readonly Dictionary<string, int> nameIndex;
    private readonly List<Column> columns;

    /// <summary>
    /// Gets the number of <see href="Column"/>s in the collection.
    /// </summary>
    public int Count => columns.Count;

    /// <summary>
    /// Gets a <see href="Column"/> instance by its index.
    /// </summary>
    public Column this[int index] => columns[index];

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

    /// <summary>
    /// Returns an enumerator that iterates through the <see href="ColumnCollection"/>.
    /// </summary>
    public List<Column>.Enumerator GetEnumerator() => columns.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<Column> IEnumerable<Column>.GetEnumerator() => GetEnumerator();
    
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="collection">Collection of items to add.</param>
    public ColumnCollection(IEnumerable<Column> collection)
      : this(collection.ToList())
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="columns">Collection of items to add.</param>
    public ColumnCollection(List<Column> columns)
    {
      this.columns = columns;
      var count = columns.Count;
      nameIndex = new Dictionary<string, int>(count);
      for (var index = count; index-- > 0;) {
        nameIndex.Add(columns[index].Name, index);
      }
    }
  }
}
