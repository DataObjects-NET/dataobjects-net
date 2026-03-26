// Copyright (C) 2007-2024 Xtensive LLC.
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
    public struct Enumerator : IEnumerator<Column>, IEnumerator
    {
      private readonly IReadOnlyList<Column> list;
      private int index;
      public Column Current { get; private set; }

      object IEnumerator.Current =>
        index == 0 || index == list.Count + 1
          ? throw new InvalidOperationException("Enumeration cannot happen")
          : Current;

      public void Dispose()
      {
      }

      public bool MoveNext()
      {
        if (index < list.Count) {
          Current = list[index++];
          return true;
        }
        index = list.Count + 1;
        Current = default;
        return false;
      }

      void IEnumerator.Reset()
      {
        index = 0;
        Current = default;
      }

      internal Enumerator(IReadOnlyList<Column> list)
      {
        this.list = list;
        index = 0;
        Current = default;
      }
    }

    private readonly Dictionary<string, int> nameIndex;
    private readonly IReadOnlyList<Column> columns;

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
    public Column this[string fullName] =>
      nameIndex.TryGetValue(fullName, out var index) ? columns[index] : null;

    /// <summary>
    /// Determines whether the collecton contains specified column
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public bool Contains(Column column)
    {
      if (columns is ICollection<Column> colColumns)
        return colColumns.Contains(column);
      return columns.Contains(column);
    }

    /// <summary>
    /// Joins this collection with specified the column collection.
    /// </summary>
    /// <param name="joined">The joined.</param>
    /// <returns>The joined collection.</returns>
    public ColumnCollection Join(IEnumerable<Column> joined)
    {
      return new ColumnCollection(this.Concat(joined).ToList());
    }

    /// <summary>
    /// Aliases the specified <see cref="Column"/> collection.
    /// </summary>
    /// <param name="alias">The alias to add.</param>
    /// <returns>Aliased collection of columns.</returns>
    public ColumnCollection Alias(string alias)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new ColumnCollection(this.Select(column => column.Clone(alias + "." + column.Name)).ToArray(Count));
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see href="ColumnCollection"/>.
    /// </summary>
    public Enumerator GetEnumerator() => new Enumerator(this);

    /// <summary>
    /// Returns an enumerator that iterates through the <see href="ColumnCollection"/>.
    /// </summary>
    IEnumerator<Column> IEnumerable<Column>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="columns">Collection of items to add.</param>
    /// <remarks>
    /// <paramref name="columns"/> is used to initialize inner field directly
    /// to save time on avoiding collection copy. If you pass an <see cref="IReadOnlyList{Column}"/>
    /// implementor that, in fact, can be changed, make sure the passed collection doesn't change afterwards.
    /// Ideally, use arrays instead of <see cref="List{T}"/> or similar collections.
    /// Changing the passed collection afterwards will lead to unpredictable results.
    /// </remarks>
    public ColumnCollection(IReadOnlyList<Column> columns)
    {
      //!!! Direct initialization by parameter is unsafe performance optimization: See remarks in ctor summary!
      this.columns = columns;
      var count = this.columns.Count;
      nameIndex = new Dictionary<string, int>(count);
      for (var index = count; index-- > 0;) {
        nameIndex.Add(this.columns[index].Name, index);
      }
    }
  }
}
