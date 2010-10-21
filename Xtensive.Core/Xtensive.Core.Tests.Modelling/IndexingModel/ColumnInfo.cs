// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Core.Tests.Modelling.IndexingModel.Resources;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Core.Tests.Modelling.IndexingModel
{
  /// <summary>
  /// Column.
  /// </summary>
  [Serializable]
  public sealed class ColumnInfo: NodeBase<TableInfo>
  {
    private TypeInfo type;

    /// <summary>
    /// Gets or sets the type of the column.
    /// </summary>
    [Property(Priority = -1000, IsImmutable = true)]
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

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Validation error.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        if (Type==null) {
          ea.Execute(() => {
            throw new ValidationException(
              string.Format(Strings.ExUndefinedTypeOfColumnX, Name),
              Path);
          });
        }
        ea.Complete();
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ColumnInfo, TableInfo, ColumnInfoCollection>(this, "Columns");
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The name.</param>
    public ColumnInfo(TableInfo table, string name)
      : base(table, name)
    {
    }

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
  }
}