// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Column of the record.
  /// </summary>
  [Serializable]
  public sealed class MappedColumn : Column
  {
    private const string ToStringFormat = "{0} = {1}";

    /// <summary>
    /// Gets the reference that describes a column.
    /// </summary>    
    public Model.ColumnInfoRef ColumnInfoRef { get; private set; }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public bool Equals(MappedColumn column)
    {
      if (column==null)
        return false;
      if (ColumnInfoRef==null) {
        if (!Equals(Name, column.Name))
          return false;
      }
      else if (!ColumnInfoRef.Equals(column.ColumnInfoRef))
        return false;

      if (!Equals(Type, column.Type))
        return false;

      return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as MappedColumn);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = ColumnInfoRef!=null ? ColumnInfoRef.GetHashCode() : 0;
      if (result==0)
        result = 29 * result + Name.GetHashCode();
      result = 29 * result + Type.GetHashCode();
      return result;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, base.ToString(), ColumnInfoRef);
    }


    // Constructors

    #region Basic constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name"><see cref="Column.Name"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    /// <param name="type"><see cref="Column.Type"/> property value.</param>
    public MappedColumn(string name, int index, Type type)
      : this(null, name, index, type)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="columnInfoRef"><see cref="ColumnInfoRef"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    /// <param name="type"><see cref="Column.Type"/> property value.</param>
    public MappedColumn(ColumnInfoRef columnInfoRef, int index, Type type)
      : this(columnInfoRef, columnInfoRef.ColumnName, index, type)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="columnInfoRef"><see cref="ColumnInfoRef"/> property value.</param>
    /// <param name="name"><see cref="Column.Name"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    /// <param name="type"><see cref="Column.Type"/> property value.</param>
    public MappedColumn(ColumnInfoRef columnInfoRef, string name, int index, Type type)
      : base(name, index, type)
    {
      ColumnInfoRef = columnInfoRef;
    }

    #endregion

    #region Origin-based constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The original <see cref="Column"/>.</param>
    /// <param name="alias">The alias to add.</param>
    public MappedColumn(Column column, string alias)
      : base(alias.IsNullOrEmpty() ? column.Name : string.Concat(alias, ".", column.Name), column.Index, column.Type)
    {
      ColumnInfoRef = ((MappedColumn) column).ColumnInfoRef;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="column">The original <see cref="Column"/> property value.</param>
    /// <param name="index"><see cref="Column.Index"/> property value.</param>
    public MappedColumn(Column column, int index)
      : base(column.Name, index, column.Type)
    {
      ColumnInfoRef = ((MappedColumn) column).ColumnInfoRef;
    }

    #endregion
  }
}