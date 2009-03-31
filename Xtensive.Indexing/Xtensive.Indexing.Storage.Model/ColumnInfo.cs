// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single column.
  /// </summary>
  [Serializable]
  public class ColumnInfo: NodeBase<TableInfo>
  {
    private TypeInfo columnType;
    private bool allowNulls;

    /// <summary>
    /// Gets or sets the type of the column.
    /// </summary>
    [Property]
    public TypeInfo ColumnType
    {
      get { return columnType; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("ColumnType", value)) {
          columnType = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a column allow <see langword="null"/> values.
    /// </summary>
    [Property]
    public bool AllowNulls
    {
      get{ return allowNulls;}
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("AllowNulls", value)) {
          allowNulls = value;
          scope.Commit();
        }
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ColumnInfo, TableInfo, ColumnInfoCollection>(this, "Columns");
    }

    /// <inheritdoc/>
    /// <exception cref="IntegrityException">Validation error.</exception>
    protected override void ValidateState()
    {
      base.ValidateState();

      if (ColumnType==null)
        throw new IntegrityException(
          string.Format(Resources.Strings.ExTypeOfColumnXDoesNotDefined, Name),
          Path);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index">The parent primary index.</param>
    /// <param name="name">The column name.</param>
    /// <param name="columnType">Type of the column.</param>
    public ColumnInfo(TableInfo index, string name, TypeInfo columnType)
      : this(index, name)
    {
      ColumnType = columnType;
    }

    /// <inheritdoc/>
    public ColumnInfo(TableInfo index, string name)
      : base(index, name)
    {
    }
  }
}