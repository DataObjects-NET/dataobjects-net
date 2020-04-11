// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;


namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents collection of <see cref="SqlColumn"/>s.
  /// </summary>
  [Serializable]
  public class SqlColumnCollection : ICollection<SqlColumn>, IReadOnlyList<SqlColumn>
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private readonly List<SqlColumn> columnList;

    /// <summary>
    /// Gets the number of elements contained in the <see cref="SqlColumnCollection"/>.
    /// </summary>
    public int Count => columnList.Count;

    /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>>
    bool ICollection<SqlColumn>.IsReadOnly => false;

    /// <inheritdoc cref="IEnumerable.GetEnumerator"/>>
    IEnumerator IEnumerable.GetEnumerator() => columnList.GetEnumerator();

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>>
    IEnumerator<SqlColumn> IEnumerable<SqlColumn>.GetEnumerator() => columnList.GetEnumerator();

    /// <summary>
    /// Returns a <see cref="List{T}.Enumerator"/> that iterates through the <see cref="SqlColumnCollection"/>.
    /// </summary>
    public List<SqlColumn>.Enumerator GetEnumerator() => columnList.GetEnumerator();

    /// <summary>
    /// Gets the column at the specified <paramref name="index"/>.
    /// </summary>
    public SqlColumn this[int index] => columnList[index];

    /// <summary>
    /// Gets the column with the specified <paramref name="name"/>
    /// or <see langword="null"/> if collection doesn't contain such a column.
    /// </summary>
    public SqlColumn this[string name] =>
      string.IsNullOrEmpty(name) ? null : columnList.Find(column => Comparer.Equals(column.Name, name));

    /// <summary>
    /// Adds a specified <paramref name="column"/> to the end of the <see cref="SqlColumnCollection"/>.
    /// </summary>
    public void Add(SqlColumn column) => columnList.Add(column);

    /// <summary>
    /// Builds a <see cref="SqlColumnRef"/> to the specified <paramref name="column"/> using
    /// the provided <paramref name="alias"/> and then adds it to the end of the <see cref="SqlColumnCollection"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="alias"/> is null.</exception>
    public void Add(SqlColumn column, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(alias, nameof(alias));
      columnList.Add(SqlDml.ColumnRef(column, alias));
    }

    /// <summary>
    /// Builds a <see cref="SqlColumnRef"/> by the specified <paramref name="expression"/> and
    /// then adds it to the end of the <see cref="SqlColumnCollection"/>.
    /// </summary>
    public void Add(SqlExpression expression) =>
      columnList.Add(expression is SqlColumn column ? column : SqlDml.ColumnRef(SqlDml.Column(expression)));

    /// <summary>
    /// Builds a <see cref="SqlColumnRef"/> by the specified <paramref name="expression"/> and
    /// <paramref name="alias"/>; then adds it to the end of the <see cref="SqlColumnCollection"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="alias"/> is <see langword="null"/>.</exception>
    public void Add(SqlExpression expression, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(alias, nameof(alias));
      columnList.Add(SqlDml.ColumnRef(SqlDml.Column(expression), alias));
    }

    /// <summary>
    /// Builds a <see cref="SqlColumnRef"/> by the specified <paramref name="expression"/> and <paramref name="alias"/>
    /// then inserts it into <see cref="SqlColumnCollection"/> at the specified <paramref name="index"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="alias"/> is <see langword="null"/>.</exception>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than <c>0</c>.
    /// -or- <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
    public void Insert(int index, SqlExpression expression, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(alias, nameof(alias));
      columnList.Insert(index, SqlDml.ColumnRef(SqlDml.Column(expression), alias));
    }

    /// <summary>
    /// Adds <paramref name="columns"/> to the end of the <see cref="SqlColumnCollection"/>.
    /// </summary>
    /// <param name="columns">Columns to be added.</param>
    public void AddRange(params SqlColumn[] columns)
    {
      ArgumentValidator.EnsureArgumentNotNull(columns, nameof(columns));
      columnList.AddRange(columns);
    }

    /// <summary>
    /// Adds <paramref name="columns"/> to the end of the <see cref="SqlColumnCollection"/>.
    /// </summary>
    /// <param name="columns">Columns to be added.</param>
    /// <typeparam name="TColumn">A type of the columns in the specified <paramref name="columns"/>
    /// collection; it must be <see cref="SqlColumn"/> or its inheritor.</typeparam>
    public void AddRange<TColumn>(IEnumerable<TColumn> columns) where TColumn : SqlColumn =>
      columnList.AddRange(columns);

    /// <inheritdoc cref="ICollection{T}.Contains"/>
    public bool Contains(SqlColumn item) => columnList.Contains(item);

    /// <inheritdoc cref="ICollection{T}.CopyTo"/>
    public void CopyTo(SqlColumn[] array, int arrayIndex) => columnList.CopyTo(array, arrayIndex);

    /// <inheritdoc cref="ICollection{T}.Remove"/>
    public bool Remove(SqlColumn item) => columnList.Remove(item);

    /// <inheritdoc cref="ICollection{T}.Clear"/>
    public void Clear() => columnList.Clear();

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public SqlColumnCollection()
    {
      columnList = new List<SqlColumn>();
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public SqlColumnCollection(IReadOnlyList<SqlColumn> list)
    {
      columnList = new List<SqlColumn>(list);
    }
  }
}