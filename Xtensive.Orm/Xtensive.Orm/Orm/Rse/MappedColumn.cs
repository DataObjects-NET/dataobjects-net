// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Mapped column of the <see cref="RecordSetHeader"/>.
  /// </summary>
  [Serializable]
  public sealed class MappedColumn : Column
  {
    private const string ToStringFormat = "{0} = {1}";

    /// <summary>
    /// Gets the reference that describes a column.
    /// </summary>    
    public ColumnInfoRef ColumnInfoRef { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, base.ToString(), ColumnInfoRef);
    }

    /// <inheritdoc/>
    public override Column Clone(int newIndex)
    {
      return new MappedColumn(ColumnInfoRef, Name, newIndex, Type);
    }

    /// <inheritdoc/>
    public override Column Clone(string newName)
    {
      return new MappedColumn(this, newName);
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

    #region Clone constructors

    private MappedColumn(MappedColumn column, string newName)
      : base(newName, column.Index, column.Type)
    {
      ColumnInfoRef = column.ColumnInfoRef;
    }

    private MappedColumn(MappedColumn column, int newIndex)
      : base(column.Name, newIndex, column.Type)
    {
      ColumnInfoRef = column.ColumnInfoRef;
    }

    #endregion
  }
}