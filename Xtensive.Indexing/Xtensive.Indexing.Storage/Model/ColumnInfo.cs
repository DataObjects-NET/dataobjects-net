// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;
using Xtensive.Core.Helpers;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single column.
  /// </summary>
  [Serializable]
  public class ColumnInfo: NodeBase<PrimaryIndexInfo>
  {
    private Type columnType;

    /// <summary>
    /// Gets or sets the type of the column.
    /// </summary>
    [Property]
    public Type ColumnType
    {
      get { return columnType; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogChange("ColumnType", value)) {
          columnType = value;
          scope.Commit();
        }
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ColumnInfo, PrimaryIndexInfo, ColumnInfoCollection>(this, "Columns");
    }

    /// <inheritdoc/>
    protected override void ValidateState()
    {
      base.ValidateState();

      if (ColumnType==null)
        throw new IntegrityException(
          string.Format("Type of column {0} does not defined.", Name),
          Path);
    }

    //Constructors

    public ColumnInfo(PrimaryIndexInfo index, string name, Type columnType)
      : this(index, name)
    {
      ColumnType = columnType;
    }

    public ColumnInfo(PrimaryIndexInfo index, string name)
      : base(index, name)
    {
    }
  }
}