// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Tests.IndexingModel.Resources;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Column.
  /// </summary>
  [Serializable]
  public class ColumnInfo: NodeBase<TableInfo>
  {
    private TypeInfo type;
    private bool isNullable;

    /// <summary>
    /// Gets or sets the type of the column.
    /// </summary>
    [Property]
    public TypeInfo Type {
      get { return type; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Type", value)) {
          type = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether 
    /// a column allow <see langword="null"/> values.
    /// </summary>
    [Property]
    public bool IsNullable {
      get { return isNullable;}
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("IsNullable", value)) {
          isNullable = value;
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

      if (Type==null)
        throw new IntegrityException(
          string.Format(Strings.ExUndefinedTypeOfColumnX, Name),
          Path);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The column name.</param>
    /// <param name="columnType">Type of the column.</param>
    public ColumnInfo(TableInfo table, string name, TypeInfo columnType)
      : this(table, name)
    {
      Type = columnType;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The name.</param>
    public ColumnInfo(TableInfo table, string name)
      : base(table, name)
    {
    }
  }
}