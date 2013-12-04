// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represent a <see cref="Index"/> bound column.
  /// </summary>
  [Serializable]
  public class IndexColumn : Node, IPairedNode<Index>
  {
    private DataTableColumn column;
    private bool ascending;
    private Index index;
    private SqlExpression expression;
    private readonly NodeCollection<Language> languages = new NodeCollection<Language>();
    private DataTableColumn typeColumn;

    /// <summary>
    /// Gets or sets the index.
    /// </summary>
    /// <value>The index.</value>
    public Index Index
    {
      get { return index; }
      set {
        this.EnsureNotLocked();
        if (index == value)
          return;
        if (index!=null)
          index.Columns.Remove(this);
        index = null;
        if (value!=null)
          value.Columns.Add(this);
      }
    }

    /// <summary>
    /// Gets or sets the column.
    /// </summary>
    /// <value>The column.</value>
    public DataTableColumn Column
    {
      get { return column; }
      set
      {
        this.EnsureNotLocked();
        column = value;
      }
    }

    /// <summary>
    /// Gets or sets column that contains type of data for <see cref="IndexColumn.Column"/>.
    /// </summary>
    public DataTableColumn TypeColumn
    {
      get { return typeColumn; }
      set
      {
        this.EnsureNotLocked();
        typeColumn = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating the ascending or descending sort direction for this instance.
    ///  The default is <see langword="true"/>.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if ascending; otherwise, <see langword="false"/>.
    /// </value>
    public bool Ascending
    {
      get { return ascending; }
      set
      {
        this.EnsureNotLocked();
        ascending = value;
      }
    }

    /// <summary>
    /// Gets or sets a name of the node.
    /// </summary>
    /// <value></value>
    public override string Name
    {
      get { return !expression.IsNullReference() ? string.Empty : column.Name; }
      set { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Gets or sets the expression.
    /// </summary>
    public SqlExpression Expression
    {
      get { return expression; }
      set
      {
        this.EnsureNotLocked();
        expression = value;
      }
    }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public NodeCollection<Language> Languages
    {
      get { return languages; }
    }

    #region ILockable Members
    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      languages.Lock(recursive);
      base.Lock(recursive);
    }
    #endregion
  
    #region IPairedNode<Index> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<Index>.UpdatePairedProperty(string property, Index value)
    {
      this.EnsureNotLocked();
      index = value;
    }

    #endregion

    #region Constructors

    internal IndexColumn(Index index, DataTableColumn column, bool ascending)
    {
      Column = column;
      Index = index;
      this.ascending = ascending;
    }

    internal IndexColumn(Index index, SqlExpression expression, bool ascending)
    {
      this.expression = expression;
      Index = index;
      this.ascending = ascending;
    }

    #endregion
  }
}